namespace Domain.Common
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedDate { get; set; }
        string? DeletedBy { get; set; }
    }
}

