using System.Globalization;
using Core.Common.Application.Pagination;

namespace DoctorBooking.DDD.Application.Appointments.Queries;

/// <summary>
/// Cursor for Appointment pagination sorted by CreatedAt + Id.
/// Supports both ASC and DESC directions.
/// </summary>
public sealed record AppointmentCreatedAtCursor(
    DateTime CreatedAt,
    Guid Id) : SortableCursor<AppointmentCreatedAtCursor>
{
    public const string Field = "created_at";
    
    public override string FieldName => Field;
    
    protected override object[] GetValues() => new object[] { CreatedAt, Id };
    
    public static AppointmentCreatedAtCursor? Decode(string? cursor) =>
        DecodeBase(cursor, Field, (values, dir) =>
            new AppointmentCreatedAtCursor(
                DateTime.Parse(values[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), 
                Guid.Parse(values[1])) with { Direction = dir });
}

/// <summary>
/// Cursor for Appointment pagination sorted by SlotStart + Id.
/// Useful for viewing upcoming appointments.
/// Supports both ASC and DESC directions.
/// </summary>
public sealed record AppointmentSlotStartCursor(
    DateTime SlotStart,
    Guid Id) : SortableCursor<AppointmentSlotStartCursor>
{
    public const string Field = "slot_start";
    
    public override string FieldName => Field;
    
    protected override object[] GetValues() => new object[] { SlotStart, Id };
    
    public static AppointmentSlotStartCursor? Decode(string? cursor) =>
        DecodeBase(cursor, Field, (values, dir) =>
            new AppointmentSlotStartCursor(
                DateTime.Parse(values[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), 
                Guid.Parse(values[1])) with { Direction = dir });
}
