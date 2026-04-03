# Infrastructure Integration Tests (Direct Approach)

Integration tests for **Infrastructure** project (Direct persistence approach) using in-memory SQLite.

## Approach

Direct persistence: Domain models mapped directly to EF Core without intermediate DbModels.

## Test Strategy

Tests verify:

- Aggregate loading with nested entities (Include/ThenInclude)
- SaveChanges persistence
- Update operations
- Delete operations
- Query methods (GetById, GetByEmail, GetByDoctorId)

## Running Tests

```bash
dotnet test
```
