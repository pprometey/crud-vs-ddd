using DoctorBooking.DDD.Domain.Users;

namespace DoctorBooking.DDD.Domain.Tests.Fakes;

public class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<UserId, UserAgg> _store = [];

    public UserAgg? FindById(UserId id)
        => _store.TryGetValue(id, out var u) ? u : null;

    public UserAgg? FindByEmail(Email email)
        => _store.Values.FirstOrDefault(u => u.Email == email);

    public void Save(UserAgg user) => _store[user.Id] = user;
}
