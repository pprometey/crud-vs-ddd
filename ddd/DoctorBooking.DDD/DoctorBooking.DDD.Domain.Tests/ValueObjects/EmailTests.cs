using DoctorBooking.DDD.Domain.Users;
using Xunit;

namespace DoctorBooking.DDD.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("  User@Example.Com  ")]
    public void Constructor_ValidEmail_Succeeds(string input)
    {
        var email = new Email(input);
        Assert.Equal(input.Trim().ToLowerInvariant(), email.Value);
    }

    [Fact]
    public void Constructor_NormalizesToLowercase()
    {
        var email = new Email("DOCTOR@HOSPITAL.ORG");
        Assert.Equal("doctor@hospital.org", email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyOrWhitespace_Throws(string input)
    {
        Assert.Throws<ArgumentException>(() => new Email(input));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain")]
    [InlineData("missing@")]
    [InlineData("double@@at.com")]
    public void Constructor_InvalidFormat_Throws(string input)
    {
        Assert.Throws<ArgumentException>(() => new Email(input));
    }

    [Fact]
    public void Equality_SameValue_Equal()
    {
        Assert.Equal(new Email("a@b.com"), new Email("A@B.COM"));
    }

    [Fact]
    public void ToString_ReturnsNormalizedValue()
    {
        var email = new Email("User@Test.Com");
        Assert.Equal("user@test.com", email.ToString());
    }
}
