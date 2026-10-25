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
    Enums/Enums.cs
  HappyPet.Application/     # Use cases, interfaces, DTOs, validation
    Dtos/Requests/
    Dtos/Responises/
    Interfaces/
    Services/AuthService.cs
    Services/UserServices.cs
    Services/PetService.cs
  HappyPet.Infrastructure/  # EF Core, PostgreSQL, security, repositories
    Persistence/HappyPetDbContext.cs
    Persistence/Repositories/
    Securuty/PasswordHasher.cs
    Securuty/TokenGenerator.cs
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

All endpoints are under `/api/v1`. Registration and login are public; every other endpoint requires `Authorization: Bearer <token>`.

| Method | Endpoint | Description |
| --- | --- | --- |
| POST | `/api/v1/user/add-user` | Register a user |
| POST | `/api/v1/auth/login` | Login and receive a JWT |
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

CORS is enabled for `http://localhost:3000` and `http://localhost:5173` by default. Override with `Cors:AllowedOrigins` in configuration.

## EF Core commands

```bash
# Add a migration
dotnet ef migrations add <MigrationName> -p src/HappyPet.Infrastructure -s src/HappyPet.WebAPI -o Persistence/Migrations

# Apply migrations
dotnet ef database update -p src/HappyPet.Infrastructure -s src/HappyPet.WebAPI

# Remove the last migration
dotnet ef migrations remove -p src/HappyPet.Infrastructure -s src/HappyPet.WebAPI
```

## Notes

- Connection string: `src/HappyPet.WebAPI/appsettings.json` (`ConnectionStrings:DefaultConnection`, port `5433`).
- `appsettings.json` is git-ignored; create it by copying `appsettings.example.json` and filling in your own password and JWT secret.
- JWT settings live under `Jwt` (`Secret`, `Issuer`, `Audience`); tokens are valid for 7 days.
- Enums are serialized as strings (e.g. `"role": "User"`, `"species": "Dog"`).
- For the frontend integration details see `docs/HappyPet-Frontend-Integration-Guide.docx`.
