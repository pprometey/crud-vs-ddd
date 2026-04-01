using Core.Common.Domain;
using DoctorBooking.DDD.Domain.Errors;

namespace DoctorBooking.DDD.Domain.Schedules;

public sealed class SlotNotFoundException : DomainException
{
    public SlotNotFoundException(TimeSlotId slotId)
        : base(ErrorCodes.Schedule.SlotNotFound,
               DomainErrors.Schedule.SlotNotFound(slotId.Value).Message) { }
}
