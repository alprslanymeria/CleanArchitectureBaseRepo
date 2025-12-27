using System.Linq.Expressions;
using Base.Domain.Entities;

namespace Base.Application.Contracts.Persistence;

public interface IGenericRepository<TEntity, TId> where TEntity : BaseEntity<TId>
{
    /// <summary>
    /// RETURNS A QUERYABLE COLLECTION OF ALL ENTITIES OF TYPE TEntity.
    /// </summary>
    IQueryable<TEntity> GetAll();

    /// <summary>
    /// ASYNCHRONOUSLY RETRIEVES AN ENTITY BY ITS UNIQUE IDENTIFIER.
    /// </summary>
    ValueTask<TEntity?> GetByIdAsync(int id);

    /// <summary>
    /// ASYNCHRONOUSLY CREATES A NEW ENTITY IN THE UNDERLYING DATA STORE.
    /// </summary>
    ValueTask CreateAsync(TEntity entity);

    /// <summary>
    /// UPDATES THE SPECIFIED ENTITY IN THE UNDERLYING DATA STORE AND RETURNS THE UPDATED ENTITY.
    /// </summary>
    TEntity Update(TEntity entity);

    /// <summary>
    /// REMOVES THE SPECIFIED ENTITY FROM THE UNDERLYING DATA STORE.
    /// </summary>
    void Delete(TEntity entity);

    /// <summary>
    /// FILTERS THE ELEMENTS OF THE SEQUENCE BASED ON A SPECIFIED PREDICATE EXPRESSION.
    /// </summary>
    IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
}