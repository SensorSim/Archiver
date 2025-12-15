# Archiver Service

Archiver is a microservice responsible for storing sensor measurements
in a PostgreSQL database. It is part of the SensorSim / Reactor
Monitoring System.

## Responsibilities

-   Accept sensor measurements (sensorId, timestamp, value)
-   Persist data using Entity Framework Core
-   Expose REST API with Swagger documentation
-   Provide health endpoints for container orchestration (Docker /
    Kubernetes)

## Tech Stack

-   .NET (ASP.NET Core Minimal API)
-   Entity Framework Core
-   PostgreSQL
-   Docker / Docker Compose
-   Swagger (OpenAPI)

## API Endpoints

### POST /measurements

Creates a new measurement.

Request body:

``` json
{
  "sensorId": "S1",
  "timestamp": "2025-12-15T14:00:00+01:00",
  "value": 42.0
}
```

Example:

``` bash
curl -X POST http://localhost:8081/measurements \
  -H "Content-Type: application/json" \
  -d "{\"sensorId\":\"S1\",\"timestamp\":\"2025-12-15T14:00:00+01:00\",\"value\":42.0}"
```

### GET /measurements

Returns stored measurements.

Optional query parameters: - sensorId

Examples:

``` bash
curl http://localhost:8081/measurements
curl "http://localhost:8081/measurements?sensorId=S1"
```

### Health Checks

-   GET /health/live
-   GET /health/ready

Examples:

``` bash
curl http://localhost:8081/health/live
curl http://localhost:8081/health/ready
```

## Local Development

### Prerequisites

-   Docker
-   .NET SDK

### Run with Docker Compose

``` bash
docker compose up -d
```

Swagger UI:

    http://localhost:8081/swagger

## Database

-   PostgreSQL
-   Database schema managed with EF Core migrations
-   Migrations are applied automatically on service startup

## Docker

The service is containerized and designed to be stateless. It can be
scaled horizontally in Kubernetes environments.

## Architecture Role

Archiver acts as the persistence layer in a microservice-based system.
Other services communicate with Archiver via REST APIs.
