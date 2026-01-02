using Base.Application.Contracts.Persistence;
using Base.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Base.Persistence;

public class GenericRepository<TEntity, TId>(AppDbContext context) : IGenericRepository<TEntity, TId> where TEntity : BaseEntity<TId>
{
    private readonly AppDbContext _context = context;
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public IQueryable<TEntity> GetAll() => _dbSet.AsQueryable().AsNoTracking();

    public async ValueTask<TEntity?> GetByIdAsync(TId id)
    {
        var entity = await _dbSet.FindAsync(id);

        if (entity is null) return entity;

        _context.Entry(entity).State = EntityState.Detached;

        return entity;
    }

    public async ValueTask CreateAsync(TEntity entity) => await _dbSet.AddAsync(entity);

    public TEntity Update(TEntity entity)
    {
        _context.Entry(entity).State = EntityState.Modified;

        return entity;
    }

    public void Delete(TEntity entity) => _dbSet.Remove(entity);

    public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate) => _dbSet.Where(predicate).AsNoTracking();
}
