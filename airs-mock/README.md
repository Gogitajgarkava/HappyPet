# HappyPet AIRS Mock

Development-only mock of the Georgian AIRS animal registry. It stores animals and vaccinations in JSON files and exposes the endpoints HappyPet needs before the real AIRS API is available.

## Features

- 1,000 seeded animals and about 2,500 vaccinations.
- Lookup by 15-digit microchip.
- Animal details plus vaccinations in one response.
- Owner personal ID is included so the HappyPet backend can reject a microchip registered to another owner.
- Animal registration and vaccination creation persist to JSON.
- Swagger UI at `/swagger`.
- Optional API key via `X-Api-Key`.

## Run locally

```powershell
cd D:\HappyPet\airs-mock
npm install
npm run seed
npm run dev
```

The service listens on `http://localhost:9100`.

## Run with Docker

The mock is a standalone container with its own Compose project. From the repository root (`D:\HappyPet`):

```powershell
docker compose -f airs-mock/docker-compose.yml up -d --build
```

The main HappyPet stack does not include the mock. Its API container reaches the mock through `host.docker.internal:9100`.

Swagger: `http://localhost:9100/swagger`

## Seed data

- `seed/airs-animals.seed.json`
- `seed/airs-vaccinations.seed.json`

On first start the server copies the seed files to `data/`. Later changes are stored in `data/airs-animals.json` and `data/airs-vaccinations.json`. Docker keeps this directory in the `happypet-airs_airs-data` volume.

To reset the mock data, remove the container together with its volume and start again:

```powershell
docker compose -f airs-mock/docker-compose.yml down -v
docker compose -f airs-mock/docker-compose.yml up -d --build
```

## Useful test values

| Value | Meaning |
| --- | --- |
| `982000000000001` | seeded animal, owner `01000000001` |
| `982000000000002` | seeded animal, owner `01000000002` |
| any other 15-digit number | not found |

The first seeded animal belongs to personal ID `01000000001`. A HappyPet user with that personal ID passes the owner check; a user with another personal ID gets `AIRS_OWNER_MISMATCH`.

## Endpoints

| Method | Path | Purpose |
| --- | --- | --- |
| GET | `/health` | health and record counts |
| GET | `/api/v1/airs/animals` | list/search animals |
| GET | `/api/v1/airs/animals/by-microchip/{microchip}` | animal + vaccinations |
| GET | `/api/v1/airs/animals/{id}` | animal + vaccinations |
| GET | `/api/v1/airs/animals/{id}/vaccinations` | vaccinations by AIRS id |
| GET | `/api/v1/airs/vaccinations?microchip=...` | vaccinations by microchip |
| POST | `/api/v1/airs/animals` | register animal |
| PUT | `/api/v1/airs/animals/{id}` | update animal |
| POST | `/api/v1/airs/animals/{id}/status` | update animal status |
| POST | `/api/v1/airs/vaccinations` | add vaccination |

If `AIRS_API_KEY` is set, requests to `/api/v1/airs/*` need:

```http
X-Api-Key: dev-airs-key
```

## Not found response

```json
{
  "code": "AIRS_ANIMAL_NOT_FOUND",
  "message": "ცხოველი მითითებული მიკროჩიპით AIRS სისტემაში ვერ მოიძებნა."
}
```

This is a development mock, not a production AIRS implementation.
