# HappyPet

.NET 8 Clean Architecture scaffold with EF Core 8 and PostgreSQL.

## Structure

```
HappyPet.sln
docker-compose.yml
src/
  HappyPet.Domain/          # Entities, value objects, domain logic (no external dependencies)
    Common/BaseEntity.cs
    Entities/Pet.cs
  HappyPet.Application/     # Use cases, interfaces, DTOs, validation
    Common/Interfaces/IApplicationDbContext.cs
    DependencyInjection.cs
  HappyPet.Infrastructure/  # EF Core, PostgreSQL, external services
    Persistence/ApplicationDbContext.cs
    Persistence/Configurations/PetConfiguration.cs
    Persistence/ApplicationDbContextFactory.cs
    DependencyInjection.cs
  HappyPet.WebAPI/           # ASP.NET Core Web API (composition root)
    Controllers/PetsController.cs
    Program.cs
```

Dependency direction: `Domain <- Application <- Infrastructure <- WebAPI`.

## Getting started

1. Start PostgreSQL:

   ```bash
   docker compose up -d
   ```

2. Restore tools and apply migrations:

   ```bash
   dotnet tool restore
   dotnet ef database update -p src/HappyPet.Infrastructure -s src/HappyPet.WebAPI
   ```

3. Run the API:

   ```bash
   dotnet run --project src/HappyPet.WebAPI
   ```

   Swagger: `http://localhost:5246/swagger`

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

- Connection string: `src/HappyPet.WebAPI/appsettings.json` (`ConnectionStrings:DefaultConnection`).
- The `Pet` entity and `PetsController` exist only to verify the DB/API wiring; replace them with real features.
- The design-time factory uses `ConnectionStrings__DefaultConnection` if set, otherwise the local Docker defaults.
