using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using DoctorBooking.DDD.Application.Schedules.Dtos;

namespace DoctorBooking.DDD.Application.Schedules.Queries.GetScheduleByDoctor;

public sealed record GetScheduleByDoctorQuery(Guid DoctorId) : IQuery<Result<ScheduleDto>>;
