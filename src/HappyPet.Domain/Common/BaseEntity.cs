namespace HappyPet.Domain.Common;

/// <summary>
/// Base type for all domain entities. Provides the identity and audit fields.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
