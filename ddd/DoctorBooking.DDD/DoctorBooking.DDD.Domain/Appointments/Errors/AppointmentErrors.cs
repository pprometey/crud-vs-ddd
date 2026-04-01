using System.Resources;
using Ardalis.GuardClauses;
using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Users;

namespace DoctorBooking.DDD.Domain.Errors;

file static class AppointmentMessages
{
    private static readonly ResourceManager Resources =
        new("DoctorBooking.DDD.Domain.Appointments.Errors.AppointmentMessages",
            typeof(AppointmentMessages).Assembly);

    internal static string Msg(string code, params object[] args) =>
        FeatureMessages.Msg(Resources, code, args);
}

// ── ErrorCodes ────────────────────────────────────────────────────────────────

public static partial class ErrorCodes
{
    public static class Appointment
    {
        public const string PatientIsOwnDoctor       = "appointment.patient_is_own_doctor";
        public const string PaymentNotAllowed        = "appointment.payment_not_allowed";
        public const string PaymentExceedsSlotPrice  = "appointment.payment_exceeds_slot_price";
        public const string CannotCancel             = "appointment.cannot_cancel";
        public const string AlreadyStarted           = "appointment.already_started";
        public const string NotConfirmedForComplete  = "appointment.not_confirmed_for_complete";
        public const string NotConfirmedForNoShow    = "appointment.not_confirmed_for_no_show";
        public const string FreeConfirmWithPrice     = "appointment.free_confirm_with_price";
        public const string FreeConfirmWrongStatus   = "appointment.free_confirm_wrong_status";
    }

    public static class Money
    {
        public const string Negative                 = "money.negative";
        public const string SubtractionNegative      = "money.subtraction_negative";
        public const string PaymentAmountZero        = "money.payment_amount_zero";
    }
}

// ── DomainErrors ──────────────────────────────────────────────────────────────

public static partial class DomainErrors
{
    public static class Appointment
    {
        public static DomainException PatientCannotBeDoctor() =>
            new(ErrorCodes.Appointment.PatientIsOwnDoctor,
                AppointmentMessages.Msg(ErrorCodes.Appointment.PatientIsOwnDoctor));

        public static DomainException PaymentNotAllowed(object status) =>
            new(ErrorCodes.Appointment.PaymentNotAllowed,
                AppointmentMessages.Msg(ErrorCodes.Appointment.PaymentNotAllowed, status));

        public static DomainException PaymentExceedsSlotPrice(object amount, object slotPrice, object remaining) =>
            new(ErrorCodes.Appointment.PaymentExceedsSlotPrice,
                AppointmentMessages.Msg(ErrorCodes.Appointment.PaymentExceedsSlotPrice, amount, slotPrice, remaining));

        public static DomainException CannotCancel(object status) =>
            new(ErrorCodes.Appointment.CannotCancel,
                AppointmentMessages.Msg(ErrorCodes.Appointment.CannotCancel, status));

        public static DomainException AlreadyStarted() =>
            new(ErrorCodes.Appointment.AlreadyStarted,
                AppointmentMessages.Msg(ErrorCodes.Appointment.AlreadyStarted));

        public static DomainException NotConfirmedForComplete(object status) =>
            new(ErrorCodes.Appointment.NotConfirmedForComplete,
                AppointmentMessages.Msg(ErrorCodes.Appointment.NotConfirmedForComplete, status));

        public static DomainException NotConfirmedForNoShow(object status) =>
            new(ErrorCodes.Appointment.NotConfirmedForNoShow,
                AppointmentMessages.Msg(ErrorCodes.Appointment.NotConfirmedForNoShow, status));

        public static DomainException FreeConfirmWithPrice() =>
            new(ErrorCodes.Appointment.FreeConfirmWithPrice,
                AppointmentMessages.Msg(ErrorCodes.Appointment.FreeConfirmWithPrice));

        public static DomainException FreeConfirmWrongStatus(object status) =>
            new(ErrorCodes.Appointment.FreeConfirmWrongStatus,
                AppointmentMessages.Msg(ErrorCodes.Appointment.FreeConfirmWrongStatus, status));
    }

    public static class Money
    {
        public static DomainException Negative() =>
            new(ErrorCodes.Money.Negative,
                AppointmentMessages.Msg(ErrorCodes.Money.Negative));

        public static DomainException SubtractionNegative(object a, object b) =>
            new(ErrorCodes.Money.SubtractionNegative,
                AppointmentMessages.Msg(ErrorCodes.Money.SubtractionNegative, a, b));

        public static DomainException PaymentAmountZero() =>
            new(ErrorCodes.Money.PaymentAmountZero,
                AppointmentMessages.Msg(ErrorCodes.Money.PaymentAmountZero));
    }
}

// ── Guard extensions ──────────────────────────────────────────────────────────

public static partial class DomainGuardExtensions
{
    public static void PatientIsOwnDoctor(this IGuardClause _, UserId patientId, UserId doctorId)
    {
        if (patientId == doctorId)
            throw DomainErrors.Appointment.PatientCannotBeDoctor();
    }

    public static void PaymentNotAllowedInStatus(this IGuardClause _, AppointmentStatus status)
    {
        if (!status.AllowsPayment())
            throw DomainErrors.Appointment.PaymentNotAllowed(status);
    }

    public static void PaymentExceedsSlotPrice(this IGuardClause _, Money newTotal, Money slotPrice, Money remaining)
    {
        if (newTotal > slotPrice)
            throw DomainErrors.Appointment.PaymentExceedsSlotPrice(newTotal, slotPrice, remaining);
    }

    public static void AppointmentNotCancellable(this IGuardClause _, AppointmentStatus status)
    {
        if (!status.IsCancellable())
            throw DomainErrors.Appointment.CannotCancel(status);
    }

    public static void AppointmentAlreadyStarted(this IGuardClause _, DateTime now, DateTime slotStart)
    {
        if (now >= slotStart)
            throw DomainErrors.Appointment.AlreadyStarted();
    }

    public static void NotConfirmedForComplete(this IGuardClause _, AppointmentStatus status)
    {
        if (status != AppointmentStatus.Confirmed)
            throw DomainErrors.Appointment.NotConfirmedForComplete(status);
    }

    public static void NotConfirmedForNoShow(this IGuardClause _, AppointmentStatus status)
    {
        if (status != AppointmentStatus.Confirmed)
            throw DomainErrors.Appointment.NotConfirmedForNoShow(status);
    }

    public static void FreeConfirmWithPrice(this IGuardClause _, Money slotPrice)
    {
        if (!slotPrice.IsZero())
            throw DomainErrors.Appointment.FreeConfirmWithPrice();
    }

    public static void FreeConfirmWrongStatus(this IGuardClause _, AppointmentStatus status)
    {
        if (status != AppointmentStatus.Planned)
            throw DomainErrors.Appointment.FreeConfirmWrongStatus(status);
    }

    public static void NegativeMoney(this IGuardClause _, decimal amount)
    {
        if (amount < 0)
            throw DomainErrors.Money.Negative();
    }

    public static void NegativeMoneySubtraction(this IGuardClause _, decimal a, decimal b)
    {
        if (a - b < 0)
            throw DomainErrors.Money.SubtractionNegative(a, b);
    }

    public static void ZeroPaymentAmount(this IGuardClause _, decimal amount)
    {
        if (amount <= 0)
            throw DomainErrors.Money.PaymentAmountZero();
    }
}
