namespace JumpRopeAss.Api.Models;

public sealed class EventEntry
{
    public ulong Id { get; set; }
    public ulong EventId { get; set; }
    public ulong GroupId { get; set; }
    public ulong AthletePersonId { get; set; }
    public int EnrollChannel { get; set; }
    public ulong EnrollUserId { get; set; }
    public int Status { get; set; }
    public string? AuditRemark { get; set; }
    public ulong? PayOrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}

