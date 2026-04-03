# DoctorBooking.DDD.Api

DDD-based API for doctor appointment booking system.

## Production-Ready Features

### ✅ OpenAPI & Interactive Documentation

**Scalar UI** - Modern OpenAPI documentation interface

- URL: `/scalar/v1` (all environments)
- OpenAPI spec: `/openapi/v1.json`

Features:

- Interactive API testing
- Code generation examples (C#, TypeScript, Python, etc.)
- Modern, responsive UI
- Search and filtering

### ✅ API Versioning

Professional URL-based versioning with multiple reader support

**Versioning Strategy:**

- Current version: **v1**
- URL format: `/api/v1/appointments`, `/api/v1/users`, etc.
- Default version: v1 (assumed if not specified)

**Supported version readers:**

- **URL segment** (recommended): `/api/v1/appointments`
- **Query string**: `/api/appointments?api-version=1.0`
- **Header**: `X-Api-Version: 1.0`

**Version information in responses:**

- `api-supported-versions` header lists all supported versions
- `api-deprecated-versions` header lists deprecated versions (when applicable)

**Example requests:**

```bash
# URL versioning (recommended)
curl https://api.example.com/api/v1/appointments

# Query string
curl https://api.example.com/api/appointments?api-version=1.0

# Header
curl -H "X-Api-Version: 1.0" https://api.example.com/api/appointments
```

**Future versioning:**

When breaking changes are needed:

1. Create new version set in `Program.cs`
2. Add version-specific endpoint groups
3. Keep old versions for deprecation period
4. Mark old versions as deprecated with sunset dates

### ✅ Error Handling

`ExceptionHandlingMiddleware` - Global exception handler

- `DomainException` → 400 Bad Request with ProblemDetails
- `ArgumentException` / `InvalidOperationException` → 400 Bad Request
- Unhandled exceptions → 500 Internal Server Error
- RFC 7807 compliant ProblemDetails responses
- Trace IDs for debugging

### ✅ CORS

Configured for cross-origin requests

**⚠️ Production**: Update `appsettings.Production.json` with specific origins:

```json
{
  "Cors": {
    "AllowedOrigins": ["https://yourdomain.com"]
  }
}
```

### ✅ Response Compression

Enabled for HTTPS to reduce payload size

### ✅ Rate Limiting

Fixed window rate limiter:

- Default: 100 requests per minute per IP
- Configurable via `appsettings.json`

Apply to endpoints:

```csharp
group.MapPost("/expensive-operation", Handler)
    .RequireRateLimiting("fixed");
```

### ✅ Health Checks

- `/health` - Readiness probe (all checks must pass)
- `/alive` - Liveness probe (basic check)

Use in Kubernetes/Docker:

```yaml
livenessProbe:
  httpGet:
    path: /alive
    port: 8080
readinessProbe:
  httpGet:
    path: /health
    port: 8080
```

### ✅ Observability (Aspire)

Built-in:

- **OpenTelemetry** - Distributed tracing
- **Metrics** - ASP.NET Core, HTTP client, runtime metrics
- **Logging** - Structured logging with OpenTelemetry integration

### ✅ Resilience

- HTTP client retry policies (via `Microsoft.Extensions.Http.Resilience`)
- Circuit breakers
- Service discovery support

## Running the API

### Development

```bash
dotnet run --environment Development
```

### Production

```bash
dotnet run --environment Production
```

## API Endpoint Examples

All endpoints are versioned. Current stable version: **v1**

### Users

- `GET /api/v1/users/{userId}` - Get user by ID
- `POST /api/v1/users` - Create user
- `GET /api/v1/users/doctors` - List doctors

### Appointments

- `POST /api/v1/appointments` - Book appointment
- `GET /api/v1/appointments/{appointmentId}` - Get appointment
- `GET /api/v1/appointments/by-doctor/{doctorId}` - List doctor's appointments
- `GET /api/v1/appointments/by-patient/{patientId}` - List patient's appointments
- `POST /api/v1/appointments/{appointmentId}/cancel` - Cancel appointment
- `POST /api/v1/appointments/{appointmentId}/complete` - Complete appointment
- `POST /api/v1/appointments/{appointmentId}/no-show` - Mark as no-show
- `POST /api/v1/appointments/{appointmentId}/payments` - Add payment

### Schedules

- `POST /api/v1/schedules` - Create schedule
- `GET /api/v1/schedules/{scheduleId}` - Get schedule
- `GET /api/v1/schedules/by-doctor/{doctorId}` - List doctor's schedules
- `GET /api/v1/schedules/slots/available` - Find available slots

## Configuration Checklist for Production

- [ ] Update CORS allowed origins in `appsettings.Production.json`
- [ ] Configure rate limiting thresholds
- [ ] Review API versioning strategy and add sunset dates for deprecated versions
- [ ] Set up authentication for `/health`, `/alive`, `/scalar` endpoints (if needed)
- [ ] Configure OpenTelemetry exporters (OTLP endpoint)
- [ ] Review logging levels (avoid verbose logging in production)
- [ ] Set up database connection strings
- [ ] Configure secrets management (Azure Key Vault, AWS Secrets Manager, etc.)

## Security Considerations

### OpenAPI Documentation in Production

Currently `/scalar` is available in all environments. Consider:

1. **Authentication**: Require authentication for documentation endpoints
2. **IP whitelisting**: Restrict access to internal IPs only
3. **Removal**: Disable completely in production if not needed

Example (add to Program.cs):

```csharp
if (!app.Environment.IsProduction())
{
    app.MapScalarApiReference();
}
```

### Health Checks in Production

Consider adding authentication or IP filtering:

```csharp
app.MapHealthChecks("/health")
   .RequireAuthorization("InternalOnly");
```

## Architecture Notes

- **DDD Tactical Patterns**: Aggregates, Entities, Value Objects, Domain Events
- **CQRS**: Separate command and query handling
- **Clean Architecture**: Domain → Application → Infrastructure → API
- **Result Pattern**: Railway-oriented programming, no exceptions for business logic
