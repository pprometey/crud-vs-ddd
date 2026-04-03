# Infrastructure.Mapped Tests

Tests for mapper classes and repository integration (Mapped persistence approach).

## Structure

### Unit Tests (Mappers/)

Tests for mapper classes that convert between domain aggregates and database models:

- **AppointmentMapperTests**: Round-trip mapping for appointments with various statuses and payment scenarios
- **ScheduleMapperTests**: Round-trip mapping for schedules with multiple time slots
- **UserMapperTests**: Round-trip mapping for users with single and multiple roles

### Integration Tests (Repositories/)

Integration tests using in-memory SQLite to verify:

- Aggregate loading with nested entities (Include/ThenInclude)
- SaveChanges persistence
- Update operations
- Delete operations
- Query methods (GetById, GetByEmail, GetByDoctorId)

## Test Strategy

All mapper tests verify:

- Round-trip consistency (Domain → DbModel → Domain preserves data)
- Edge cases (empty collections, minimal data)
- Business state preservation (statuses, calculated values)

Integration tests verify EF Core configuration and mapping work correctly together.
