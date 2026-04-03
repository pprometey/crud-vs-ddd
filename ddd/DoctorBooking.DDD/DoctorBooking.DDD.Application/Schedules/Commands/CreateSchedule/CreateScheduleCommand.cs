using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;

namespace DoctorBooking.DDD.Application.Schedules.Commands.CreateSchedule;

public sealed record CreateScheduleCommand(Guid DoctorId) : ICommand<Result<Guid>>;
