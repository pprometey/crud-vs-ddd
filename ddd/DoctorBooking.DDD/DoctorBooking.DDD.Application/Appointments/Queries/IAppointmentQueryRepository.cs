using Core.Common.Application.Pagination;
using DoctorBooking.DDD.Application.Appointments.Dtos;

namespace DoctorBooking.DDD.Application.Appointments.Queries;

/// <summary>
/// Query repository for read-only Appointment operations (no tracking, direct DTO projection).
/// </summary>
public interface IAppointmentQueryRepository
{
    Task<AppointmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<AppointmentDto>> GetByPatientPagedAsync(Guid patientId, PageRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<AppointmentDto>> GetByDoctorPagedAsync(Guid doctorId, PageRequest request, CancellationToken cancellationToken = default);
}
