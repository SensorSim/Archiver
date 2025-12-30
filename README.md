# Archiver

Measurement archive service.

Stores measurements in its own Postgres database and exposes query endpoints (filters + paging). Measurements are treated as an immutable log.

## Branching

- `dev` – development
- `main` – stable / demo-ready

## Requirements

- .NET SDK (tested with 10.0.101)
- Postgres

## Run

Recommended: run the full stack with Docker Compose (see `infra/docker`).

Local run (you still need Postgres running):

```bash
dotnet run
```

## Configuration

Environment variables:

- `ConnectionStrings__Postgres` – Postgres connection string

## API

Swagger (docker default): `http://localhost:8081/swagger`

Endpoints:

- `POST /measurements`
- `GET /measurements` (filters + paging)
- `GET /measurements/{id}`

Health:

- `GET /health/live`
- `GET /health/ready`
