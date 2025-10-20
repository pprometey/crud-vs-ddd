using System.Linq.Expressions;

namespace DoctorBooking.CRUD.Db.Repositories;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>>? include = null, bool asNoTracking = true);
    Task<T?> GetByIdAsync(int id, Func<IQueryable<T>, IQueryable<T>>? include = null);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}
