using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DoctorBooking.CRUD.Db.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly MedicalBookingContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(MedicalBookingContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>>? include = null, bool asNoTracking = true)
    {
        IQueryable<T> query = _dbSet;
        if (include != null) query = include(query);
        if (asNoTracking) query = query.AsNoTracking();
        return await query.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id, Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        IQueryable<T> query = _dbSet;
        if (include != null) query = include(query);
        return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }
}
