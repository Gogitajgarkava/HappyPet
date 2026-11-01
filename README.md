# HappyPet

.NET 8 Clean Architecture backend with EF Core 8, PostgreSQL and JWT authentication.

## Structure

```
HappyPet.sln
docker-compose.yml
src/
  HappyPet.Domain/          # Entities, value objects, domain logic (no external dependencies)
    Common/BaseEntity.cs
    Entities/User.cs
    Entities/Pet.cs
    Entities/PetPhoto.cs
    Enums/Enums.cs
  HappyPet.Application/     # Use cases, interfaces, DTOs, validation
    Dtos/Requests/
    Dtos/Responises/
    Interfaces/
    Services/AuthService.cs
    Services/UserServices.cs
    Services/PetService.cs
  HappyPet.Infrastructure/  # EF Core, PostgreSQL, security, S3 storage, repositories
    Persistence/HappyPetDbContext.cs
    Persistence/Repositories/
    Securuty/PasswordHasher.cs
    Securuty/TokenGenerator.cs
    Storage/S3FileStorageService.cs
  HappyPet.WebAPI/           # ASP.NET Core Web API (composition root)
    Controllers/AuthController.cs
    Controllers/UserControler.cs
    Controllers/PetController.cs
    Program.cs
```

Dependency direction: `Domain <- Application <- Infrastructure <- WebAPI`.

## Getting started

1. Start PostgreSQL:

   ```bash
   docker compose up -d
   ```

2. Create the local configuration file from the example and set your own database password and JWT secret:

   ```bash
   cp appsettings.example.json src/HappyPet.WebAPI/appsettings.json
   ```

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

All endpoints are under `/api/v1`. Registration, login, email verification and password recovery are public; every other endpoint requires `Authorization: Bearer <token>`.

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
| GET | `/api/v1/pet/owner/{ownerId}` | List pets by owner |
| GET | `/api/v1/pet/lost` | List lost pets |
| PUT | `/api/v1/pet/{id}` | Update pet (owner or admin) |
| DELETE | `/api/v1/pet/{id}` | Delete pet (owner or admin) |
| GET | `/api/v1/pet/{id}/photos` | List pet photos (owner or admin) |
| POST | `/api/v1/pet/{id}/photos` | Upload a pet photo, `multipart/form-data`, max 5 MB (owner or admin) |
| PUT | `/api/v1/pet/{id}/photos/{photoId}/primary` | Set the primary pet photo (owner or admin) |
| DELETE | `/api/v1/pet/{id}/photos/{photoId}` | Delete a pet photo (owner or admin) |

CORS is enabled for `http://localhost:3000` and `http://localhost:5173` by default, with credentials allowed for the refresh cookie. Override with `Cors:AllowedOrigins` in configuration. Frontend requests that use the refresh flow must send `credentials: 'include'`.

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
