namespace ScrollForCause.Api.Common;

public class PagedResponse<T>
{
    public List<T> Data { get; set; } = [];
    public string? Cursor { get; set; }
    public bool HasMore { get; set; }
}
