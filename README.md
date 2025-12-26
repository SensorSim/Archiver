# Archiver

**Purpose:** Append-only storage for measurements.

Archiver owns the measurement database and provides query endpoints for “what happened”.
Other services must not read/write the Archiver DB directly.

Database:
- `postgres-archiver` (Docker Compose)

---

## API

Swagger:
- `http://localhost:8081/swagger`

Endpoints:
- `POST /measurements` – store a new measurement
- `GET /measurements` – query archived measurements (paging + filters)
- `GET /measurements/{id}` – fetch a single record

Health:
- `GET /health/live`
- `GET /health/ready`

---

## Design note: append-only

Measurements are treated as an immutable log.
If you add PUT/DELETE endpoints, they should return **405 Method Not Allowed** (or be restricted to admin/retention tasks),
so the “archive” remains trustworthy.
