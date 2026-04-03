using System.Resources;
using Core.Common.Domain;

namespace DoctorBooking.DDD.Application.Appointments.Errors;

internal static class AppointmentMessages
{
    private static readonly ResourceManager Resources =
        new("DoctorBooking.DDD.Application.Appointments.Errors.AppointmentMessages",
            typeof(AppointmentMessages).Assembly);

    internal static string Msg(string code, params object[] args) =>
        FeatureMessages.Msg(Resources, code, args);
}

// ── ErrorCodes ────────────────────────────────────────────────────────────────

public static partial class AppErrorCodes
{
    public static class Appointment
    {
        // Handler errors
        public const string NotFound = "appointment.not_found";
        
        // Validation errors
        public const string IdRequired = "appointment.id_required";
        public const string PatientIdRequired = "appointment.patient_id_required";
        public const string SlotIdRequired = "appointment.slot_id_required";
        public const string AmountPositive = "appointment.amount_positive";
        public const string PaidAtRequired = "appointment.paid_at_required";
        public const string CancelledByRequired = "appointment.cancelled_by_required";
    }
}
