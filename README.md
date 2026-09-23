# ReportQueue

A .NET 10 project for a SQL Server-backed report job queue. The ASP.NET Core API stores new jobs and exposes their status. A separate worker project provides the starting point for background processing.

## Current status

- Create a job with `POST /api/jobs`.
- Retrieve job status with `GET /api/jobs/{id}`.
- Store jobs using Entity Framework Core and SQL Server.
- Run a worker demonstration that logs a simulated job once at startup.

The worker does not yet fetch queued jobs, generate reports, update job status, or retry failures. Jobs created through the API remain queued unless another process updates them.

## Project structure

| Project | Purpose |
| --- | --- |
| `Api` | HTTP endpoints, database configuration, OpenAPI, and Scalar UI. |
| `Domain` | Job entity, request contract, status enum, and EF Core `DataContext`. |
| `Worker1` | Background service and simulated job processor. |
| `Worker` | Placeholder class library; not included in `ReportQueue.slnx`. |

## Prerequisites

- .NET 10 SDK.
- A running SQL Server instance and credentials with access to the `ReportQueue` database.
- EF Core CLI (`dotnet-ef`) version 10.0.12 for the database setup below.

Run the following commands from the repository root. Shell examples use PowerShell.

## Setup

### 1. Restore and build

```powershell
dotnet restore ReportQueue.slnx
dotnet build ReportQueue.slnx --no-restore
```

### 2. Configure SQL Server

The API reads `ConnectionStrings:Connection`. The section name must be **ConnectionStrings**, including the final `s`.

Example configuration in `Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Connection": "Server=localhost;Database=ReportQueue;User Id=<username>;Password=<password>;TrustServerCertificate=True;"
  }
}
```

Merge this section into the existing settings. Replace the placeholders with your local connection details. Keep actual credentials out of source control. You can override the setting for the current PowerShell session instead:

```powershell
$env:ConnectionStrings__Connection = 'Server=localhost;Database=ReportQueue;User Id=<username>;Password=<password>;TrustServerCertificate=True;'
```

Use the same session for migration and API commands so they receive the override. `TrustServerCertificate=True` is intended here for local development.

### 3. Create the database schema

The repository does not currently include EF Core migrations, and the API does not create the database automatically. For a new database, install the matching CLI if it is not already available:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
```

Generate the initial migration in `Domain`, then apply it using `Api` as the startup project:

```powershell
dotnet ef migrations add InitialCreate --project Domain --startup-project Api
dotnet ef database update --project Domain --startup-project Api
```

Generate `InitialCreate` only while the project has no migrations; once migrations are committed, other developers only need `database update`. The database user needs permission to create the database/schema, or an administrator must provision them. If your database already contains tables, reconcile its schema before applying an initial migration.

### 4. Start the API

Trust the local HTTPS development certificate if needed:

```powershell
dotnet dev-certs https --trust
dotnet run --project Api --launch-profile https
```

The HTTPS launch profile uses:

- API: https://localhost:7192
- Scalar API explorer: https://localhost:7192/scalar/v1
- OpenAPI document: https://localhost:7192/openapi/v1.json

Scalar and OpenAPI are enabled only in the Development environment. The launch profiles set this environment automatically. The API also configures HTTP on port `5222` and uses HTTPS redirection.

### 5. Run the worker demonstration

In a separate terminal:

```powershell
dotnet run --project Worker1
```

The worker logs `Looking for jobs...`, `Processing job...`, and `Job completed.` It runs this sequence once; it does not continuously poll for jobs. Stop the host with `Ctrl+C`.

## API usage

### Create a job

`POST /api/jobs`

Both `type` and `payload` are strings. If the payload contains JSON, serialize it into a string rather than sending a nested object.

```json
{
  "type": "SalesReport",
  "payload": "{\"month\":\"2026-09\"}"
}
```

`SalesReport` is an example label; there is no report handler or supported-type registry yet.

PowerShell example:

```powershell
$body = @{
    type = 'SalesReport'
    payload = (@{ month = '2026-09' } | ConvertTo-Json -Compress)
} | ConvertTo-Json

$job = Invoke-RestMethod -Method Post `
    -Uri 'https://localhost:7192/api/jobs' `
    -ContentType 'application/json' `
    -Body $body

$job
```

Successful response: `200 OK`.

```json
{
  "id": "2b2ebce1-b8e7-4a16-8b9d-d0ccf74ee53c",
  "status": 0
}
```

New jobs start with `Queued` status, zero attempts, and a maximum of three attempts. `CreatedAt` and `AvailableAt` are set to the current UTC time. Retry execution is not implemented yet.

### Get a job

`GET /api/jobs/{id}`

```powershell
Invoke-RestMethod -Uri "https://localhost:7192/api/jobs/$($job.id)"
```

Returns `200 OK` with `id`, `type`, `status`, `attempts`, `createdAt`, `startedAt`, `completedAt`, and `error`. The last three fields are initially null. A missing job returns `404 Not Found`. The route requires a GUID.

Statuses are serialized as numbers:

| Value | Status |
| --- | --- |
| `0` | Queued |
| `1` | Processing |
| `2` | Completed |
| `3` | Failed |

## Troubleshooting

| Problem | What to check |
| --- | --- |
| `The ConnectionString property has not been initialized.` | Ensure the setting is `ConnectionStrings:Connection`, not `ConnectionString:Connection`, and restart the API after changing it. |
| SQL Server connection or login failure | Check that SQL Server is running and the server address, credentials, and database permissions are correct. |
| Missing database or `Invalid object name 'Job'` | Apply the EF Core migrations to the database configured for the API. |
| Build cannot overwrite `Api.dll` or `Domain.dll` | Stop the running API/debugging session, then rebuild. |
| Local HTTPS certificate error | Run `dotnet dev-certs https --trust` and retry. |
| Job stays queued | Expected with the current worker demonstration; database-backed processing is not implemented. |

## Development notes

There is currently no automated test project. After setup, use Scalar or the examples above to create a job and retrieve it by its returned ID. The API does not currently configure authentication or require authorization on the job endpoints.
