using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 活动组别/参赛组别
/// </summary>
public partial class EventGroup
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 活动ID
    /// </summary>
    public ulong EventId { get; set; }

    /// <summary>
    /// 组别编码（A1/A2等）
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 组别名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 报名费（分）
    /// </summary>
    public int FeeAmount { get; set; }

    /// <summary>
    /// 名额（空表示不限）
    /// </summary>
    public int? Quota { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortNo { get; set; }
}
