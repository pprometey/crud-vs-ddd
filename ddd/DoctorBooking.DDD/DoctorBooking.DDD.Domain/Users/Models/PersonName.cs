using Ardalis.GuardClauses;
using DoctorBooking.DDD.Domain.Errors;

namespace DoctorBooking.DDD.Domain.Users;

public readonly record struct PersonName
{
    public string FirstName { get; }
    public string LastName { get; }

    public PersonName(string firstName, string lastName)
    {
        Guard.Against.EmptyFirstName(firstName);
        Guard.Against.EmptyLastName(lastName);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public string FullName => $"{FirstName} {LastName}";

    public override string ToString() => FullName;
}
