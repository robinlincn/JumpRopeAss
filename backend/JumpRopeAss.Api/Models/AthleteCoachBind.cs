using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 运动员第二/第三教练员绑定
/// </summary>
public partial class AthleteCoachBind
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 运动员人员ID
    /// </summary>
    public ulong AthletePersonId { get; set; }

    /// <summary>
    /// 教练员人员ID
    /// </summary>
    public ulong CoachPersonId { get; set; }

    /// <summary>
    /// 绑定级别：2第二教练；3第三教练
    /// </summary>
    public sbyte BindLevel { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
