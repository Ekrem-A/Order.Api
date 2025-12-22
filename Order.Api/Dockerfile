# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Order.Api/Order.Api.csproj", "Order.Api/"]
COPY ["Order.Application/Order.Application.csproj", "Order.Application/"]
COPY ["Order.Domain/Order.Domain.csproj", "Order.Domain/"]
COPY ["Order.Infrastructure/Order.Infrastructure.csproj", "Order.Infrastructure/"]

RUN dotnet restore "Order.Api/Order.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Order.Api"
RUN dotnet build "Order.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Order.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Order.Api.dll"]
