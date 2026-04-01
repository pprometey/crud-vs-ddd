using System.Resources;
using Ardalis.GuardClauses;
using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Schedules;

namespace DoctorBooking.DDD.Domain.Errors;

file static class ScheduleMessages
{
    private static readonly ResourceManager Resources =
        new("DoctorBooking.DDD.Domain.Schedules.Errors.ScheduleMessages",
            typeof(ScheduleMessages).Assembly);

    internal static string Msg(string code, params object[] args) =>
        FeatureMessages.Msg(Resources, code, args);
}

// ── ErrorCodes ────────────────────────────────────────────────────────────────

public static partial class ErrorCodes
{
    public static class Schedule
    {
        public const string SlotInPast                = "schedule.slot_in_past";
        public const string SlotOverlaps              = "schedule.slot_overlaps";
        public const string SlotNotFound              = "schedule.slot_not_found";
        public const string SlotHasActiveAppointments = "schedule.slot_has_active_appointments";
        public const string SlotAlreadyConfirmed      = "schedule.slot_already_confirmed";
    }
}

// ── DomainErrors ──────────────────────────────────────────────────────────────

public static partial class DomainErrors
{
    public static class Schedule
    {
        public static DomainException SlotInPast() =>
            new(ErrorCodes.Schedule.SlotInPast,
                ScheduleMessages.Msg(ErrorCodes.Schedule.SlotInPast));

        public static DomainException SlotOverlaps() =>
            new(ErrorCodes.Schedule.SlotOverlaps,
                ScheduleMessages.Msg(ErrorCodes.Schedule.SlotOverlaps));

        public static DomainException SlotNotFound(object slotId) =>
            new(ErrorCodes.Schedule.SlotNotFound,
                ScheduleMessages.Msg(ErrorCodes.Schedule.SlotNotFound, slotId));

        public static DomainException SlotHasActiveAppointments(object slotId, int count) =>
            new(ErrorCodes.Schedule.SlotHasActiveAppointments,
                ScheduleMessages.Msg(ErrorCodes.Schedule.SlotHasActiveAppointments, slotId, count));

        public static DomainException SlotAlreadyConfirmed() =>
            new(ErrorCodes.Schedule.SlotAlreadyConfirmed,
                ScheduleMessages.Msg(ErrorCodes.Schedule.SlotAlreadyConfirmed));
    }
}

// ── Guard extensions ──────────────────────────────────────────────────────────

public static partial class DomainGuardExtensions
{
    public static void SlotInPast(this IGuardClause _, DateTime start, DateTime now)
    {
        if (start <= now)
            throw DomainErrors.Schedule.SlotInPast();
    }

    public static void OverlappingSlot(this IGuardClause _, IEnumerable<TimeSlot> slots, DateTime start, TimeSpan duration)
    {
        if (slots.Any(s => s.OverlapsWith(start, duration)))
            throw DomainErrors.Schedule.SlotOverlaps();
    }

    public static void ActiveAppointmentsExist(this IGuardClause _, TimeSlotId slotId, int activeCount)
    {
        if (activeCount > 0)
            throw DomainErrors.Schedule.SlotHasActiveAppointments(slotId, activeCount);
    }
}
