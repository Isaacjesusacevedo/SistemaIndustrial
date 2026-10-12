# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY SistemaIndustrial.AD/*.csproj ./SistemaIndustrial.AD/
COPY SistemaIndustrial.BR/*.csproj ./SistemaIndustrial.BR/
COPY SistemaIndustrial.API/*.csproj ./SistemaIndustrial.API/
COPY SistemaIndustrial.AD.Test/*.csproj ./SistemaIndustrial.AD.Test/
COPY SistemaIndustrial.BR.Test/*.csproj ./SistemaIndustrial.BR.Test/
COPY SistemaIndustrial.API.Test/*.csproj ./SistemaIndustrial.API.Test/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build and publish
RUN dotnet publish SistemaIndustrial.API/SistemaIndustrial.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SistemaIndustrial.API.dll"]