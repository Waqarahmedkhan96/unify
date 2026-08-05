FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/Unify.Erp.sln backend/Unify.Erp.sln
COPY backend/src/Unify.Erp.Api/Unify.Erp.Api.csproj backend/src/Unify.Erp.Api/
COPY backend/src/Unify.Erp.Application/Unify.Erp.Application.csproj backend/src/Unify.Erp.Application/
COPY backend/src/Unify.Erp.Contracts/Unify.Erp.Contracts.csproj backend/src/Unify.Erp.Contracts/
COPY backend/src/Unify.Erp.Domain/Unify.Erp.Domain.csproj backend/src/Unify.Erp.Domain/
COPY backend/src/Unify.Erp.Infrastructure/Unify.Erp.Infrastructure.csproj backend/src/Unify.Erp.Infrastructure/

RUN dotnet restore backend/src/Unify.Erp.Api/Unify.Erp.Api.csproj

COPY backend/ backend/
WORKDIR /src/backend/src/Unify.Erp.Api
RUN dotnet publish Unify.Erp.Api.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --retries=3 CMD curl --fail http://localhost:8080/health/live || exit 1
USER app
ENTRYPOINT ["dotnet", "Unify.Erp.Api.dll"]
