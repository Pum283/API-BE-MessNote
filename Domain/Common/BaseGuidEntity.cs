namespace Domain.Common
{
    /// <summary>
    /// Base entity với Guid Id
    /// </summary>
    public abstract class BaseGuidEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

