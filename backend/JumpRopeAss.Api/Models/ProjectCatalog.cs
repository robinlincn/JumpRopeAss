using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 项目字典（来源：项目设定.xlsx）
/// </summary>
public partial class ProjectCatalog
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 项目编码（选填）
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 项目名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 人数（单人/双人/团队）
    /// </summary>
    public int ParticipantCount { get; set; }

    /// <summary>
    /// 状态：1启用；0停用
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
