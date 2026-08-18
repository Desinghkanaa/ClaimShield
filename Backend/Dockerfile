# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY Backend/ClaimShield.Api.csproj Backend/
RUN dotnet restore Backend/ClaimShield.Api.csproj

COPY Backend/ Backend/

RUN dotnet publish Backend/ClaimShield.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "ClaimShield.Api.dll"]