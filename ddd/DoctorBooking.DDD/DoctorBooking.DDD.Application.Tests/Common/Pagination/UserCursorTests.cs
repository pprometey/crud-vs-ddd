using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Users.Queries;
using Xunit;

namespace DoctorBooking.DDD.Application.Tests.Common.Pagination;

public class UserCursorTests
{
    [Fact]
    public void UserCreatedAtCursor_Encode_ShouldIncludeSortKeyAndDirection()
    {
        // Arrange
        var createdAt = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var id = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var cursor = new UserCreatedAtCursor(createdAt, id) with { Direction = SortDirection.Desc };

        // Act
        var encoded = cursor.Encode();

        // Assert
        Assert.NotNull(encoded);
        Assert.NotEmpty(encoded);
    }

    [Fact]
    public void UserCreatedAtCursor_Decode_ShouldRestoreOriginalValues()
    {
        // Arrange
        var createdAt = new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc);
        var id = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var original = new UserCreatedAtCursor(createdAt, id) with { Direction = SortDirection.Asc };
        var encoded = original.Encode();

        // Act
        var decoded = UserCreatedAtCursor.Decode(encoded);

        // Assert
        Assert.NotNull(decoded);
        Assert.Equal(original.CreatedAt, decoded.CreatedAt);
        Assert.Equal(original.Id, decoded.Id);
        Assert.Equal(original.Direction, decoded.Direction);
    }

    [Fact]
    public void UserCreatedAtCursor_Decode_WithWrongField_ShouldReturnNull()
    {
        // Arrange
        var cursor = new UserNameCursor("Smith", "John", Guid.NewGuid()) with { Direction = SortDirection.Asc };
        var encoded = cursor.Encode(); // encoded with "name:asc"

        // Act
        var decoded = UserCreatedAtCursor.Decode(encoded); // trying to decode as created_at

        // Assert
        Assert.Null(decoded);
    }

    [Fact]
    public void UserNameCursor_Encode_ShouldIncludeAllFields()
    {
        // Arrange
        var lastName = "Smith";
        var firstName = "John";
        var id = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var cursor = new UserNameCursor(lastName, firstName, id) with { Direction = SortDirection.Asc };

        // Act
        var encoded = cursor.Encode();

        // Assert
        Assert.NotNull(encoded);
        Assert.NotEmpty(encoded);
    }

    [Fact]
    public void UserNameCursor_Decode_ShouldRestoreAllFields()
    {
        // Arrange
        var lastName = "Smith";
        var firstName = "John";
        var id = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var original = new UserNameCursor(lastName, firstName, id) with { Direction = SortDirection.Desc };
        var encoded = original.Encode();

        // Act
        var decoded = UserNameCursor.Decode(encoded);

        // Assert
        Assert.NotNull(decoded);
        Assert.Equal(original.LastName, decoded.LastName);
        Assert.Equal(original.FirstName, decoded.FirstName);
        Assert.Equal(original.Id, decoded.Id);
        Assert.Equal(original.Direction, decoded.Direction);
    }

    [Fact]
    public void UserEmailCursor_Encode_Decode_ShouldRoundTrip()
    {
        // Arrange
        var email = "john.smith@example.com";
        var id = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var original = new UserEmailCursor(email, id) with { Direction = SortDirection.Asc };

        // Act
        var encoded = original.Encode();
        var decoded = UserEmailCursor.Decode(encoded);

        // Assert
        Assert.NotNull(decoded);
        Assert.Equal(original.Email, decoded.Email);
        Assert.Equal(original.Id, decoded.Id);
        Assert.Equal(original.Direction, decoded.Direction);
    }

    [Fact]
    public void Decode_WithNull_ShouldReturnNull()
    {
        // Act
        var decoded = UserCreatedAtCursor.Decode(null);

        // Assert
        Assert.Null(decoded);
    }

    [Fact]
    public void Decode_WithEmptyString_ShouldReturnNull()
    {
        // Act
        var decoded = UserCreatedAtCursor.Decode(string.Empty);

        // Assert
        Assert.Null(decoded);
    }

    [Fact]
    public void Decode_WithInvalidBase64_ShouldReturnNull()
    {
        // Act
        var decoded = UserCreatedAtCursor.Decode("invalid-base64!!!");

        // Assert
        Assert.Null(decoded);
    }

    [Theory]
    [InlineData(SortDirection.Asc)]
    [InlineData(SortDirection.Desc)]
    public void UserCreatedAtCursor_PreservesDirection(SortDirection direction)
    {
        // Arrange
        var cursor = new UserCreatedAtCursor(DateTime.UtcNow, Guid.NewGuid()) with { Direction = direction };

        // Act
        var encoded = cursor.Encode();
        var decoded = UserCreatedAtCursor.Decode(encoded);

        // Assert
        Assert.NotNull(decoded);
        Assert.Equal(direction, decoded.Direction);
    }

    [Fact]
    public void UserNameCursor_WithSpecialCharacters_ShouldEncodeAndDecode()
    {
        // Arrange
        var lastName = "O'Brien";
        var firstName = "Seán";
        var id = Guid.NewGuid();
        var original = new UserNameCursor(lastName, firstName, id) with { Direction = SortDirection.Asc };

        // Act
        var encoded = original.Encode();
        var decoded = UserNameCursor.Decode(encoded);

        // Assert
        Assert.NotNull(decoded);
        Assert.Equal(original.LastName, decoded.LastName);
        Assert.Equal(original.FirstName, decoded.FirstName);
        Assert.Equal(original.Id, decoded.Id);
    }
}
