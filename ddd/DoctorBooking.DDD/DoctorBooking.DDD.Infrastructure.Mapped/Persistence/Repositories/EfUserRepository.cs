using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.DDD.Infrastructure.Mapped.Persistence.Repositories;

/// <summary>
/// Repository с маппингом Domain ↔ DbModel для User aggregate
/// </summary>
public sealed class EfUserRepository(AppDbContext db) : IUserRepository
{
    public UserAgg? FindById(UserId id)
    {
        var dbModel = db.Users
            .Include(u => u.Roles)
            .FirstOrDefault(u => u.Id == id.Value);

        return dbModel == null ? null : UserMapper.ToDomain(dbModel);
    }

    public UserAgg? FindByEmail(Email email)
    {
        var dbModel = db.Users
            .Include(u => u.Roles)
            .FirstOrDefault(u => u.Email == email.Value);

        return dbModel == null ? null : UserMapper.ToDomain(dbModel);
    }

    public void Save(UserAgg user)
    {
        var dbModel = UserMapper.ToDbModel(user);

        var existing = db.Users
            .Include(u => u.Roles)
            .FirstOrDefault(u => u.Id == user.Id.Value);

        if (existing == null)
        {
            db.Users.Add(dbModel);
        }
        else
        {
            existing.Email = dbModel.Email;
            existing.FirstName = dbModel.FirstName;
            existing.LastName = dbModel.LastName;
            // Version is NOT copied - EF will handle concurrency automatically

            db.Entry(existing).State = EntityState.Modified; // Ensure Version increments

            db.UserRoles.RemoveRange(existing.Roles);
            foreach (var role in dbModel.Roles)
            {
                role.UserId = existing.Id;
                db.UserRoles.Add(role);
            }
        }
    }
}
