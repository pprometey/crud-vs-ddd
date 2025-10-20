using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;

namespace DoctorBooking.CRUD.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;

    public UserService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _uow.Users.GetAllAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _uow.Users.GetByIdAsync(id);
    }

    public async Task CreateAsync(User user)
    {
        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _uow.Users.GetByIdAsync(id);
        if (entity != null)
        {
            _uow.Users.Remove(entity);
            await _uow.SaveChangesAsync();
        }
    }
}
