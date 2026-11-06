# AIRS Integration

This document describes the AIRS microchip validation that is implemented in HappyPet, plus the development mock used until the real AIRS API is available.

## Status

- Implemented: AIRS HTTP client, microchip lookup, owner check, pet create/update validation, AIRS lookup endpoints, Dockerized mock.
- Not implemented: real AIRS production integration, official AIRS authentication, AIRS synchronization jobs, AIRS write-back.

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
