namespace Base.Domain.Entities;

/// <summary>
/// BASE CLASS FOR ENTITIES THAT SUPPORT AUDIT LOGGING
/// INHERIT FROM THIS CLASS TO AUTOMATICALLY TRACK CREATED/MODIFIED TIMESTAMPS
/// </summary>
public abstract class AuditableEntity<TId> : BaseEntity<TId>, IAuditableEntity
{
    /// <summary>
    /// THE DATE AND TIME WHEN THE ENTITY WAS CREATED (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
