# HappyPet

.NET 8 Clean Architecture backend with EF Core 8, PostgreSQL and JWT authentication.

## Structure

```
HappyPet.sln
docker-compose.yml
airs-mock/                # Development-only mock of the Georgian AIRS registry
src/
  HappyPet.Domain/          # Entities, value objects, domain logic (no external dependencies)
    Common/BaseEntity.cs
    Entities/User.cs
    Entities/Pet.cs
    Entities/PetPhoto.cs
    Entities/Vaccination.cs
    Entities/MarketplaceListing.cs
    Entities/MarketplaceListingPhoto.cs
    Entities/OwnershipTransfer.cs
    Enums/Enums.cs
  HappyPet.Application/     # Use cases, interfaces, DTOs, validation
    Dtos/Requests/
    Dtos/Responises/
    Dtos/Airs/
    Interfaces/
    Services/AuthService.cs
    Services/UserServices.cs
    Services/PetService.cs
    Services/VaccinationService.cs
    Services/MarketplaceService.cs
    Services/AirsService.cs
    Common/GeoDistance.cs  # Haversine distance and bounding box helpers
    Validators/            # FluentValidation rules for every request DTO
  HappyPet.Infrastructure/  # EF Core, PostgreSQL, security, S3 storage, repositories
    Airs/AirsClient.cs
    Persistence/HappyPetDbContext.cs
    Persistence/Repositories/
    Securuty/PasswordHasher.cs
    Securuty/TokenGenerator.cs
    Storage/S3FileStorageService.cs
  HappyPet.WebAPI/           # ASP.NET Core Web API (composition root)
    Controllers/AuthController.cs
    Controllers/UserControler.cs
    Controllers/PetController.cs
    Controllers/MarketplaceController.cs
    Controllers/AirsController.cs
    ErrorHandling/ApiExceptionHandler.cs
    Filters/FluentValidationActionFilter.cs
    HealthChecks/AirsHealthCheck.cs
    Program.cs
tests/
  HappyPet.Tests/            # xUnit unit tests (services, AIRS, error handling)
```

Dependency direction: `Domain <- Application <- Infrastructure <- WebAPI`.

## Getting started with Docker

The repository includes a complete local stack:

- `api` — ASP.NET Core Web API
- `postgres` — PostgreSQL 16
- `migrate` — one-time EF Core migration job
- `mailpit` — local SMTP server and mailbox UI for OTP testing
- `localstack` — local S3-compatible storage for pet photos
- `caddy` — HTTPS reverse proxy

The AIRS mock runs as a **separate container** in its own Compose project, not as a service of this stack.

1. Create the local environment file and change the secrets if needed:

   ```bash
   cp .env.example .env
   ```

2. Start the AIRS mock first (separate container):

   ```bash
   docker compose -f airs-mock/docker-compose.yml up -d --build
   ```

3. Build and start the stack:

   ```bash
   docker compose up -d --build
   ```

   The API reaches the mock through `host.docker.internal:9100`. If the mock is not running, pet creation and microchip changes fail with `503 AIRS_UNAVAILABLE`; the rest of the API keeps working.

4. Open the services:

   - API / Swagger: `https://localhost:7149/swagger`
   - Health: `https://localhost:7149/health`
   - Mailpit: `http://localhost:8025`
   - Local S3: `http://localhost:4566`
   - AIRS mock / Swagger: `http://localhost:9100/swagger`
   - PostgreSQL: `localhost:5434`

The AIRS mock seeds itself on first start and keeps its JSON data in the `happypet-airs_airs-data` volume.

LocalStack is used only for local development. Production should use real AWS S3 with CloudFront OAC.

Caddy uses its internal CA for local HTTPS. If the browser does not trust it, import the Caddy root certificate once:

```powershell
docker compose cp caddy:/data/caddy/pki/authorities/local/root.crt ./caddy-root.crt
certutil -addstore -User Root caddy-root.crt
```

> `Secure` refresh cookies require HTTPS. Always use `https://localhost:7149` and not the internal HTTP port.

