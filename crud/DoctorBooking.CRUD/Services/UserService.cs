using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Db.Repositories;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DoctorBooking.CRUD.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;

    public UserService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<User>> GetAllAsync()
        => await _uow.Users.GetAllAsync();

    public async Task<User?> GetByIdAsync(int id)
        => await _uow.Users.GetByIdAsync(id);

    public async Task CreateAsync(User user)
    {
        await EnsureEmailUniqueAsync(user);
        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        await EnsureEmailUniqueAsync(user);
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

    // --- Private helpers ---

    private async Task EnsureEmailUniqueAsync(User user)
    {
        var normalized = (user.Email ?? string.Empty).Trim().ToLowerInvariant();
        var exists = await _uow.Users.AnyAsync(u =>
            ((u.Email ?? string.Empty).ToLower()) == normalized &&
            u.Id != user.Id);

        if (exists)
            throw new InvalidOperationException("User.Email must be unique in the system.");
    }
}
