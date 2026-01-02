namespace Base.Domain.Entities;

/// <summary>
/// INTERFACE FOR ENTITIES THAT SUPPORT AUDIT LOGGING
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// THE DATE AND TIME WHEN THE ENTITY WAS CREATED (UTC)
    /// </summary>
    DateTime CreatedAt { get; set; }
}
