using System.Resources;
using Ardalis.GuardClauses;
using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Users;

namespace DoctorBooking.DDD.Domain.Errors;

file static class UserMessages
{
    private static readonly ResourceManager Resources =
        new("DoctorBooking.DDD.Domain.Users.Errors.UserMessages",
            typeof(UserMessages).Assembly);

    internal static string Msg(string code, params object[] args) =>
        FeatureMessages.Msg(Resources, code, args);
}

// ── ErrorCodes ────────────────────────────────────────────────────────────────

public static partial class ErrorCodes
{
    public static class User
    {
        public const string MustHaveAtLeastOneRole = "user.must_have_at_least_one_role";
        public const string NotAPatient            = "user.not_a_patient";
        public const string NotFound               = "user.not_found";
        public const string EmailEmpty             = "user.email_empty";
        public const string EmailInvalidFormat     = "user.email_invalid_format";
        public const string FirstNameEmpty         = "user.first_name_empty";
        public const string LastNameEmpty          = "user.last_name_empty";
    }
}

// ── DomainErrors ──────────────────────────────────────────────────────────────

public static partial class DomainErrors
{
    public static class User
    {
        public static DomainException MustHaveAtLeastOneRole() =>
            new(ErrorCodes.User.MustHaveAtLeastOneRole,
                UserMessages.Msg(ErrorCodes.User.MustHaveAtLeastOneRole));

        public static DomainException NotAPatient(object userId) =>
            new(ErrorCodes.User.NotAPatient,
                UserMessages.Msg(ErrorCodes.User.NotAPatient, userId));

        public static DomainException NotFound(object userId) =>
            new(ErrorCodes.User.NotFound,
                UserMessages.Msg(ErrorCodes.User.NotFound, userId));

        public static DomainException EmailEmpty() =>
            new(ErrorCodes.User.EmailEmpty,
                UserMessages.Msg(ErrorCodes.User.EmailEmpty));

        public static DomainException EmailInvalidFormat(object value) =>
            new(ErrorCodes.User.EmailInvalidFormat,
                UserMessages.Msg(ErrorCodes.User.EmailInvalidFormat, value));

        public static DomainException FirstNameEmpty() =>
            new(ErrorCodes.User.FirstNameEmpty,
                UserMessages.Msg(ErrorCodes.User.FirstNameEmpty));

        public static DomainException LastNameEmpty() =>
            new(ErrorCodes.User.LastNameEmpty,
                UserMessages.Msg(ErrorCodes.User.LastNameEmpty));
    }
}

// ── Guard extensions ──────────────────────────────────────────────────────────

public static partial class DomainGuardExtensions
{
    public static void LastRoleRemoval(this IGuardClause _, IReadOnlySet<UserRole> roles, UserRole roleBeingRemoved)
    {
        if (roles.Count <= 1 && roles.Contains(roleBeingRemoved))
            throw DomainErrors.User.MustHaveAtLeastOneRole();
    }

    public static void UserNotPatient(this IGuardClause _, UserAgg user)
    {
        if (!user.HasRole(UserRole.Patient))
            throw DomainErrors.User.NotAPatient(user.Id.Value);
    }

    public static void InvalidEmail(this IGuardClause _, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw DomainErrors.User.EmailEmpty();

        var normalized = value.Trim().ToLowerInvariant();
        if (!IsValidEmailFormat(normalized))
            throw DomainErrors.User.EmailInvalidFormat(value);
    }

    public static void EmptyFirstName(this IGuardClause _, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw DomainErrors.User.FirstNameEmpty();
    }

    public static void EmptyLastName(this IGuardClause _, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw DomainErrors.User.LastNameEmpty();
    }

    private static bool IsValidEmailFormat(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