Useful commands:

```bash
docker compose ps
docker compose logs -f api
docker compose down
docker compose down -v   # also removes PostgreSQL, LocalStack and Caddy data
```

The AIRS mock is managed separately:

```bash
docker compose -f airs-mock/docker-compose.yml ps
docker compose -f airs-mock/docker-compose.yml logs -f airs-mock
docker compose -f airs-mock/docker-compose.yml down
docker compose -f airs-mock/docker-compose.yml down -v   # also removes mocked AIRS data
```

## Local development without the full Docker stack

1. Start PostgreSQL and Mailpit, plus the AIRS mock in its own container:

   ```bash
   docker compose up -d postgres mailpit
   docker compose -f airs-mock/docker-compose.yml up -d --build
   ```

2. Create the local configuration file from the example and set your own database password and JWT secret:

   ```bash
   cp appsettings.example.json src/HappyPet.WebAPI/appsettings.json
   ```

   Set the connection string port to `5434` for the Docker PostgreSQL instance. Set `Airs:BaseUrl` to `http://localhost:9100` and `Airs:ApiKey` to the same value as `AIRS_API_KEY` (default `dev-airs-key`).

   > `appsettings.json` is git-ignored on purpose. Never commit real credentials.

3. Restore tools and apply migrations:

   ```bash
   dotnet tool restore
   dotnet ef database update -p src/HappyPet.Infrastructure -s src/HappyPet.WebAPI
   ```

4. Run the API:

   ```bash
   dotnet run --project src/HappyPet.WebAPI --launch-profile https
   ```

   API: `https://localhost:7149`  
   Swagger: `https://localhost:7149/swagger`

## API

All endpoints are under `/api/v1`. Registration, login, email verification, password recovery, the public lost-pet search and public marketplace listings are public; every other endpoint requires `Authorization: Bearer <token>`.

Entity identifiers are integers (`int`, PostgreSQL identity). For example: `/api/v1/pet/1`, `/api/v1/pet/1/photos/2`.

