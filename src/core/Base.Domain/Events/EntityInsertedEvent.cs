using Base.Domain.Entities;

namespace Base.Domain.Events;

public class EntityInsertedEvent<T, TId>(T entity) where T : BaseEntity<TId>
{
    public T Entity { get; } = entity;
}
