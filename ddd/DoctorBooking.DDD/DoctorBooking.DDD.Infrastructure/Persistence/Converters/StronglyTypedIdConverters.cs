using DoctorBooking.DDD.Domain.Appointments;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DoctorBooking.DDD.Infrastructure.Persistence.Converters;

public sealed class UserIdConverter() : ValueConverter<UserId, Guid>(
    id => id.Value,
    value => new UserId(value));

public sealed class ScheduleIdConverter() : ValueConverter<ScheduleId, Guid>(
    id => id.Value,
    value => new ScheduleId(value));

public sealed class TimeSlotIdConverter() : ValueConverter<TimeSlotId, Guid>(
    id => id.Value,
    value => new TimeSlotId(value));

public sealed class AppointmentIdConverter() : ValueConverter<AppointmentId, Guid>(
    id => id.Value,
    value => new AppointmentId(value));

public sealed class PaymentIdConverter() : ValueConverter<PaymentId, Guid>(
    id => id.Value,
    value => new PaymentId(value));

public sealed class EmailConverter() : ValueConverter<Email, string>(
    email => email.Value,
    value => new Email(value));

public sealed class MoneyConverter() : ValueConverter<Money, decimal>(
    money => money.Amount,
    value => new Money(value));

public sealed class TimeSpanToTicksConverter() : ValueConverter<TimeSpan, long>(
    ts => ts.Ticks,
    ticks => TimeSpan.FromTicks(ticks));