| Method | Endpoint | Description |
| --- | --- | --- |
| POST | `/api/v1/user/add-user` | Register a user and send an email-verification OTP |
| POST | `/api/v1/auth/login` | Login, receive an access token and set the refresh cookie |
| POST | `/api/v1/auth/refresh` | Rotate the refresh token and issue a new access token |
| POST | `/api/v1/auth/logout` | Revoke the current refresh token and clear its cookie |
| POST | `/api/v1/auth/logout-all` | Revoke all refresh tokens for the current user (Bearer token required) |
| POST | `/api/v1/auth/verify-email` | Verify an email with a 6-digit OTP |
| POST | `/api/v1/auth/resend-verification` | Resend the email-verification OTP |
| POST | `/api/v1/auth/forgot-password` | Send a password-reset OTP |
| POST | `/api/v1/auth/reset-password` | Reset a password with an OTP |
| GET | `/api/v1/user/{id}` | Get user by id (owner or admin) |
| GET | `/api/v1/user/{email}` | Get user by email (owner or admin) |
| PUT | `/api/v1/user/{id}` | Update profile (owner or admin) |
| DELETE | `/api/v1/user/{id}` | Delete user (owner or admin) |
| POST | `/api/v1/pet/add-pet` | Add a pet for the current user |
| GET | `/api/v1/pet/{id}` | Get pet (owner or admin) |
| GET | `/api/v1/pet/owner/{ownerId}` | List pets by owner (paginated) |
| GET | `/api/v1/pet/lost` | Public lost-pet search (paginated, optional geo filter) |
| POST | `/api/v1/pet/{id}/lost` | Mark a pet as lost with last-seen location (owner or admin) |
| POST | `/api/v1/pet/{id}/found` | Mark a lost pet as found (owner or admin) |
| POST | `/api/v1/pet/{id}/deceased` | Mark a pet as deceased (owner or admin) |
| GET | `/api/v1/pet/{id}/vaccinations` | List a pet's vaccinations (owner or admin) |
| POST | `/api/v1/pet/{id}/vaccinations` | Add a vaccination record (owner or admin) |
| PUT | `/api/v1/pet/{id}/vaccinations/{vaccinationId}` | Update a vaccination record (owner or admin) |
| DELETE | `/api/v1/pet/{id}/vaccinations/{vaccinationId}` | Delete a vaccination record (owner or admin) |
| POST | `/api/v1/pet/{id}/vaccinations/import-airs` | Import vaccinations from AIRS by microchip (owner or admin) |
| GET | `/api/v1/pet/vaccinations/due` | Vaccinations due for the current user's pets |
| PUT | `/api/v1/pet/{id}` | Update pet (owner or admin) |
| DELETE | `/api/v1/pet/{id}` | Delete pet (owner or admin) |
| GET | `/api/v1/pet/{id}/photos` | List pet photos (owner or admin) |
| POST | `/api/v1/pet/{id}/photos` | Upload a pet photo, `multipart/form-data`, max 5 MB (owner or admin) |
| PUT | `/api/v1/pet/{id}/photos/{photoId}/primary` | Set the primary pet photo (owner or admin) |
| DELETE | `/api/v1/pet/{id}/photos/{photoId}` | Delete a pet photo (owner or admin) |
| POST | `/api/v1/marketplace/listings` | Publish a pet sale or giveaway listing |
| GET | `/api/v1/marketplace/listings` | Public marketplace search (paginated) |
| GET | `/api/v1/marketplace/listings/{id}` | Public listing details |
| GET | `/api/v1/marketplace/my` | List the current user's listings (paginated) |
| PUT | `/api/v1/marketplace/listings/{id}` | Update an active listing (owner or admin) |
| DELETE | `/api/v1/marketplace/listings/{id}` | Withdraw an active listing (owner or admin) |
| POST | `/api/v1/marketplace/listings/{id}/complete` | Complete a sale/giveaway and create a pending AIRS transfer (owner or admin) |
| POST | `/api/v1/marketplace/listings/{id}/block` | Block a listing with a reason (admin only) |
| POST | `/api/v1/marketplace/listings/{id}/unblock` | Restore a blocked listing (admin only) |
| GET | `/api/v1/marketplace/listings/{id}/photos` | Public listing photos |
| POST | `/api/v1/marketplace/listings/{id}/photos` | Upload a listing photo, `multipart/form-data`, max 5 MB (owner or admin) |
| PUT | `/api/v1/marketplace/listings/{id}/photos/{photoId}/primary` | Set the primary listing photo (owner or admin) |
| DELETE | `/api/v1/marketplace/listings/{id}/photos/{photoId}` | Delete a listing photo (owner or admin) |
| GET | `/api/v1/airs/animals/by-microchip/{microchip}` | AIRS animal and vaccinations by microchip |
| GET | `/api/v1/airs/vaccinations/{microchip}` | AIRS vaccinations by microchip |

CORS is enabled for `http://localhost:3000` and `http://localhost:5173` by default, with credentials allowed for the refresh cookie. Override with `Cors:AllowedOrigins` in configuration. Frontend requests that use the refresh flow must send `credentials: 'include'`.

## API conventions

### Pagination

