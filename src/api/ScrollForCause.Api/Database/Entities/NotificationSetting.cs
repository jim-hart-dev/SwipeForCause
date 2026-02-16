namespace ScrollForCause.Api.Database.Entities;

public class NotificationSetting
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserType { get; set; } = string.Empty;
    public string NewInterestEmail { get; set; } = "immediate";
    public bool InterestUpdate { get; set; } = true;
    public string NewContentDigest { get; set; } = "weekly";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
