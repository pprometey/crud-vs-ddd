using Core.Common.Application;
using Core.Common.Application.Results;
using Core.Common.Application.CQRS;
using Core.Common.Application.Persistence;
using DoctorBooking.DDD.Domain.Errors;
using DoctorBooking.DDD.Domain.Schedules;
using DoctorBooking.DDD.Domain.Users;

namespace DoctorBooking.DDD.Application.Schedules.Commands.CreateSchedule;

public sealed class CreateScheduleHandler : Core.Common.Application.CQRS.ICommandHandler<CreateScheduleCommand, Result<Guid>>
{
    private readonly IScheduleRepository _scheduleRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _uow;

    public CreateScheduleHandler(
        IScheduleRepository scheduleRepo,
        IUserRepository userRepo,
        IUnitOfWork uow)
    {
        _scheduleRepo = scheduleRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async ValueTask<Result<Guid>> Handle(
        CreateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var doctorId = new UserId(command.DoctorId);

        // Verify doctor exists and has Doctor role
        var doctor = _userRepo.FindById(doctorId)
            ?? throw DomainErrors.User.NotFound(doctorId.Value);

        if (!doctor.IsDoctor())
        {
            return Result<Guid>.Failure(new ValidationError(
                nameof(command.DoctorId),
                "schedule.user_not_doctor",
                "User must have Doctor role to create a schedule"));
        }

        // Check if schedule already exists
        var existing = _scheduleRepo.FindByDoctor(doctorId);
        if (existing is not null)
        {
            return Result<Guid>.Failure(new ValidationError(
                nameof(command.DoctorId),
                "schedule.already_exists",
                "Schedule already exists for this doctor"));
        }

        var schedule = new ScheduleAgg(ScheduleId.New(), doctorId);
        _scheduleRepo.Save(schedule);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(schedule.Id.Value);
    }
}
