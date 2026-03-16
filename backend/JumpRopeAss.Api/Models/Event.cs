namespace JumpRopeAss.Api.Models;

public sealed class Event
{
    public ulong Id { get; set; }
    public int EventType { get; set; }
    public int SignupScope { get; set; }
    public ulong? LimitOrgId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public DateTime? SignupStartAt { get; set; }
    public DateTime? SignupEndAt { get; set; }
    public DateOnly? EventDate { get; set; }
    public string? Location { get; set; }
    public string? DescriptionHtml { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

