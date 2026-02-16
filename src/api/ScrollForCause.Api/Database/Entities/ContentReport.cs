namespace ScrollForCause.Api.Database.Entities;

public class ContentReport
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterType { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public Guid ContentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "pending";
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ActionTaken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
