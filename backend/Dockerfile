# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first so layer cache is reused when only source changes.
COPY CrisisPlatform.Api/CrisisPlatform.Api.csproj CrisisPlatform.Api/
RUN dotnet restore CrisisPlatform.Api/CrisisPlatform.Api.csproj

# Copy the rest of the API source and publish.
COPY CrisisPlatform.Api/ CrisisPlatform.Api/
RUN dotnet publish CrisisPlatform.Api/CrisisPlatform.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Runtime stage — smallest possible image, no SDK.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Run as a non-root user for security.
RUN addgroup --system --gid 1001 appgroup \
 && adduser  --system --uid 1001 --ingroup appgroup appuser
USER appuser

COPY --from=build --chown=appuser:appgroup /app/publish .

# Expose the HTTP port used by ASP.NET Core in containers.
# HTTPS termination is handled by the reverse proxy (nginx / ingress).
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "CrisisPlatform.Api.dll"]
