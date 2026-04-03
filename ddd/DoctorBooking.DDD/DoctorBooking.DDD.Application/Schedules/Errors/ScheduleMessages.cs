using System.Resources;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Application.Schedules.Errors;

internal static class ScheduleMessages
{
    private static readonly ResourceManager Resources =
        new("DoctorBooking.DDD.Application.Schedules.Errors.ScheduleMessages",
            typeof(ScheduleMessages).Assembly);

    internal static string Msg(string code, params object[] args) =>
        FeatureMessages.Msg(Resources, code, args);
}

// ── ErrorCodes ────────────────────────────────────────────────────────────────

public static partial class AppErrorCodes
{
    public static class Schedule
    {
        // Handler errors
        public const string NotFound = "schedule.not_found";
        
        // Validation errors
        public const string DoctorIdRequired = "schedule.doctor_id_required";
        public const string StartRequired = "schedule.start_required";
        public const string DurationPositive = "schedule.duration_positive";
        public const string PriceNonNegative = "schedule.price_non_negative";
        public const string SlotIdRequired = "schedule.slot_id_required";
    }
}
