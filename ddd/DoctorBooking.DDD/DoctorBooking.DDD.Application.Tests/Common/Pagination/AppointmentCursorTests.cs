using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Appointments.Queries;
using Xunit;

namespace DoctorBooking.DDD.Application.Tests.Common.Pagination;

public class AppointmentCursorTests
{
    [Fact]
    public void AppointmentCreatedAtCursor_Encode_Decode_ShouldRoundTrip()
    {
        // Arrange
        var createdAt = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var id = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var original = new AppointmentCreatedAtCursor(createdAt, id) with { Direction = SortDirection.Desc };

        // Act
        var encoded = original.Encode();
        var decoded = AppointmentCreatedAtCursor.Decode(encoded);

        // Assert
        Assert.NotNull(decoded);
        Assert.Equal(original.CreatedAt, decoded.CreatedAt);
        Assert.Equal(original.Id, decoded.Id);
        Assert.Equal(original.Direction, decoded.Direction);
    }

    [Fact]
    public void AppointmentSlotStartCursor_Encode_Decode_ShouldRoundTrip()
    {
        // Arrange
        var slotStart = new DateTime(2026, 4, 15, 14, 30, 0, DateTimeKind.Utc);
        var id = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var original = new AppointmentSlotStartCursor(slotStart, id) with { Direction = SortDirection.Asc };

        // Act
        var encoded = original.Encode();
        var decoded = AppointmentSlotStartCursor.Decode(encoded);

        // Assert
        Assert.NotNull(decoded);
        Assert.Equal(original.SlotStart, decoded.SlotStart);
        Assert.Equal(original.Id, decoded.Id);
        Assert.Equal(original.Direction, decoded.Direction);
    }

    [Fact]
    public void AppointmentSlotStartCursor_Decode_WithWrongField_ShouldReturnNull()
    {
        // Arrange
        var cursor = new AppointmentCreatedAtCursor(DateTime.UtcNow, Guid.NewGuid()) with { Direction = SortDirection.Asc };
        var encoded = cursor.Encode();

        // Act
        var decoded = AppointmentSlotStartCursor.Decode(encoded);

        // Assert
        Assert.Null(decoded);
    }

    [Theory]
    [InlineData(SortDirection.Asc)]
    [InlineData(SortDirection.Desc)]
    public void AppointmentSlotStartCursor_PreservesDirection(SortDirection direction)
    {
        // Arrange
        var cursor = new AppointmentSlotStartCursor(DateTime.UtcNow, Guid.NewGuid()) with { Direction = direction };

        // Act
        var encoded = cursor.Encode();
        var decoded = AppointmentSlotStartCursor.Decode(encoded);

        // Assert
        Assert.NotNull(decoded);
        Assert.Equal(direction, decoded.Direction);
    }

    [Fact]
    public void AppointmentCreatedAtCursor_Decode_WithNull_ShouldReturnNull()
    {
        // Act
        var decoded = AppointmentCreatedAtCursor.Decode(null);

        // Assert
        Assert.Null(decoded);
    }

    [Fact]
    public void AppointmentCreatedAtCursor_Decode_WithInvalidData_ShouldReturnNull()
    {
        // Act
        var decoded = AppointmentCreatedAtCursor.Decode("invalid-cursor-data");

        // Assert
        Assert.Null(decoded);
    }
}
