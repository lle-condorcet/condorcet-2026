# Multi-stage build for .NET 10 application
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project file and restore
COPY ElkLoggingLabApp.csproj .
RUN dotnet restore

# Copy source and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd --system appgroup && useradd --system --gid appgroup --no-create-home appuser

COPY --from=build /app/publish .

# Ensure the SQLite DB directory is writable
RUN mkdir -p /app/data && chown -R appuser:appgroup /app/data

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/elklab.db"

EXPOSE 8080

USER appuser
ENTRYPOINT ["dotnet", "ElkLoggingLabApp.dll"]
