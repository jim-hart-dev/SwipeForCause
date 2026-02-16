namespace ScrollForCause.Api.Database.Entities;

public class PostTag
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public string Tag { get; set; } = string.Empty;
}
