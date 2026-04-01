using DoctorBooking.DDD.Domain.Appointments;
using Xunit;

namespace DoctorBooking.DDD.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_NegativeAmount_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Money(-1));
    }

    [Fact]
    public void Constructor_ZeroAmount_Succeeds()
    {
        var m = new Money(0);
        Assert.True(m.IsZero());
    }

    [Fact]
    public void Zero_IsZero()
    {
        Assert.True(Money.Zero.IsZero());
    }

    [Fact]
    public void Addition_ReturnsSumAmount()
    {
        var result = new Money(30) + new Money(20);
        Assert.Equal(new Money(50), result);
    }

    [Fact]
    public void Subtraction_ValidDifference_Succeeds()
    {
        var result = new Money(50) - new Money(20);
        Assert.Equal(new Money(30), result);
    }

    [Fact]
    public void Subtraction_ResultWouldBeNegative_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new Money(10) - new Money(20));
    }

    [Fact]
    public void Subtraction_EqualAmounts_ReturnsZero()
    {
        var result = new Money(50) - new Money(50);
        Assert.Equal(Money.Zero, result);
    }

    [Fact]
    public void Comparison_GreaterThan_Works()
    {
        Assert.True(new Money(100) > new Money(50));
        Assert.False(new Money(50) > new Money(100));
    }

    [Fact]
    public void Comparison_LessThan_Works()
    {
        Assert.True(new Money(10) < new Money(20));
    }

    [Fact]
    public void Equality_SameAmount_Equal()
    {
        Assert.Equal(new Money(42), new Money(42));
        Assert.True(new Money(42) == new Money(42));
    }

    [Fact]
    public void Equality_DifferentAmount_NotEqual()
    {
        Assert.NotEqual(new Money(42), new Money(43));
    }

    [Fact]
    public void GreaterThanOrEqual_EqualValues_ReturnsTrue()
    {
        Assert.True(new Money(50) >= new Money(50));
    }
}
