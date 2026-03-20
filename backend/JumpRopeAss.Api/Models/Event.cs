﻿﻿﻿using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 活动表（赛事/评定/培训统一建模）
/// </summary>
public partial class Event
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 活动类型：1赛事；2评定；3培训
    /// </summary>
    public sbyte EventType { get; set; }

    /// <summary>
    /// 报名范围：1开放型；2封闭型（预留）
    /// </summary>
    public sbyte SignupScope { get; set; }

    /// <summary>
    /// 封闭型限定机构ID（预留）
    /// </summary>
    public ulong? LimitOrgId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 封面图URL
    /// </summary>
    public string? CoverUrl { get; set; }

    public string? LogoUrl { get; set; }

    public string? BannerUrl { get; set; }

    public string? Slogan { get; set; }

    public string? HostOrg { get; set; }

    public string? CoOrgs { get; set; }

    public string? Contacts { get; set; }

    public string? Projects { get; set; }

    /// <summary>
    /// 是否需要审核：1是；0否
    /// </summary>
    public sbyte NeedAudit { get; set; }

    /// <summary>
    /// 是否需要缴费：1是；0否
    /// </summary>
    public sbyte NeedPay { get; set; }

    /// <summary>
    /// 报名开始时间
    /// </summary>
    public DateTime? SignupStartAt { get; set; }

    /// <summary>
    /// 报名结束时间
    /// </summary>
    public DateTime? SignupEndAt { get; set; }

    /// <summary>
    /// 活动日期
    /// </summary>
    public DateOnly? EventDate { get; set; }

    public DateTime? EventStartAt { get; set; }
    public DateTime? EventEndAt { get; set; }

    /// <summary>
    /// 地点
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 详情介绍（HTML）
    /// </summary>
    public string? DescriptionHtml { get; set; }

    /// <summary>
    /// 状态：0草稿；1报名中；2报名截止；3进行中；4已结束；5下线
    /// </summary>
    public sbyte Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
