using System;
using System.Collections.Generic;

namespace JumpRopeAss.Api.Models;

/// <summary>
/// 考核记录归档（报名+成绩+发证信息）
/// </summary>
public partial class AssessmentRecord
{
    /// <summary>
    /// 主键
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// 活动ID（评定/培训）
    /// </summary>
    public ulong? EventId { get; set; }

    /// <summary>
    /// 人员ID
    /// </summary>
    public ulong PersonId { get; set; }

    /// <summary>
    /// 角色类型：1学员；2教练员；3裁判员
    /// </summary>
    public sbyte RoleType { get; set; }

    /// <summary>
    /// 组别
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// 项目
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// 等级/段位
    /// </summary>
    public string? Level { get; set; }

    /// <summary>
    /// 考核状态/结果（通过/未通过等）
    /// </summary>
    public string? ResultStatus { get; set; }

    /// <summary>
    /// 分数/成绩
    /// </summary>
    public int? Score { get; set; }

    /// <summary>
    /// 所属协会
    /// </summary>
    public string? AssociationName { get; set; }

    /// <summary>
    /// 省
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// 市
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// 区县
    /// </summary>
    public string? District { get; set; }

    /// <summary>
    /// 签证人
    /// </summary>
    public string? SignPerson { get; set; }

    /// <summary>
    /// 活动日期
    /// </summary>
    public DateOnly? ActivityDate { get; set; }

    /// <summary>
    /// 发证日期
    /// </summary>
    public DateOnly? IssueDate { get; set; }

    /// <summary>
    /// 有效期文本（如2023/9/2-2027/9/1）
    /// </summary>
    public string? ValidPeriodText { get; set; }

    /// <summary>
    /// 教练员
    /// </summary>
    public string? CoachName { get; set; }

    /// <summary>
    /// 地点/活动地点
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 证书编号
    /// </summary>
    public string? CertNo { get; set; }

    /// <summary>
    /// 称号（如初级教练员、三级裁判员等）
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 所属单位/学校
    /// </summary>
    public string? OrgName { get; set; }

    /// <summary>
    /// 活动名称
    /// </summary>
    public string? ActivityName { get; set; }

    /// <summary>
    /// 发证机构
    /// </summary>
    public string? IssuerOrg { get; set; }

    /// <summary>
    /// 推荐人
    /// </summary>
    public string? Recommender { get; set; }

    /// <summary>
    /// 推荐人电话
    /// </summary>
    public string? RecommenderPhone { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
