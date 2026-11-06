# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY HappyPet.sln ./
COPY .config/ .config/
COPY src/HappyPet.Domain/HappyPet.Domain.csproj src/HappyPet.Domain/
COPY src/HappyPet.Application/HappyPet.Application.csproj src/HappyPet.Application/
COPY src/HappyPet.Infrastructure/HappyPet.Infrastructure.csproj src/HappyPet.Infrastructure/
COPY src/HappyPet.WebAPI/HappyPet.WebAPI.csproj src/HappyPet.WebAPI/

RUN dotnet restore HappyPet.sln

COPY . .
RUN dotnet tool restore
RUN dotnet publish src/HappyPet.WebAPI/HappyPet.WebAPI.csproj \
    -c Release \
    -o /out/api \
    --no-restore \
    /p:UseAppHost=false

FROM build AS ef-bundle

# These values are used only while EF Core builds the migration bundle.
ENV ASPNETCORE_ENVIRONMENT=Development \
    ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=HappyPetDb;Username=postgres;Password=postgres" \
    Jwt__Secret="build-only-happypet-jwt-secret-0123456789abcdef" \
    Jwt__RefreshTokenHashKey="build-only-happypet-refresh-hash-key-0123456789abcdef" \
    Otp__HashKey="build-only-happypet-otp-hash-key-0123456789abcdef" \
    Airs__BaseUrl="http://localhost:9100"

RUN dotnet ef migrations bundle \
    --project src/HappyPet.Infrastructure \
    --startup-project src/HappyPet.WebAPI \
    --configuration Release \
    --output /out/efbundle \
    --self-contained \
    --runtime linux-x64

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /out/api ./

RUN groupadd --gid 1001 happypet \
    && useradd --uid 1001 --gid happypet --create-home happypet \
    && chown -R happypet:happypet /app

USER happypet

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=6 \
    CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "HappyPet.WebAPI.dll"]

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0 AS migrate
WORKDIR /app

COPY --from=ef-bundle /out/efbundle /app/efbundle
RUN chmod +x /app/efbundle

ENTRYPOINT ["/app/efbundle"]
