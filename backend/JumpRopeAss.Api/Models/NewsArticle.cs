namespace JumpRopeAss.Api.Models;

public sealed class NewsArticle
{
    public ulong Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string? Summary { get; set; }
    public string? ContentHtml { get; set; }
    public string ContentType { get; set; } = "text";
    public string? VideoUrl { get; set; }
    public DateTime? PublishAt { get; set; }
    public long ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

