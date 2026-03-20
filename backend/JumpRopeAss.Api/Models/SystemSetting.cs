using System;

namespace JumpRopeAss.Api.Models;

public sealed class SystemSetting
{
    public ulong Id { get; set; }
    public string Key { get; set; } = null!;
    public string ValueJson { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

