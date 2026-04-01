using DoctorBooking.DDD.Domain.Users;

namespace DoctorBooking.DDD.Domain.Users;

public interface IUserRepository
{
    UserAgg? FindById(UserId id);
    UserAgg? FindByEmail(Email email);
    void Save(UserAgg user);
}
