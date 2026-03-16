namespace JumpRopeAss.Api.Models;

public sealed class UserIdentitySubmit
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public string RealName { get; set; } = string.Empty;
    public string IdCardNo { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? RejectReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

