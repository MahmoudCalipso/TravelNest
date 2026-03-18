
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TravelNest.Domain.Entities;
using TravelNest.Domain.Interfaces;
using TravelNest.Infrastructure.Data;

namespace TravelNest.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);

        // Check if entity is soft-deleted
        if (entity is BaseEntity baseEntity && baseEntity.IsDeleted)
            return null;

        return entity;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
        => await Query().ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await Query().Where(predicate).ToListAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => await Query().FirstOrDefaultAsync(predicate);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        => await Query().AnyAsync(predicate);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate == null ? await Query().CountAsync() : await Query().CountAsync(predicate);

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
        => await _dbSet.AddRangeAsync(entities);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Remove(T entity)
    {
        // Soft delete: Set IsDeleted = true instead of hard deleting
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.IsDeleted = true;
            baseEntity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
        else
        {
            // Hard delete for entities that don't inherit from BaseEntity
            _dbSet.Remove(entity);
        }
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        // Soft delete: Set IsDeleted = true for all entities
        foreach (var entity in entities)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.IsDeleted = true;
                baseEntity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
            else
            {
                // Hard delete for entities that don't inherit from BaseEntity
                _dbSet.Remove(entity);
            }
        }
    }

    public IQueryable<T> Query()
    {
        var query = _dbSet.AsQueryable();

        // Auto-filter soft-deleted records
        if (typeof(BaseEntity).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => !((BaseEntity)(object)e).IsDeleted);
        }

        return query;
    }
}

