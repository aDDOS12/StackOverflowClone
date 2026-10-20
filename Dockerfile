# syntax=docker/dockerfile:1

# ---------- Phase 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/StackOverflowClone.Domain/StackOverflowClone.Domain.csproj src/StackOverflowClone.Domain/
COPY src/StackOverflowClone.Application/StackOverflowClone.Application.csproj src/StackOverflowClone.Application/
COPY src/StackOverflowClone.Infrastructure/StackOverflowClone.Infrastructure.csproj src/StackOverflowClone.Infrastructure/
COPY src/StackOverflowClone.Api/StackOverflowClone.Api.csproj src/StackOverflowClone.Api/
RUN dotnet restore src/StackOverflowClone.Api/StackOverflowClone.Api.csproj

COPY src/ src/
RUN dotnet publish src/StackOverflowClone.Api/StackOverflowClone.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ---------- Phase 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "StackOverflowClone.Api.dll"]