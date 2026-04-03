using DoctorBooking.DDD.Domain.Users;
using DoctorBooking.DDD.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.DDD.Infrastructure.Persistence.Repositories;

public sealed class EfUserRepository(AppDbContext db) : IUserRepository
{
    public UserAgg? FindById(UserId id)
    {
        var user = db.Users.FirstOrDefault(u => u.Id == id);
        if (user is not null)
            HydrateRoles(user);
        return user;
    }

    public UserAgg? FindByEmail(Email email)
    {
        var user = db.Users.FirstOrDefault(u => u.Email == email);
        if (user is not null)
            HydrateRoles(user);
        return user;
    }

    public void Save(UserAgg user)
    {
        if (db.Entry(user).State == EntityState.Detached)
            db.Users.Add(user);
        else
            db.Entry(user).State = EntityState.Modified; // Ensure Version increments

        SyncRoles(user);
    }

    private void HydrateRoles(UserAgg user)
    {
        var roleEntries = db.UserRoles
            .Where(r => r.UserId == user.Id)
            .Select(r => r.Role)
            .ToList();

        // Use reflection to set the private _roles field from DB
#pragma warning disable S3011 // Accessibility bypass is by design for ORM hydration without public setters
        var rolesField = typeof(UserAgg)
            .GetField("_roles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
#pragma warning restore S3011
        var roles = (HashSet<UserRole>)rolesField.GetValue(user)!;
        roles.Clear();
        foreach (var role in roleEntries)
            roles.Add(role);
    }

    private void SyncRoles(UserAgg user)
    {
        var existingEntries = db.UserRoles
            .Where(r => r.UserId == user.Id)
            .ToList();

        var currentRoles = user.Roles;

        // Remove roles that no longer exist
        foreach (var entry in existingEntries.Where(e => !currentRoles.Contains(e.Role)))
            db.UserRoles.Remove(entry);

        // Add new roles
        var existingRoleSet = existingEntries.Select(e => e.Role).ToHashSet();
        foreach (var role in currentRoles.Where(r => !existingRoleSet.Contains(r)))
            db.UserRoles.Add(new UserRoleEntry { UserId = user.Id, Role = role });
    }
}
