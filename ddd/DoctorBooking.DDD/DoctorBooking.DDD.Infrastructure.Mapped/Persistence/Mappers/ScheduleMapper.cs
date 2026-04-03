using System.Reflection;
using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Mappers;

public static class ScheduleMapper
{
    public static ScheduleDbModel ToDbModel(ScheduleAgg aggregate)
    {
        return new ScheduleDbModel
        {
            Id = aggregate.Id.Value,
            DoctorId = aggregate.DoctorId.Value,
            Version = aggregate.Version,
            Slots = aggregate.Slots.Select(ToDbModel).ToList()
        };
    }

    public static ScheduleAgg ToDomain(ScheduleDbModel dbModel)
    {
        var aggregate = new ScheduleAgg(
            new ScheduleId(dbModel.Id),
            new UserId(dbModel.DoctorId));

        // Rebuild aggregate through public methods
        foreach (var slotDb in dbModel.Slots)
        {
            aggregate.AddSlot(
                new TimeSlotId(slotDb.Id),
                slotDb.Start,
                TimeSpan.FromTicks(slotDb.DurationTicks),
                new Money(slotDb.PriceAmount),
                slotDb.Start.AddDays(-1)); // Use time before slot for hydration
        }

        HydrateVersion(aggregate, dbModel.Version);
        return aggregate;
    }

    private static void HydrateVersion(ScheduleAgg aggregate, int version)
    {
#pragma warning disable S3011 // Accessibility bypass is by design for ORM hydration without public setters
        typeof(AggregateRoot<ScheduleId>)
            .GetProperty(nameof(AggregateRoot<ScheduleId>.Version))!
            .SetValue(aggregate, version);
#pragma warning restore S3011
    }

    private static TimeSlotDbModel ToDbModel(TimeSlot slot)
    {
        return new TimeSlotDbModel
        {
            Id = slot.Id.Value,
            Start = slot.Start,
            DurationTicks = slot.Duration.Ticks,
            PriceAmount = slot.Price.Amount,
            DoctorId = slot.DoctorId.Value
        };
    }
}
