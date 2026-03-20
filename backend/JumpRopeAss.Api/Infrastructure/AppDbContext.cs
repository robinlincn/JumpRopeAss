using System;
using System.Collections.Generic;
using JumpRopeAss.Api.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace JumpRopeAss.Api.Infrastructure;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AboutPage> AboutPages { get; set; }

    public virtual DbSet<AdminRole> AdminRoles { get; set; }

    public virtual DbSet<AdminUser> AdminUsers { get; set; }

    public virtual DbSet<AdminUserRole> AdminUserRoles { get; set; }

    public virtual DbSet<AssessmentRecord> AssessmentRecords { get; set; }

    public virtual DbSet<AthleteAffiliationHistory> AthleteAffiliationHistories { get; set; }

    public virtual DbSet<AthleteCoachBind> AthleteCoachBinds { get; set; }

    public virtual DbSet<AthleteParentBind> AthleteParentBinds { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Banner> Banners { get; set; }

    public virtual DbSet<CertPricing> CertPricings { get; set; }

    public virtual DbSet<CertType> CertTypes { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventEntry> EventEntries { get; set; }

    public virtual DbSet<EventGroup> EventGroups { get; set; }

    public virtual DbSet<LocalAssociation> LocalAssociations { get; set; }

    public virtual DbSet<MemberUnit> MemberUnits { get; set; }

    public virtual DbSet<NewsArticle> NewsArticles { get; set; }

    public virtual DbSet<Org> Orgs { get; set; }

    public virtual DbSet<PayOrder> PayOrders { get; set; }

    public virtual DbSet<PayWechatNotify> PayWechatNotifies { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<PersonAthlete> PersonAthletes { get; set; }

    public virtual DbSet<PersonCoach> PersonCoaches { get; set; }

    public virtual DbSet<PersonJudge> PersonJudges { get; set; }

    public virtual DbSet<PersonParent> PersonParents { get; set; }

    public virtual DbSet<ProjectCatalog> ProjectCatalogs { get; set; }

    public virtual DbSet<School> Schools { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    public virtual DbSet<UserIdentitySubmit> UserIdentitySubmits { get; set; }

    public virtual DbSet<UserWechat> UserWechats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AboutPage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("about_page", tb => tb.HasComment("关于协会页面内容"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Code, "uk_about_code").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasDefaultValueSql("'about'")
                .HasComment("页面编码")
                .HasColumnName("code");
            entity.Property(e => e.ContentHtml)
                .HasComment("内容HTML")
                .HasColumnType("mediumtext")
                .HasColumnName("content_html");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasComment("协会地址")
                .HasColumnName("address");
            entity.Property(e => e.HistoryHtml)
                .HasComment("协会历史HTML")
                .HasColumnType("mediumtext")
                .HasColumnName("history_html");
            entity.Property(e => e.HonorsHtml)
                .HasComment("协会荣誉HTML")
                .HasColumnType("mediumtext")
                .HasColumnName("honors_html");
            entity.Property(e => e.LogoUrl)
                .HasComment("协会Logo（URL或base64 data url）")
                .HasColumnType("mediumtext")
                .HasColumnName("logo_url");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasComment("协会名称")
                .HasColumnName("name");
            entity.Property(e => e.OverviewHtml)
                .HasComment("协会概述HTML")
                .HasColumnType("mediumtext")
                .HasColumnName("overview_html");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasComment("标题")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("system_setting").UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Key, "uk_system_setting_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Key).HasMaxLength(64).HasColumnName("key");
            entity.Property(e => e.ValueJson).HasColumnType("mediumtext").HasColumnName("value_json");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<AdminRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("admin_role", tb => tb.HasComment("后台角色"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Code, "uk_admin_role_code").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(64)
                .HasComment("角色编码")
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasComment("角色名称")
                .HasColumnName("name");
            entity.Property(e => e.PermissionsJson)
                .HasComment("权限点JSON数组")
                .HasColumnType("mediumtext")
                .HasColumnName("permissions_json");
        });

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("admin_user", tb => tb.HasComment("后台用户"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Username, "uk_admin_username").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasComment("密码哈希")
                .HasColumnName("password_hash");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1启用；0停用")
                .HasColumnName("status");
            entity.Property(e => e.Username)
                .HasMaxLength(64)
                .HasComment("账号")
                .HasColumnName("username");
        });

        modelBuilder.Entity<AdminUserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("admin_user_role", tb => tb.HasComment("后台用户-角色关联"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.AdminUserId, e.AdminRoleId }, "uk_admin_user_role").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.AdminRoleId)
                .HasComment("后台角色ID")
                .HasColumnName("admin_role_id");
            entity.Property(e => e.AdminUserId)
                .HasComment("后台用户ID")
                .HasColumnName("admin_user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
        });

        modelBuilder.Entity<AssessmentRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("assessment_record", tb => tb.HasComment("考核记录归档（报名+成绩+发证信息）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.CertNo, "idx_assess_cert_no");

            entity.HasIndex(e => e.EventId, "idx_assess_event");

            entity.HasIndex(e => new { e.PersonId, e.RoleType, e.CreatedAt }, "idx_assess_person_role_time");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.ActivityDate)
                .HasComment("活动日期")
                .HasColumnName("activity_date");
            entity.Property(e => e.ActivityName)
                .HasMaxLength(128)
                .HasComment("活动名称")
                .HasColumnName("activity_name");
            entity.Property(e => e.AssociationName)
                .HasMaxLength(128)
                .HasComment("所属协会")
                .HasColumnName("association_name");
            entity.Property(e => e.CertNo)
                .HasMaxLength(64)
                .HasComment("证书编号")
                .HasColumnName("cert_no");
            entity.Property(e => e.City)
                .HasMaxLength(32)
                .HasComment("市")
                .HasColumnName("city");
            entity.Property(e => e.CoachName)
                .HasMaxLength(64)
                .HasComment("教练员")
                .HasColumnName("coach_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.District)
                .HasMaxLength(32)
                .HasComment("区县")
                .HasColumnName("district");
            entity.Property(e => e.EventId)
                .HasComment("活动ID（评定/培训）")
                .HasColumnName("event_id");
            entity.Property(e => e.GroupName)
                .HasMaxLength(64)
                .HasComment("组别")
                .HasColumnName("group_name");
            entity.Property(e => e.IssueDate)
                .HasComment("发证日期")
                .HasColumnName("issue_date");
            entity.Property(e => e.IssuerOrg)
                .HasMaxLength(128)
                .HasComment("发证机构")
                .HasColumnName("issuer_org");
            entity.Property(e => e.Level)
                .HasMaxLength(32)
                .HasComment("等级/段位")
                .HasColumnName("level");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasComment("地点/活动地点")
                .HasColumnName("location");
            entity.Property(e => e.OrgName)
                .HasMaxLength(128)
                .HasComment("所属单位/学校")
                .HasColumnName("org_name");
            entity.Property(e => e.PersonId)
                .HasComment("人员ID")
                .HasColumnName("person_id");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(128)
                .HasComment("项目")
                .HasColumnName("project_name");
            entity.Property(e => e.Province)
                .HasMaxLength(32)
                .HasComment("省")
                .HasColumnName("province");
            entity.Property(e => e.Recommender)
                .HasMaxLength(64)
                .HasComment("推荐人")
                .HasColumnName("recommender");
            entity.Property(e => e.RecommenderPhone)
                .HasMaxLength(32)
                .HasComment("推荐人电话")
                .HasColumnName("recommender_phone");
            entity.Property(e => e.ResultStatus)
                .HasMaxLength(16)
                .HasComment("考核状态/结果（通过/未通过等）")
                .HasColumnName("result_status");
            entity.Property(e => e.RoleType)
                .HasComment("角色类型：1学员；2教练员；3裁判员")
                .HasColumnName("role_type");
            entity.Property(e => e.Score)
                .HasComment("分数/成绩")
                .HasColumnName("score");
            entity.Property(e => e.SignPerson)
                .HasMaxLength(64)
                .HasComment("签证人")
                .HasColumnName("sign_person");
            entity.Property(e => e.Title)
                .HasMaxLength(64)
                .HasComment("称号（如初级教练员、三级裁判员等）")
                .HasColumnName("title");
            entity.Property(e => e.ValidPeriodText)
                .HasMaxLength(64)
                .HasComment("有效期文本（如2023/9/2-2027/9/1）")
                .HasColumnName("valid_period_text");
        });

        modelBuilder.Entity<AthleteAffiliationHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("athlete_affiliation_history", tb => tb.HasComment("运动员学校/培训机构变更历史（留痕可追溯）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.AthletePersonId, "idx_aff_hist_athlete");

            entity.HasIndex(e => new { e.ChangeType, e.CreatedAt }, "idx_aff_hist_type_time");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.AfterOrgId)
                .HasComment("变更后机构ID")
                .HasColumnName("after_org_id");
            entity.Property(e => e.AthletePersonId)
                .HasComment("运动员人员ID")
                .HasColumnName("athlete_person_id");
            entity.Property(e => e.BeforeOrgId)
                .HasComment("变更前机构ID")
                .HasColumnName("before_org_id");
            entity.Property(e => e.ChangeType)
                .HasComment("变更类型：1学校；2培训机构")
                .HasColumnName("change_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OperatorType)
                .HasComment("操作人类型：1后台管理员；2本人；3家长；4教练")
                .HasColumnName("operator_type");
            entity.Property(e => e.OperatorUserId)
                .HasComment("操作人账号ID")
                .HasColumnName("operator_user_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasComment("变更原因")
                .HasColumnName("reason");
        });

        modelBuilder.Entity<AthleteCoachBind>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("athlete_coach_bind", tb => tb.HasComment("运动员第二/第三教练员绑定"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.CoachPersonId, "idx_bind_coach");

            entity.HasIndex(e => new { e.AthletePersonId, e.BindLevel }, "uk_athlete_bind_level").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.AthletePersonId)
                .HasComment("运动员人员ID")
                .HasColumnName("athlete_person_id");
            entity.Property(e => e.BindLevel)
                .HasComment("绑定级别：2第二教练；3第三教练")
                .HasColumnName("bind_level");
            entity.Property(e => e.CoachPersonId)
                .HasComment("教练员人员ID")
                .HasColumnName("coach_person_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
        });

        modelBuilder.Entity<AthleteParentBind>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("athlete_parent_bind", tb => tb.HasComment("运动员-家长绑定（支持多个家长）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.AthletePersonId, e.ParentPersonId }, "uk_athlete_parent").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.AthletePersonId)
                .HasComment("运动员人员ID")
                .HasColumnName("athlete_person_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ParentPersonId)
                .HasComment("家长人员ID")
                .HasColumnName("parent_person_id");
            entity.Property(e => e.Relation)
                .HasMaxLength(16)
                .HasComment("关系：父亲/母亲/监护人等")
                .HasColumnName("relation");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1有效；0无效")
                .HasColumnName("status");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("audit_log", tb => tb.HasComment("操作审计日志"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.ActorType, e.ActorId, e.CreatedAt }, "idx_audit_actor_time");

            entity.HasIndex(e => new { e.BizType, e.BizId }, "idx_audit_biz");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(64)
                .HasComment("动作编码（如identity_approve/first_coach_change）")
                .HasColumnName("action");
            entity.Property(e => e.ActorId)
                .HasComment("操作人ID（后台用户ID或账号ID）")
                .HasColumnName("actor_id");
            entity.Property(e => e.ActorType)
                .HasComment("操作人类型：1后台；2小程序")
                .HasColumnName("actor_type");
            entity.Property(e => e.BizId)
                .HasComment("业务ID")
                .HasColumnName("biz_id");
            entity.Property(e => e.BizType)
                .HasMaxLength(32)
                .HasComment("业务类型（如event_entry/user_identity）")
                .HasColumnName("biz_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DetailJson)
                .HasComment("详情JSON（变更前后摘要）")
                .HasColumnType("mediumtext")
                .HasColumnName("detail_json");
        });

        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("banner", tb => tb.HasComment("Banner配置"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.Position, e.Status }, "idx_banner_pos_status");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl)
                .HasComment("图片URL")
                .HasColumnType("mediumtext")
                .HasColumnName("image_url");
            entity.Property(e => e.VideoUrl)
                .HasComment("视频URL")
                .HasColumnType("mediumtext")
                .HasColumnName("video_url");
            entity.Property(e => e.MediaType)
                .HasMaxLength(32)
                .HasDefaultValueSql("'image'")
                .HasComment("类型：image/video")
                .HasColumnName("media_type");
            entity.Property(e => e.LinkType)
                .HasMaxLength(16)
                .HasComment("跳转类型：none/news/event/url")
                .HasColumnName("link_type");
            entity.Property(e => e.LinkValue)
                .HasMaxLength(512)
                .HasComment("跳转值（ID或URL）")
                .HasColumnName("link_value");
            entity.Property(e => e.Position)
                .HasMaxLength(32)
                .HasDefaultValueSql("'home'")
                .HasComment("位置：home等")
                .HasColumnName("position");
            entity.Property(e => e.SortNo)
                .HasComment("排序号")
                .HasColumnName("sort_no");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1启用；0停用")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasComment("标题")
                .HasColumnName("title");
        });

        modelBuilder.Entity<CertPricing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("cert_pricing", tb => tb.HasComment("证书定价（首次/补证分开定价）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.CertTypeId, e.IssueScene, e.EffectiveFrom }, "idx_pricing_type_scene_time");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasComment("价格（分）")
                .HasColumnName("amount");
            entity.Property(e => e.CertTypeId)
                .HasComment("证书类型ID")
                .HasColumnName("cert_type_id");
            entity.Property(e => e.EffectiveFrom)
                .HasComment("生效时间")
                .HasColumnType("datetime")
                .HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo)
                .HasComment("失效时间")
                .HasColumnType("datetime")
                .HasColumnName("effective_to");
            entity.Property(e => e.IssueScene)
                .HasComment("发证场景：1首次；2补证")
                .HasColumnName("issue_scene");
        });

        modelBuilder.Entity<CertType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("cert_type", tb => tb.HasComment("证书类型表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Code, "uk_cert_type_code").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasComment("类型编码：coach_cert/athlete_level等")
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasComment("证书类型名称")
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1启用；0停用")
                .HasColumnName("status");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("certificate", tb => tb.HasComment("证书表（支持电子版展示）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.HolderPersonId, e.CertTypeId }, "idx_cert_holder");

            entity.HasIndex(e => e.CertNo, "uk_cert_no").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.CertNo)
                .HasMaxLength(64)
                .HasComment("证书编号（唯一）")
                .HasColumnName("cert_no");
            entity.Property(e => e.CertTypeId)
                .HasComment("证书类型ID")
                .HasColumnName("cert_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(512)
                .HasComment("电子证书文件URL（PDF/图片）")
                .HasColumnName("file_url");
            entity.Property(e => e.HolderPersonId)
                .HasComment("持证人员ID")
                .HasColumnName("holder_person_id");
            entity.Property(e => e.IssueAt)
                .HasComment("发证时间")
                .HasColumnType("datetime")
                .HasColumnName("issue_at");
            entity.Property(e => e.IssueScene)
                .HasComment("发证场景：1首次；2补证")
                .HasColumnName("issue_scene");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1有效；2作废")
                .HasColumnName("status");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("event", tb => tb.HasComment("活动表（赛事/评定/培训统一建模）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.SignupStartAt, e.SignupEndAt }, "idx_event_signup_time");

            entity.HasIndex(e => new { e.EventType, e.Status }, "idx_event_type_status");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.CoverUrl)
                .HasComment("封面图URL")
                .HasColumnType("mediumtext")
                .HasColumnName("cover_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DescriptionHtml)
                .HasComment("详情介绍（HTML）")
                .HasColumnType("mediumtext")
                .HasColumnName("description_html");
            entity.Property(e => e.EventDate)
                .HasComment("活动日期")
                .HasColumnName("event_date");
            entity.Property(e => e.EventType)
                .HasComment("活动类型：1赛事；2评定；3培训")
                .HasColumnName("event_type");
            entity.Property(e => e.LimitOrgId)
                .HasComment("封闭型限定机构ID（预留）")
                .HasColumnName("limit_org_id");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasComment("地点")
                .HasColumnName("location");
            entity.Property(e => e.LogoUrl)
                .HasComment("活动Logo")
                .HasColumnType("mediumtext")
                .HasColumnName("logo_url");
            entity.Property(e => e.BannerUrl)
                .HasComment("活动Banner")
                .HasColumnType("mediumtext")
                .HasColumnName("banner_url");
            entity.Property(e => e.Slogan)
                .HasMaxLength(255)
                .HasComment("活动标语")
                .HasColumnName("slogan");
            entity.Property(e => e.HostOrg)
                .HasMaxLength(128)
                .HasComment("主办单位")
                .HasColumnName("host_org");
            entity.Property(e => e.CoOrgs)
                .HasColumnType("json")
                .HasComment("协办单位")
                .HasColumnName("co_orgs");
            entity.Property(e => e.Contacts)
                .HasColumnType("json")
                .HasComment("联系人信息")
                .HasColumnName("contacts");
            entity.Property(e => e.Projects)
                .HasColumnType("json")
                .HasComment("比赛项目")
                .HasColumnName("projects");
            entity.Property(e => e.EventStartAt)
                .HasComment("活动开始时间")
                .HasColumnType("datetime")
                .HasColumnName("event_start_at");
            entity.Property(e => e.EventEndAt)
                .HasComment("活动结束时间")
                .HasColumnType("datetime")
                .HasColumnName("event_end_at");
            entity.Property(e => e.NeedAudit)
                .HasDefaultValueSql("'1'")
                .HasComment("是否需要审核：1是；0否")
                .HasColumnName("need_audit");
            entity.Property(e => e.NeedPay)
                .HasDefaultValueSql("'1'")
                .HasComment("是否需要缴费：1是；0否")
                .HasColumnName("need_pay");
            entity.Property(e => e.SignupEndAt)
                .HasComment("报名结束时间")
                .HasColumnType("datetime")
                .HasColumnName("signup_end_at");
            entity.Property(e => e.SignupScope)
                .HasDefaultValueSql("'1'")
                .HasComment("报名范围：1开放型；2封闭型（预留）")
                .HasColumnName("signup_scope");
            entity.Property(e => e.SignupStartAt)
                .HasComment("报名开始时间")
                .HasColumnType("datetime")
                .HasColumnName("signup_start_at");
            entity.Property(e => e.Status)
                .HasComment("状态：0草稿；1报名中；2报名截止；3进行中；4已结束；5下线")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(128)
                .HasComment("标题")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<EventEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("event_entry", tb => tb.HasComment("活动报名表（状态机驱动）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.AthletePersonId, "idx_entry_athlete");

            entity.HasIndex(e => new { e.EventId, e.Status }, "idx_entry_event_status");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.AthletePersonId)
                .HasComment("运动员人员ID")
                .HasColumnName("athlete_person_id");
            entity.Property(e => e.AuditRemark)
                .HasMaxLength(255)
                .HasComment("审核备注/驳回原因")
                .HasColumnName("audit_remark");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EnrollChannel)
                .HasComment("报名方式：1运动员；2家长；3教练（第一教练）")
                .HasColumnName("enroll_channel");
            entity.Property(e => e.EnrollUserId)
                .HasComment("报名提交账号ID")
                .HasColumnName("enroll_user_id");
            entity.Property(e => e.EventId)
                .HasComment("活动ID")
                .HasColumnName("event_id");
            entity.Property(e => e.GroupId)
                .HasComment("组别ID")
                .HasColumnName("group_id");
            entity.Property(e => e.PayOrderId)
                .HasComment("支付订单ID")
                .HasColumnName("pay_order_id");
            entity.Property(e => e.Status)
                .HasComment("状态：0已提交待审核；1审核驳回；2审核通过待缴费；3支付中；4已缴费；5已确认；6已取消；7支付失败；8退款中；9已退款")
                .HasColumnName("status");
        });

        modelBuilder.Entity<EventGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("event_group", tb => tb.HasComment("活动组别/参赛组别"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.EventId, "idx_group_event");

            entity.HasIndex(e => new { e.EventId, e.Code }, "uk_event_group_code").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(16)
                .HasComment("组别编码（A1/A2等）")
                .HasColumnName("code");
            entity.Property(e => e.EventId)
                .HasComment("活动ID")
                .HasColumnName("event_id");
            entity.Property(e => e.FeeAmount)
                .HasComment("报名费（分）")
                .HasColumnName("fee_amount");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasComment("组别名称")
                .HasColumnName("name");
            entity.Property(e => e.Quota)
                .HasComment("名额（空表示不限）")
                .HasColumnName("quota");
            entity.Property(e => e.SortNo)
                .HasComment("排序号")
                .HasColumnName("sort_no");
        });

        modelBuilder.Entity<LocalAssociation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("local_association", tb => tb.HasComment("地方协会（展示）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.ContactName)
                .HasMaxLength(64)
                .HasComment("联系人")
                .HasColumnName("contact_name");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(32)
                .HasComment("联系电话")
                .HasColumnName("contact_phone");
            entity.Property(e => e.ContentHtml)
                .HasComment("详情HTML")
                .HasColumnType("mediumtext")
                .HasColumnName("content_html");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Intro)
                .HasMaxLength(500)
                .HasComment("简介")
                .HasColumnName("intro");
            entity.Property(e => e.LogoUrl)
                .HasComment("LOGO")
                .HasColumnType("mediumtext")
                .HasColumnName("logo_url");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasComment("协会名称")
                .HasColumnName("name");
            entity.Property(e => e.SortNo)
                .HasComment("排序号")
                .HasColumnName("sort_no");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1启用；0停用")
                .HasColumnName("status");
        });

        modelBuilder.Entity<MemberUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("member_unit", tb => tb.HasComment("会员单位（展示）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Name, "idx_member_name");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasComment("地址")
                .HasColumnName("address");
            entity.Property(e => e.ContactName)
                .HasMaxLength(64)
                .HasComment("联系人")
                .HasColumnName("contact_name");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(32)
                .HasComment("联系电话")
                .HasColumnName("contact_phone");
            entity.Property(e => e.ContentHtml)
                .HasComment("详情HTML")
                .HasColumnType("mediumtext")
                .HasColumnName("content_html");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Intro)
                .HasMaxLength(500)
                .HasComment("简介")
                .HasColumnName("intro");
            entity.Property(e => e.LogoUrl)
                .HasComment("LOGO")
                .HasColumnType("mediumtext")
                .HasColumnName("logo_url");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasComment("单位名称")
                .HasColumnName("name");
            entity.Property(e => e.SortNo)
                .HasComment("排序号")
                .HasColumnName("sort_no");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1启用；0停用")
                .HasColumnName("status");
        });

        modelBuilder.Entity<NewsArticle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("news_article", tb => tb.HasComment("新闻资讯"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.Status, e.PublishAt }, "idx_news_status_time");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.ContentHtml)
                .HasComment("正文HTML")
                .HasColumnType("mediumtext")
                .HasColumnName("content_html");
            entity.Property(e => e.ContentType)
                .HasMaxLength(16)
                .HasDefaultValueSql("'text'")
                .HasComment("内容类型：text图文；video视频")
                .HasColumnName("content_type");
            entity.Property(e => e.CoverUrl)
                .HasComment("封面图")
                .HasColumnType("mediumtext")
                .HasColumnName("cover_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PublishAt)
                .HasComment("发布时间")
                .HasColumnType("datetime")
                .HasColumnName("publish_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1已发布；0草稿；2下线")
                .HasColumnName("status");
            entity.Property(e => e.Summary)
                .HasMaxLength(500)
                .HasComment("摘要")
                .HasColumnName("summary");
            entity.Property(e => e.Tags)
                .HasMaxLength(128)
                .HasComment("标签（逗号分隔）")
                .HasColumnName("tags");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasComment("标题")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VideoUrl)
                .HasMaxLength(512)
                .HasComment("视频URL")
                .HasColumnName("video_url");
            entity.Property(e => e.ViewCount)
                .HasComment("浏览量")
                .HasColumnName("view_count");
        });

        modelBuilder.Entity<Org>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("org", tb => tb.HasComment("机构表（培训机构/中小学）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Name, "idx_org_name");

            entity.HasIndex(e => e.OrgType, "idx_org_type");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasComment("详细地址")
                .HasColumnName("address");
            entity.Property(e => e.City)
                .HasMaxLength(32)
                .HasComment("市")
                .HasColumnName("city");
            entity.Property(e => e.ContactName)
                .HasMaxLength(64)
                .HasComment("联系人姓名")
                .HasColumnName("contact_name");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(32)
                .HasComment("联系人电话")
                .HasColumnName("contact_phone");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasComment("删除时间（软删）")
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.District)
                .HasMaxLength(32)
                .HasComment("区县")
                .HasColumnName("district");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasComment("机构名称")
                .HasColumnName("name");
            entity.Property(e => e.OrgType)
                .HasComment("机构类型：1培训机构；2中小学")
                .HasColumnName("org_type");
            entity.Property(e => e.Province)
                .HasMaxLength(32)
                .HasComment("省")
                .HasColumnName("province");
            entity.Property(e => e.ShortName)
                .HasMaxLength(64)
                .HasComment("机构简称")
                .HasColumnName("short_name");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1启用；0停用")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<PayOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("pay_order", tb => tb.HasComment("支付订单表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.BizType, e.BizId }, "idx_pay_biz");

            entity.HasIndex(e => e.WxOutTradeNo, "uk_out_trade_no").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasComment("订单金额（分）")
                .HasColumnName("amount");
            entity.Property(e => e.BizId)
                .HasComment("业务ID（如entryId/certIssueId）")
                .HasColumnName("biz_id");
            entity.Property(e => e.BizType)
                .HasComment("业务类型：1活动报名；2首次发证；3补证")
                .HasColumnName("biz_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PaidAt)
                .HasComment("支付完成时间")
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.Status)
                .HasComment("状态：0待支付；1已支付；2已关闭；3已退款")
                .HasColumnName("status");
            entity.Property(e => e.UserId)
                .HasComment("下单账号ID")
                .HasColumnName("user_id");
            entity.Property(e => e.WxOutTradeNo)
                .HasMaxLength(64)
                .HasComment("微信商户订单号")
                .HasColumnName("wx_out_trade_no");
        });

        modelBuilder.Entity<PayWechatNotify>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("pay_wechat_notify", tb => tb.HasComment("微信支付回调记录（原文落库）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.OutTradeNo, "idx_notify_out_trade_no");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.NotifyRaw)
                .HasComment("回调原文（用于审计与排障）")
                .HasColumnType("mediumtext")
                .HasColumnName("notify_raw");
            entity.Property(e => e.OutTradeNo)
                .HasMaxLength(64)
                .HasComment("商户订单号")
                .HasColumnName("out_trade_no");
            entity.Property(e => e.PayOrderId)
                .HasComment("支付订单ID")
                .HasColumnName("pay_order_id");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(64)
                .HasComment("微信支付单号")
                .HasColumnName("transaction_id");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("person", tb => tb.HasComment("人员主表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Mobile, "idx_person_mobile");

            entity.HasIndex(e => e.FullName, "idx_person_name");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(512)
                .HasComment("头像URL")
                .HasColumnName("avatar_url");
            entity.Property(e => e.Birthday)
                .HasComment("出生日期")
                .HasColumnName("birthday");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasComment("删除时间（软删）")
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.FullName)
                .HasMaxLength(64)
                .HasComment("姓名")
                .HasColumnName("full_name");
            entity.Property(e => e.Gender)
                .HasComment("性别：0未知；1男；2女")
                .HasColumnName("gender");
            entity.Property(e => e.IdCardNo)
                .HasMaxLength(32)
                .HasComment("身份证号（敏感）")
                .HasColumnName("id_card_no");
            entity.Property(e => e.Mobile)
                .HasMaxLength(32)
                .HasComment("手机号")
                .HasColumnName("mobile");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1正常；0禁用")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<PersonAthlete>(entity =>
        {
            entity.HasKey(e => e.PersonId).HasName("PRIMARY");

            entity
                .ToTable("person_athlete", tb => tb.HasComment("运动员角色表（中小学必填；培训机构选填；第一教练员终身绑定）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.FirstCoachPersonId, "idx_athlete_first_coach");

            entity.HasIndex(e => e.SchoolId, "idx_athlete_school");

            entity.HasIndex(e => e.TrainingOrgId, "idx_athlete_training_org");

            entity.Property(e => e.PersonId)
                .ValueGeneratedNever()
                .HasComment("人员ID")
                .HasColumnName("person_id");
            entity.Property(e => e.ClassName)
                .HasMaxLength(64)
                .HasComment("班级（如一班/七年级三班等）")
                .HasColumnName("class_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.FirstCoachPersonId)
                .HasComment("第一教练员人员ID（终身绑定，仅平台可改）")
                .HasColumnName("first_coach_person_id");
            entity.Property(e => e.GradeName)
                .HasMaxLength(32)
                .HasComment("年级（如一年级/七年级等）")
                .HasColumnName("grade_name");
            entity.Property(e => e.SchoolId)
                .HasComment("所属中小学ID（必填，指向school.id）")
                .HasColumnName("school_id");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1有效；0无效")
                .HasColumnName("status");
            entity.Property(e => e.TrainingOrgId)
                .HasComment("所属培训机构ID（选填）")
                .HasColumnName("training_org_id");
        });

        modelBuilder.Entity<PersonCoach>(entity =>
        {
            entity.HasKey(e => e.PersonId).HasName("PRIMARY");

            entity
                .ToTable("person_coach", tb => tb.HasComment("教练员角色表（必须绑定机构）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.OrgId, "idx_coach_org");

            entity.Property(e => e.PersonId)
                .ValueGeneratedNever()
                .HasComment("人员ID")
                .HasColumnName("person_id");
            entity.Property(e => e.CoachLevel)
                .HasMaxLength(32)
                .HasComment("教练等级（可选）")
                .HasColumnName("coach_level");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OrgId)
                .HasComment("所属机构ID（培训机构或中小学）")
                .HasColumnName("org_id");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1有效；0无效")
                .HasColumnName("status");
        });

        modelBuilder.Entity<PersonJudge>(entity =>
        {
            entity.HasKey(e => e.PersonId).HasName("PRIMARY");

            entity
                .ToTable("person_judge", tb => tb.HasComment("裁判员角色表（必须绑定机构）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.OrgId, "idx_judge_org");

            entity.Property(e => e.PersonId)
                .ValueGeneratedNever()
                .HasComment("人员ID")
                .HasColumnName("person_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.JudgeLevel)
                .HasMaxLength(32)
                .HasComment("裁判等级（可选）")
                .HasColumnName("judge_level");
            entity.Property(e => e.OrgId)
                .HasComment("所属机构ID（培训机构或中小学）")
                .HasColumnName("org_id");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1有效；0无效")
                .HasColumnName("status");
        });

        modelBuilder.Entity<PersonParent>(entity =>
        {
            entity.HasKey(e => e.PersonId).HasName("PRIMARY");

            entity
                .ToTable("person_parent", tb => tb.HasComment("家长角色表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.Property(e => e.PersonId)
                .ValueGeneratedNever()
                .HasComment("人员ID")
                .HasColumnName("person_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1有效；0无效")
                .HasColumnName("status");
        });

        modelBuilder.Entity<ProjectCatalog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("project_catalog", tb => tb.HasComment("项目字典（来源：项目设定.xlsx）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Name, "uk_project_name").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasComment("项目编码（选填）")
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasComment("项目名称")
                .HasColumnName("name");
            entity.Property(e => e.ParticipantCount)
                .HasDefaultValueSql("'1'")
                .HasComment("人数（单人/双人/团队）")
                .HasColumnName("participant_count");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1启用；0停用")
                .HasColumnName("status");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("school", tb => tb.HasComment("学校主表（中小学）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Name, "idx_school_name");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasComment("详细地址")
                .HasColumnName("address");
            entity.Property(e => e.City)
                .HasMaxLength(32)
                .HasComment("市")
                .HasColumnName("city");
            entity.Property(e => e.ContactName)
                .HasMaxLength(64)
                .HasComment("联系人姓名")
                .HasColumnName("contact_name");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(32)
                .HasComment("联系人电话")
                .HasColumnName("contact_phone");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.District)
                .HasMaxLength(32)
                .HasComment("区县")
                .HasColumnName("district");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasComment("学校名称")
                .HasColumnName("name");
            entity.Property(e => e.Province)
                .HasMaxLength(32)
                .HasComment("省")
                .HasColumnName("province");
            entity.Property(e => e.ShortName)
                .HasMaxLength(64)
                .HasComment("学校简称")
                .HasColumnName("short_name");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1启用；0停用")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("更新时间")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user_account", tb => tb.HasComment("账号表"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.UserType, "idx_user_type");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'1'")
                .HasComment("状态：1正常；0禁用")
                .HasColumnName("status");
            entity.Property(e => e.UserType)
                .HasComment("账号类型：1小程序；2后台")
                .HasColumnName("user_type");
        });

        modelBuilder.Entity<UserIdentitySubmit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user_identity_submit", tb => tb.HasComment("用户认证提交记录（需后台审核）"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "idx_identity_status_time");

            entity.HasIndex(e => new { e.UserId, e.Status }, "idx_identity_user_status");

            entity.Property(e => e.Id)
                .HasComment("主键")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("提交时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IdCardNo)
                .HasMaxLength(32)
                .HasComment("身份证号（敏感）")
                .HasColumnName("id_card_no");
            entity.Property(e => e.Mobile)
                .HasMaxLength(32)
                .HasComment("手机号")
                .HasColumnName("mobile");
            entity.Property(e => e.RealName)
                .HasMaxLength(64)
                .HasComment("真实姓名")
                .HasColumnName("real_name");
            entity.Property(e => e.RejectReason)
                .HasMaxLength(255)
                .HasComment("驳回原因")
                .HasColumnName("reject_reason");
            entity.Property(e => e.ReviewedAt)
                .HasComment("审核时间")
                .HasColumnType("datetime")
                .HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewedByAdminId)
                .HasComment("审核后台账号ID")
                .HasColumnName("reviewed_by_admin_id");
            entity.Property(e => e.Status)
                .HasComment("状态：0待审核；1通过；2驳回")
                .HasColumnName("status");
            entity.Property(e => e.UserId)
                .HasComment("账号ID")
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<UserWechat>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity
                .ToTable("user_wechat", tb => tb.HasComment("小程序微信账号信息"))
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Openid, "uk_openid").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasComment("账号ID")
                .HasColumnName("user_id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(512)
                .HasComment("微信头像")
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("创建时间")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Nickname)
                .HasMaxLength(64)
                .HasComment("微信昵称")
                .HasColumnName("nickname");
            entity.Property(e => e.Openid)
                .HasMaxLength(64)
                .HasComment("微信openid")
                .HasColumnName("openid");
            entity.Property(e => e.Unionid)
                .HasMaxLength(64)
                .HasComment("微信unionid")
                .HasColumnName("unionid");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
