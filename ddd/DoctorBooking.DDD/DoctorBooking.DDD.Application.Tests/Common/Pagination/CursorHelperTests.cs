using Core.Common.Application.Pagination;
using Xunit;

namespace DoctorBooking.DDD.Application.Tests.Common.Pagination;

public class CursorHelperTests
{
    [Theory]
    [InlineData("created_at", SortDirection.Asc, "created_at:asc")]
    [InlineData("created_at", SortDirection.Desc, "created_at:desc")]
    [InlineData("name", SortDirection.Asc, "name:asc")]
    [InlineData("email", SortDirection.Desc, "email:desc")]
    public void BuildSortKey_ShouldCombineFieldAndDirection(
        string field, 
        SortDirection direction, 
        string expected)
    {
        // Act
        var result = CursorHelper.BuildSortKey(field, direction);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("created_at:asc", "created_at", SortDirection.Asc)]
    [InlineData("created_at:desc", "created_at", SortDirection.Desc)]
    [InlineData("name:asc", "name", SortDirection.Asc)]
    [InlineData("email:desc", "email", SortDirection.Desc)]
    [InlineData("field_with_underscores:asc", "field_with_underscores", SortDirection.Asc)]
    [InlineData("complex_field_name:desc", "complex_field_name", SortDirection.Desc)]
    public void ParseSortKey_WithValidInput_ShouldExtractFieldAndDirection(
        string sortKey, 
        string expectedField, 
        SortDirection expectedDirection)
    {
        // Act
        var (field, direction) = CursorHelper.ParseSortKey(sortKey);

        // Assert
        Assert.Equal(expectedField, field);
        Assert.Equal(expectedDirection, direction);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("field")]
    [InlineData("")]
    [InlineData("field:invalid")]
    public void ParseSortKey_WithInvalidInput_ShouldReturnDefaults(string sortKey)
    {
        // Act
        var (field, direction) = CursorHelper.ParseSortKey(sortKey);

        // Assert
        Assert.Equal(string.Empty, field);
        Assert.Equal(SortDirection.Asc, direction);
    }

    [Fact]
    public void ParseSortKey_WithNull_ShouldReturnDefaults()
    {
        // Act
        var (field, direction) = CursorHelper.ParseSortKey(null);

        // Assert
        Assert.Equal(string.Empty, field);
        Assert.Equal(SortDirection.Asc, direction);
    }

}