`GET /api/v1/pet/owner/{ownerId}` and `GET /api/v1/pet/lost` accept `page` (default `1`) and `pageSize` (default `20`, maximum `100`) and return a pagination envelope:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42,
  "totalPages": 3
}
```

Out-of-range values are normalized: `page < 1` becomes `1`, and `pageSize` is clamped to `1..100`.

### Lost-pet search

`GET /api/v1/pet/lost` is public and returns a safe projection of lost pets. It never includes the owner id, owner personal data or the pet's microchip number. Available query parameters:

| Parameter | Default | Description |
| --- | --- | --- |
| `page`, `pageSize` | `1`, `20` | Standard pagination envelope |
| `latitude`, `longitude` | — | Search origin; must be supplied together |
| `radiusKm` | `25` | Search radius in kilometres (clamped to `1..100`) |

When `latitude` and `longitude` are supplied, results are limited to pets inside the radius, ordered by distance, and each item includes `distanceKm`. The bounding box is evaluated in SQL and the exact Haversine distance is applied afterwards.

```json
{
  "items": [
    {
      "id": 7,
      "name": "შერი",
      "species": "Dog",
      "breed": "German Shepherd",
      "gender": "Female",
      "color": "თეთრი",
      "lastSeenAt": "2026-11-09T08:00:00Z",
      "lastSeenLatitude": 41.7151,
      "lastSeenLongitude": 44.8271,
      "lastSeenAddress": "ვაკე, თბილისი",
      "contactPhone": "+995510000000",
      "distanceKm": 0.42,
      "photos": []
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```

Reporting a pet as lost requires `lastSeenAt`, `latitude` and `longitude`; `lastSeenAddress` and `contactPhone` are optional. `contactPhone` is shown publicly as-is, so owners decide what to publish. Calling `/lost` again updates the last-seen details but keeps the original `lostAt`. `/found` clears `isLost`, records `foundAt` and is idempotent. A deceased pet cannot be reported as lost.

### Pet lifecycle

Pets can be marked as deceased with `POST /api/v1/pet/{id}/deceased`:

```json
{
  "deceasedAt": "2026-11-12T00:00:00Z",
  "deathCause": "ავადმყოფობა"
}
```

Both fields are optional; `deceasedAt` defaults to the current time, cannot be in the future and cannot be earlier than the pet's birth date. Death is irreversible in the API: repeated calls are idempotent and do not overwrite the first record. The response includes `isDeceased`, `deceasedAt` and `deathCause`. Pet create/update requests accept `size` (`Small`, `Medium`, `Large`), `isSterilized` and `sterilizedAt`.

### Vaccinations

Vaccination records belong to a pet and are managed by its owner (or an admin). Each record stores the vaccine name, disease, manufacturer, manufacturer country, immunity duration, vaccination type, batch and certificate numbers, administration date, next due date, veterinarian, clinic and free-form notes.

```json
{
  "vaccineName": "Rabies",
  "disease": "რაბიესი",
  "manufacturer": "MSD Animal Health",
  "manufacturerCountry": "საფრანგეთი",
  "immunityYears": 1,
  "vaccinationType": "Mandatory",
  "batchNumber": "BATCH-2026-5401",
  "certificateNumber": "CERT-2026-000001",
  "vaccinatedAt": "2026-10-07T00:00:00Z",
  "veterinarianName": "ექიმი",
  "clinicName": "კლინიკა",
  "notes": "გვერდითი ეფექტი არ დაფიქსირდა"
}
```

`vaccinatedAt` is required, cannot be in the future and, for new records, cannot be older than one year. `immunityYears` accepts `1`, `2` or `3`; `vaccinationType` accepts `Prophylactic` or `Mandatory`. When `immunityYears` is supplied, `nextDueAt` is calculated automatically as `vaccinatedAt + immunityYears − 1 month`. A client-supplied `nextDueAt` that differs from the calculated value is rejected unless the caller is an admin or veterinarian and sends `overrideNextDueDate: true`; such records are marked with `isNextDueOverridden: true`. When `immunityYears` is omitted, a supplied `nextDueAt` must be later than `vaccinatedAt`.

New vaccinations are rejected when the pet is deceased or when the previous vaccination has not yet expired. Admin and veterinarian users can bypass the previous-vaccination guard for early revaccination. AIRS imports are not subject to these local restrictions. Responses include `daysUntilDue` and `isOverdue` so a client can render reminders without extra calculations.

`GET /api/v1/pet/vaccinations/due?daysAhead=30` returns all vaccinations with `nextDueAt` up to the requested horizon for the current user's pets. `daysAhead` defaults to `30` and is clamped to `1..365`; overdue records are always included.

`POST /api/v1/pet/{id}/vaccinations/import-airs` fetches the pet's AIRS record by microchip and imports vaccinations that are not stored yet. Records are deduplicated by AIRS vaccination id, so the endpoint is safe to call repeatedly. The response reports `imported` and `skipped` counts.

### Marketplace

The marketplace supports paid sales and free giveaways of dogs, cats and other pets. A listing is standalone and does **not** require a registered pet; this is intentional so newborn puppies and kittens without a microchip can be listed. A seller may optionally link an existing pet with `petId`; the pet must belong to the seller, must be alive and must not already have an active listing.

```json
{
  "petId": null,
  "petName": "შერი",
  "species": "Dog",
  "breed": "ლაბრადორი",
  "gender": "Female",
  "birthDate": "2026-09-01T00:00:00Z",
  "color": "კრემისფერი",
  "size": "Medium",
  "isSterilized": false,
  "isVaccinated": true,
  "microchipNumber": null,
  "listingType": "Sale",
  "price": 800,
  "city": "თბილისი",
  "contactPhone": "+995555000000",
  "description": "მეგობრული ლეკვი"
}
```

Business rules:

- `listingType` is `Sale` or `Giveaway`. Prices are always in GEL; the response contains `"currency": "GEL"`.
- A `Sale` requires a price greater than `0` and no more than `1 000 000`. A `Giveaway` accepts `null` or `0`; any other price is rejected.
- `birthDate` is required and the pet must be at least **8 weeks** old at publication time.
- `microchipNumber` is optional. When supplied it must contain exactly 15 digits. When a registered pet is linked and has a microchip, the listing copies it automatically; a conflicting value is rejected.
- The listing is published immediately with `Status = Active` and expires automatically after **30 days**. Expired listings are hidden from public search and detail endpoints; a background service marks them `Expired`.
- `contactPhone` is public by design so buyers can contact the seller directly.
- Up to 5 photos can be attached, each up to 5 MB, in JPEG, PNG or WebP format. The first uploaded photo becomes primary.
- Admins can block a listing with `POST /api/v1/marketplace/listings/{id}/block`; blocked listings are hidden from the public. An admin can unblock a listing before it expires.

`GET /api/v1/marketplace/listings` is public and supports `page`, `pageSize`, `species`, `listingType`, `city`, `minPrice`, `maxPrice` and `search`. Only active, non-expired listings are returned.

Completing a listing:

```json
{
  "buyerUserId": 12,
  "buyerFullName": "ნინო ბერიძე",
  "buyerPersonalId": "01000000002",
  "buyerPhone": "+995555111111",
  "transferredAt": null,
  "notes": "შეხვედრა კლინიკაში"
}
```

`POST /api/v1/marketplace/listings/{id}/complete` is available to the seller or an admin. It marks the listing `Completed` and creates a local `OwnershipTransfer` with `Status = PendingAirsSync`. For a linked pet it records the pet id and microchip; for a standalone newborn listing the transfer still records the seller, buyer, type and price. The actual AIRS ownership change is performed later by an operator or clinic, so the local pet owner is **not** changed automatically by this endpoint.

### Request validation

Request DTOs are validated with **FluentValidation**. Validators live in `HappyPet.Application/Validators` and are executed by `FluentValidationActionFilter` before the controller runs. Model-binding failures (malformed JSON, wrong types) use the same envelope.

Validation failures return `400` with `code: "VALIDATION_ERROR"` and a per-field `errors` map:

```json
{
  "code": "VALIDATION_ERROR",
  "message": "მოთხოვნის მონაცემები არასწორია.",
  "traceId": "0HN7...",
  "errors": {
    "Email": ["არასწორი ელ-ფოსტის ფორმატი."],
    "Password": ["პაროლი უნდა იყოს მინიმუმ 6 სიმბოლო."]
  }
}
```

Validation messages are localized to Georgian. The DTOs themselves are plain transport models; all rules are kept in the validators.

### Error format

Every error response uses the same envelope and never exposes stack traces or exception details:

```json
{
  "code": "BUSINESS_RULE_VIOLATION",
  "message": "მითითებული მიკროჩიპის ნომრით ცხოველი უკვე რეგისტრირებულია.",
  "traceId": "0HN7..."
}
```

Validation errors additionally include a field map:

```json
{
  "code": "VALIDATION_ERROR",
  "message": "მოთხოვნის მონაცემები არასწორია.",
  "traceId": "0HN7...",
  "errors": {
    "Email": ["არასწორი ელ-ფოსტის ფორმატი."]
  }
}
```

| Code | HTTP | Meaning |
| --- | --- | --- |
| `VALIDATION_ERROR` | `400` | Request body or query validation failed |
| `BUSINESS_RULE_VIOLATION` | `400` | Domain rule rejected the request |
| `INVALID_ARGUMENT` | `400` | Malformed argument |
| `AIRS_MICROCHIP_NOT_FOUND` | `400` | Microchip is not registered in AIRS |
| `AIRS_OWNER_MISMATCH` | `400` | AIRS microchip belongs to another owner |
| `AIRS_UNAVAILABLE` | `503` | AIRS is unreachable or timed out |
| `UNAUTHORIZED` | `401` | Invalid credentials |
| `REFRESH_TOKEN_MISSING` / `REFRESH_TOKEN_INVALID` | `401` | Session expired or revoked |
| `EMAIL_NOT_VERIFIED` | `403` | Login blocked until the email is verified |
| `NOT_FOUND` | `404` | Requested resource does not exist |
| `INTERNAL_ERROR` | `500` | Unexpected server error; details are logged only |

### Health checks

| Endpoint | Purpose |
| --- | --- |
| `/health` | All registered checks |
| `/health/live` | Liveness only; does not touch the database |
| `/health/ready` | Readiness: PostgreSQL (unhealthy → `503`) and AIRS (degraded → `200`) |

AIRS is treated as an external, optional dependency: when it is down the API stays ready, and only pet microchip validation returns `503 AIRS_UNAVAILABLE`.

## Rate limiting

Authentication endpoints are protected per client IP with fixed-window rate limits:

| Endpoints | Policy | Default |
| --- | --- | --- |
| `POST /api/v1/auth/login` | `auth-login` | 10 requests per 5 minutes |
| `POST /api/v1/auth/verify-email` | `auth-otp` | 10 requests per 10 minutes |
| `POST /api/v1/auth/resend-verification` | `auth-otp` | 10 requests per 10 minutes |
| `POST /api/v1/auth/forgot-password` | `auth-otp` | 10 requests per 10 minutes |
| `POST /api/v1/auth/reset-password` | `auth-otp` | 10 requests per 10 minutes |

When the limit is exceeded the API returns `429` with the standard error envelope and a `Retry-After` header:

```json
{
  "code": "RATE_LIMIT_EXCEEDED",
  "message": "ძალიან ბევრი მოთხოვნა. გთხოვთ სცადოთ ცოტა ხანში.",
  "traceId": "0HN7..."
}
```

Configuration:

| Key | Default | Description |
| --- | --- | --- |
| `RateLimiting:LoginPermitLimit` | `10` | Login requests allowed per window |
| `RateLimiting:LoginWindowMinutes` | `5` | Login window length |
| `RateLimiting:OtpPermitLimit` | `10` | OTP requests allowed per window |
| `RateLimiting:OtpWindowMinutes` | `10` | OTP window length |

> Behind Caddy, `ForwardedHeaders` must be enabled (it is when `ReverseProxy:Enabled=true`) so limits are applied to the real client IP rather than the proxy.

### Tests

```bash
dotnet test
```

The xUnit suite covers AIRS validation, pet creation/update rules, lost/found flows, vaccination records, marketplace listing rules and ownership transfers, geo distance, pagination, login and refresh-token handling, OTP flows and the unified error handler. GitHub Actions runs build, tests and both Docker image builds on every push and pull request.

## AIRS mock and microchip validation

The `airs-mock` container is a development-only stand-in for the real AIRS API. It runs separately from the main stack and stores data in JSON; it ships with 1,000 animals and about 2,500 vaccinations. See [airs-mock/README.md](airs-mock/README.md) for the mock API and seed values.

Pet creation and microchip changes are validated against AIRS:

- A pet without a microchip number (`""`) skips AIRS validation; multiple pets without a microchip are allowed.
- The microchip must exist in AIRS.
- The AIRS owner personal ID must match the authenticated HappyPet user's personal ID.
- If the microchip is not found, belongs to another owner, or AIRS is unavailable, the pet is not created or updated.
- An unchanged microchip is not checked again during update.
- The local duplicate-microchip check runs before the AIRS call.

| Situation | HTTP | Response code |
| --- | --- | --- |
| Microchip not found in AIRS | `400` | `AIRS_MICROCHIP_NOT_FOUND` |
| Microchip belongs to another owner | `400` | `AIRS_OWNER_MISMATCH` |
| AIRS unavailable or timed out | `503` | `AIRS_UNAVAILABLE` |

Configuration:

| Key | Default | Description |
| --- | --- | --- |
| `Airs__BaseUrl` | `http://host.docker.internal:9100` in Compose | AIRS service URL |
| `Airs__ApiKey` | `dev-airs-key` | Sent as `X-Api-Key` |
| `Airs__TimeoutSeconds` | `10` | HTTP timeout |

The mock is not production AIRS. Real AIRS integration remains pending official API access and documentation. See [docs/AIRS-Integration.md](docs/AIRS-Integration.md) for the full contract.

## Email verification and password recovery (OTP)

Registration creates the account with `isEmailVerified: false` and sends a 6-digit OTP to the registered email address. Login is blocked until the email is verified.

| Rule | Value |
| --- | --- |
| OTP length | 6 digits |
| Email-verification lifetime | 15 minutes |
| Password-reset lifetime | 10 minutes |
| Maximum verification attempts | 5 |
| Resend cooldown | 60 seconds |
| Maximum sends per purpose | 3 per 15 minutes |

- Only an HMAC-SHA256 hash of the OTP is stored. OTP values are never returned by the API or written to logs.
- Requesting a new OTP invalidates the previous active OTP for the same purpose.
- `verify-email` and `reset-password` accept `email` + `code`; `reset-password` also requires `newPassword` (minimum 6 characters).
- OTPs are purpose-scoped: an email-verification code cannot reset a password, and a password-reset code cannot verify an email.
- `verify-email` returns `400` with `{ "message": "ელ-ფოსტა უკვე დადასტურებულია." }` if the email is already verified; a code is not accepted or consumed in that case.
- `resend-verification` and `forgot-password` always return a generic success response so they do not reveal whether an account exists.
- An unverified login returns `403` with `{ "code": "EMAIL_NOT_VERIFIED", "message": "..." }`.
- Existing accounts are marked as email-verified by the `AddEmailVerificationOtp` migration so they are not locked out.

Configuration:

| Key | Required | Description |
| --- | --- | --- |
| `Email:SmtpHost` | no | SMTP host; default `smtp.gmail.com` |
| `Email:SmtpPort` | no | SMTP port; default `587` |
| `Email:EnableSsl` | no | Enable STARTTLS/SSL; default `true` |
| `Email:From` | no | Sender address; defaults to `Email:SmtpUser` |
| `Email:SmtpUser` / `Email:SmtpPassword` | yes | SMTP credentials |
| `Otp:HashKey` | yes | Random secret of at least 32 characters used to hash OTPs |

- Use different random values for `Jwt:Secret`, `Jwt:RefreshTokenHashKey` and `Otp:HashKey`.
- Never commit real SMTP, JWT or OTP secrets. `appsettings.json` and `appsettings.Development.json` are git-ignored.

## JWT and sessions

Access tokens are short-lived JWTs. Refresh tokens are opaque random values stored as HMAC-SHA256 hashes in the `RefreshTokens` table and delivered only in an `HttpOnly` cookie.

| Rule | Value |
| --- | --- |
| Access token lifetime | 15 minutes |
| Refresh token lifetime | 30 days |
| Refresh token cookie | `happypet_refresh`, `HttpOnly`, `Secure`, `SameSite=Lax`, path `/api/v1/auth` |
| Rotation | Every refresh revokes the old token and issues a new one |
| Reuse detection | Reusing an old token outside the 10-second race window revokes the token family |
| Password reset | Revokes all active refresh tokens for the user |
| Logout | Revokes the current refresh token |
| Logout-all | Revokes every active refresh token for the user |
| Cleanup | A background service deletes expired refresh tokens daily |

Configuration:

| Key | Required | Description |
| --- | --- | --- |
| `Jwt:Secret` | yes | JWT signing secret, at least 32 characters |
| `Jwt:Issuer` | yes | JWT issuer |
| `Jwt:Audience` | yes | JWT audience |
| `Jwt:AccessTokenMinutes` | no | Access-token lifetime; default `15` |
| `Jwt:RefreshTokenDays` | no | Refresh-token lifetime; default `30` |
| `Jwt:RefreshTokenHashKey` | yes | Random secret of at least 32 characters used to hash refresh tokens |

Login and refresh responses contain `accessToken` and `expiresIn` (seconds), plus the user fields. The refresh token is never returned in the JSON body. Refresh tokens are rotated automatically; clients must use a single-flight refresh to avoid racing multiple refresh requests.

## Photo storage (AWS S3)

Pet photos are uploaded to S3. The database stores only the object key and metadata, not the binary content.

Configuration (`Aws:S3`):

| Key | Required | Description |
| --- | --- | --- |
| `BucketName` | yes | Existing S3 bucket for pet photos |
| `Region` | yes | AWS region, e.g. `eu-central-1` |
| `PublicBaseUrl` | recommended | CloudFront or custom domain used to build public photo URLs |
| `ServiceUrl` | no | S3-compatible endpoint for local development (MinIO, LocalStack, S3Mock) |
| `AccessKeyId` / `SecretAccessKey` | no | Local development only; production should use the default AWS credential chain |

- Credentials are resolved through the standard AWS chain (environment variables, profile, IAM role) unless explicit local keys are configured.
- The bucket must already exist. The API does not create it.
- Recommended production setup: private bucket + CloudFront with OAC. If `PublicBaseUrl` is empty, the API falls back to the public S3 URL, which only works when the bucket policy allows public reads.
- Allowed files: JPEG, PNG, WebP; maximum 5 MB; maximum 10 photos per pet.
- Never commit real AWS keys. `appsettings.json` is git-ignored.

## EF Core commands

```bash
# Add a migration
dotnet ef migrations add <MigrationName> -p src/HappyPet.Infrastructure -s src/HappyPet.WebAPI -o Migrations

# Apply migrations
dotnet ef database update -p src/HappyPet.Infrastructure -s src/HappyPet.WebAPI

# Remove the last migration
dotnet ef migrations remove -p src/HappyPet.Infrastructure -s src/HappyPet.WebAPI
```

## Notes

- Connection string: `src/HappyPet.WebAPI/appsettings.json` (`ConnectionStrings:DefaultConnection`, port `5433`).
- `appsettings.json` and `appsettings.Development.json` are git-ignored. Create them from `appsettings.example.json` and fill in the database password, JWT secret, SMTP credentials and OTP hash key.
- JWT settings live under `Jwt`: access tokens are valid for 15 minutes and refresh tokens for 30 days by default. See the JWT and sessions section for rotation and revocation behavior.
- Enums are serialized as strings (e.g. `"role": "User"`, `"species": "Dog"`).
- The current schema uses integer IDs. A database created with the previous GUID schema must be recreated; there is no in-place GUID-to-integer upgrade migration.
- For the frontend integration details see `docs/HappyPet-Frontend-Integration-Guide.docx`.
