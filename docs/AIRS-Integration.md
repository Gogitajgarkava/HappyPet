# AIRS Integration

This document describes the AIRS microchip validation that is implemented in HappyPet, plus the development mock used until the real AIRS API is available.

## Status

- Implemented: AIRS HTTP client, microchip lookup, owner check, pet create/update validation, AIRS lookup endpoints, Dockerized mock, marketplace ownership-transfer queue and local owner write-back after an operator confirms the AIRS change.
- Not implemented: real AIRS production integration, official AIRS authentication, AIRS synchronization jobs, automatic AIRS write-back.

## Operator ownership-transfer workflow

A marketplace completion creates an `OwnershipTransfer` with `Status = PendingAirsSync`. The actual change of owner is performed manually in the AIRS web portal by a `Veterinarian` or `Admin` operator.

| Method | Path | Purpose |
| --- | --- | --- |
| GET | `/api/v1/operator/dashboard` | Counts of pending, synced and rejected transfers |
| GET | `/api/v1/operator/transfers?status=PendingAirsSync` | Operator queue with optional search and pagination |
| GET | `/api/v1/operator/transfers/{id}` | Transfer details |
| POST | `/api/v1/operator/transfers/{id}/mark-synced` | Store the AIRS reference and update the local pet owner |
| POST | `/api/v1/operator/transfers/{id}/reject` | Close the transfer with a reason |

`mark-synced` requires an `airsReference`. For a transfer linked to a registered pet, the buyer must have a HappyPet account (`buyerUserId` in the transfer or supplied by the operator). If the transfer has no linked account, the API tries to match the buyer by personal ID. On success:

- the transfer becomes `Synced`;
- `airsReference`, `airsSyncedAt`, `processedByUserId`, `processedAt` and operator notes are stored;
- `Pet.OwnerId` is changed to the buyer in the same database transaction.

A standalone listing (newborn pet without a registered pet) can be marked synced without a buyer account. Rejecting a transfer does not change the pet owner. Only `PendingAirsSync` transfers can be processed.

AIRS ownership changes are still manual because the official AIRS write API is not available. The operator must complete the change in `https://airs.nfa.gov.ge` first and then confirm it in HappyPet.

## Configuration

| Key | Required | Description |
| --- | --- | --- |
| `Airs:BaseUrl` | yes | AIRS base URL, for example `http://localhost:9100` |
| `Airs:ApiKey` | no | Sent as `X-Api-Key` |
| `Airs:TimeoutSeconds` | no | HTTP timeout, default `10` |

The API fails at startup if `Airs:BaseUrl` is missing.

## HappyPet endpoints

Both endpoints require a Bearer access token.

### Get AIRS animal and vaccinations

```http
GET /api/v1/airs/animals/by-microchip/{microchip}
```

Success:

```json
{
  "animal": {
    "id": "animal-000001",
    "microchipNumber": "982000000000001",
    "name": "შერი",
    "species": "Dog",
    "breed": "German Shepherd",
    "gender": "Female",
    "birthDate": "2013-09-18",
    "color": "თეთრი",
    "distinguishingMarks": "",
    "status": "Active",
    "owner": {
      "personalId": "01000000001",
      "firstName": "გიორგი",
      "lastName": "ჯღარკავა",
      "phone": "+995510000000",
      "email": "owner1@example.com",
      "address": "ბათუმი, ჭავჭავაძის გამზირი 94"
    }
  },
  "vaccinations": []
}
```

Not found: `404` with `AIRS_MICROCHIP_NOT_FOUND`.

AIRS unavailable: `503` with `AIRS_UNAVAILABLE`.

### Get AIRS vaccinations

```http
GET /api/v1/airs/vaccinations/{microchip}
```

Returns the vaccination array for the animal.

## Pet create and update validation

`POST /api/v1/pet/add-pet` and `PUT /api/v1/pet/{id}` validate the microchip against AIRS before writing to the HappyPet database.

1. The local HappyPet duplicate check runs first.
2. If the microchip is non-empty, AIRS must return an animal.
3. The AIRS owner `personalId` must match the authenticated user's `PersonalId`.
4. AIRS must be reachable. If not, the pet is not written.
5. During update, an unchanged microchip is not checked again.

An empty microchip on create skips AIRS validation. Update requires a microchip.

## Error responses

All errors use the shared envelope `{ "code", "message", "traceId" }` (see the README API conventions). Unexpected `500` responses never expose internal exception details.

| Situation | HTTP | Code |
| --- | --- | --- |
| Microchip not found in AIRS | `400` | `AIRS_MICROCHIP_NOT_FOUND` |
| Microchip belongs to another owner | `400` | `AIRS_OWNER_MISMATCH` |
| AIRS unavailable or timed out | `503` | `AIRS_UNAVAILABLE` |
| Microchip already in HappyPet | `400` | existing duplicate message |

Example:

```json
{
  "code": "AIRS_OWNER_MISMATCH",
  "message": "მითითებული მიკროჩიპი AIRS სისტემაში სხვა მფლობელზეა რეგისტრირებული. ცხოველის რეგისტრაცია შეუძლებელია."
}
```

## Development mock

Location: `airs-mock/` in the repository root. It runs as a standalone container in its own Compose project: `docker compose -f airs-mock/docker-compose.yml up -d --build`. The main stack reaches it through `host.docker.internal:9100`.

The mock exposes:

| Method | Path | Purpose |
| --- | --- | --- |
| GET | `/api/v1/airs/animals/by-microchip/{microchip}` | animal + vaccinations |
| GET | `/api/v1/airs/animals` | list/search |
| GET | `/api/v1/airs/animals/{id}/vaccinations` | vaccinations |
| POST | `/api/v1/airs/animals` | register animal |
| POST | `/api/v1/airs/vaccinations` | add vaccination |

Seed data: 1,000 animals, about 2,500 vaccinations.

Useful test values:

| Value | Meaning |
| --- | --- |
| `982000000000001` | owner `01000000001` |
| `982000000000002` | owner `01000000002` |
| any other 15-digit number | not found |

Swagger: `http://localhost:9100/swagger`

The mock is development-only. Real AIRS integration remains blocked until official API access and documentation are available.
