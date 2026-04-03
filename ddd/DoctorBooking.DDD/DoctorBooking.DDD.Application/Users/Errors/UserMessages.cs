using System.Resources;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Application.Users.Errors;

internal static class UserMessages
{
    private static readonly ResourceManager Resources =
        new("DoctorBooking.DDD.Application.Users.Errors.UserMessages",
            typeof(UserMessages).Assembly);

    internal static string Msg(string code, params object[] args) =>
        FeatureMessages.Msg(Resources, code, args);
}

// ── ErrorCodes ────────────────────────────────────────────────────────────────

public static partial class AppErrorCodes
{
    public static class User
    {
        // Handler errors
        public const string NotFound = "user.not_found";
        
        // Validation errors
        public const string IdRequired = "user.id_required";
        public const string EmailRequired = "user.email_required";
        public const string EmailInvalidFormat = "user.email_invalid_format";
        public const string FirstNameRequired = "user.first_name_required";
        public const string LastNameRequired = "user.last_name_required";
        public const string RoleRequired = "user.role_required";
        public const string RoleInvalid = "user.role_invalid";
    }
}
