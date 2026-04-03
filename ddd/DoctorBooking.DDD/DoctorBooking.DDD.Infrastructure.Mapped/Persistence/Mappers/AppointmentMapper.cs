using System.Reflection;
using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Models;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Mappers;

public static class AppointmentMapper
{
    public static AppointmentDbModel ToDbModel(AppointmentAgg aggregate)
    {
        return new AppointmentDbModel
        {
            Id = aggregate.Id.Value,
            SlotId = aggregate.SlotId.Value,
            PatientId = aggregate.PatientId.Value,
            DoctorId = aggregate.DoctorId.Value,
            SlotStart = aggregate.SlotStart,
            SlotPriceAmount = aggregate.SlotPrice.Amount,
            Status = (int)aggregate.Status,
            Version = aggregate.Version,
            Payments = aggregate.Payments.Select(ToDbModel).ToList()
        };
    }

    public static AppointmentAgg ToDomain(AppointmentDbModel dbModel)
    {
        // Step 1: Create aggregate (Status = Planned)
        var aggregate = new AppointmentAgg(
            new AppointmentId(dbModel.Id),
            new TimeSlotId(dbModel.SlotId),
            new UserId(dbModel.PatientId),
            new UserId(dbModel.DoctorId),
            dbModel.SlotStart,
            new Money(dbModel.SlotPriceAmount));

        // Step 2: Replay payments (may auto-transition to Confirmed if fully paid)
        foreach (var paymentDb in dbModel.Payments)
        {
            aggregate.AddPayment(
                new PaymentId(paymentDb.Id),
                new Money(paymentDb.Amount),
                paymentDb.PaidAt);
        }

        // Step 3: Apply final status transition if needed
        var targetStatus = (AppointmentStatus)dbModel.Status;

        if (targetStatus == AppointmentStatus.Confirmed && aggregate.Status == AppointmentStatus.Planned)
        {
            // Free appointment was confirmed
            aggregate.ConfirmFree();
        }
        else if (targetStatus == AppointmentStatus.Completed)
        {
            aggregate.Complete();
        }
        else if (targetStatus == AppointmentStatus.Cancelled)
        {
            // For hydration, use dummy values - this is restoring past state
            aggregate.Cancel(new UserId(dbModel.PatientId), aggregate.SlotStart.AddDays(-1));
        }

        HydrateVersion(aggregate, dbModel.Version);
        return aggregate;
    }

    private static void HydrateVersion(AppointmentAgg aggregate, int version)
    {
#pragma warning disable S3011 // Accessibility bypass is by design for ORM hydration without public setters
        typeof(AggregateRoot<AppointmentId>)
            .GetProperty(nameof(AggregateRoot<AppointmentId>.Version))!
            .SetValue(aggregate, version);
#pragma warning restore S3011
    }

    private static PaymentDbModel ToDbModel(Payment payment)
    {
        return new PaymentDbModel
        {
            Id = payment.Id.Value,
            Amount = payment.Amount.Amount,
            PaidAt = payment.PaidAt,
            Status = (int)payment.Status
        };
    }
}
