using System.Text;
using System.Data;
using DotNetEnv;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<MySqlOptions>(builder.Configuration.GetSection("MySql"));

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var mySql = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MySqlOptions>>().Value;
    options.UseMySql(mySql.ConnectionString, ServerVersion.AutoDetect(mySql.ConnectionString));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var hasKey = !string.IsNullOrWhiteSpace(jwt.SigningKey);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = hasKey && !string.IsNullOrWhiteSpace(jwt.Issuer),
            ValidIssuer = jwt.Issuer,
            ValidateAudience = hasKey && !string.IsNullOrWhiteSpace(jwt.Audience),
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = hasKey,
            IssuerSigningKey = hasKey ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey!)) : null,
            ValidateLifetime = hasKey,
            ClockSkew = TimeSpan.FromMinutes(2),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

var uploadRoot = Path.Combine(app.Environment.ContentRootPath, "upload");
Directory.CreateDirectory(uploadRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadRoot),
    RequestPath = "/upload"
});

var urls = builder.Configuration["ASPNETCORE_URLS"] ?? "";
var hasHttpsEndpoint = urls.Contains("https://", StringComparison.OrdinalIgnoreCase);
if (hasHttpsEndpoint)
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    db.Database.OpenConnection();
    try
    {
        var conn = db.Database.GetDbConnection();

        static bool ColumnExists(System.Data.Common.DbConnection c, string tableName, string columnName)
        {
            using var cmd = c.CreateCommand();
            cmd.CommandText = """
SELECT COUNT(*)
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @tableName
  AND COLUMN_NAME = @columnName
""";
            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@tableName";
            p1.Value = tableName;
            cmd.Parameters.Add(p1);

            var p2 = cmd.CreateParameter();
            p2.ParameterName = "@columnName";
            p2.Value = columnName;
            cmd.Parameters.Add(p2);

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        static void AddColumnIfMissing(System.Data.Common.DbConnection c, string tableName, string columnName, string columnDdl)
        {
            if (ColumnExists(c, tableName, columnName)) return;
            using var cmd = c.CreateCommand();
            cmd.CommandText = $"ALTER TABLE `{tableName}` ADD COLUMN {columnDdl};";
            cmd.ExecuteNonQuery();
        }

        static void ModifyColumn(System.Data.Common.DbConnection c, string tableName, string columnDdl)
        {
            using var cmd = c.CreateCommand();
            cmd.CommandText = $"ALTER TABLE `{tableName}` MODIFY COLUMN {columnDdl};";
            cmd.ExecuteNonQuery();
        }

        static void Exec(System.Data.Common.DbConnection c, string sql)
        {
            using var cmd = c.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        Exec(conn, """
CREATE TABLE IF NOT EXISTS `system_setting` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `key` VARCHAR(64) NOT NULL,
  `value_json` MEDIUMTEXT NOT NULL,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_system_setting_key` (`key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
""");

        AddColumnIfMissing(conn, "about_page", "name", "`name` VARCHAR(128) NULL COMMENT '协会名称'");
        AddColumnIfMissing(conn, "about_page", "address", "`address` VARCHAR(255) NULL COMMENT '协会地址'");
        AddColumnIfMissing(conn, "about_page", "logo_url", "`logo_url` MEDIUMTEXT NULL COMMENT '协会Logo（URL或base64 data url）'");
        AddColumnIfMissing(conn, "about_page", "overview_html", "`overview_html` MEDIUMTEXT NULL COMMENT '协会概述HTML'");
        AddColumnIfMissing(conn, "about_page", "history_html", "`history_html` MEDIUMTEXT NULL COMMENT '协会历史HTML'");
        AddColumnIfMissing(conn, "about_page", "honors_html", "`honors_html` MEDIUMTEXT NULL COMMENT '协会荣誉HTML'");

        AddColumnIfMissing(conn, "event", "logo_url", "`logo_url` MEDIUMTEXT NULL COMMENT '活动Logo'");
        AddColumnIfMissing(conn, "event", "banner_url", "`banner_url` MEDIUMTEXT NULL COMMENT '活动Banner'");
        AddColumnIfMissing(conn, "event", "slogan", "`slogan` VARCHAR(255) NULL COMMENT '活动标语'");
        AddColumnIfMissing(conn, "event", "host_org", "`host_org` VARCHAR(128) NULL COMMENT '主办单位'");
        AddColumnIfMissing(conn, "event", "co_orgs", "`co_orgs` JSON NULL COMMENT '协办单位'");
        AddColumnIfMissing(conn, "event", "contacts", "`contacts` JSON NULL COMMENT '联系人信息'");
        AddColumnIfMissing(conn, "event", "projects", "`projects` JSON NULL COMMENT '比赛项目'");
        AddColumnIfMissing(conn, "event", "event_start_at", "`event_start_at` DATETIME NULL COMMENT '活动开始时间'");
        AddColumnIfMissing(conn, "event", "event_end_at", "`event_end_at` DATETIME NULL COMMENT '活动结束时间'");

        AddColumnIfMissing(conn, "banner", "media_type", "`media_type` VARCHAR(32) NULL DEFAULT 'image' COMMENT '类型：image/video'");
        AddColumnIfMissing(conn, "banner", "video_url", "`video_url` MEDIUMTEXT NULL COMMENT '视频URL'");
        AddColumnIfMissing(conn, "banner", "image_url", "`image_url` MEDIUMTEXT NULL COMMENT '图片URL'");

        AddColumnIfMissing(conn, "admin_role", "permissions_json", "`permissions_json` MEDIUMTEXT NULL COMMENT '权限点JSON数组'");

        AddColumnIfMissing(conn, "local_association", "logo_url", "`logo_url` MEDIUMTEXT NULL COMMENT 'LOGO'");
        AddColumnIfMissing(conn, "member_unit", "logo_url", "`logo_url` MEDIUMTEXT NULL COMMENT 'LOGO'");
        AddColumnIfMissing(conn, "news_article", "cover_url", "`cover_url` MEDIUMTEXT NULL COMMENT '封面图'");
        AddColumnIfMissing(conn, "event", "cover_url", "`cover_url` MEDIUMTEXT NULL COMMENT '封面图URL'");

        try
        {
            ModifyColumn(conn, "event", "`logo_url` MEDIUMTEXT NULL COMMENT '活动Logo'");
            ModifyColumn(conn, "event", "`banner_url` MEDIUMTEXT NULL COMMENT '活动Banner'");
            ModifyColumn(conn, "event", "`cover_url` MEDIUMTEXT NULL COMMENT '封面图URL'");
            ModifyColumn(conn, "banner", "`image_url` MEDIUMTEXT NULL COMMENT '图片URL'");
            ModifyColumn(conn, "banner", "`video_url` MEDIUMTEXT NULL COMMENT '视频URL'");
            ModifyColumn(conn, "local_association", "`logo_url` MEDIUMTEXT NULL COMMENT 'LOGO'");
            ModifyColumn(conn, "member_unit", "`logo_url` MEDIUMTEXT NULL COMMENT 'LOGO'");
            ModifyColumn(conn, "news_article", "`cover_url` MEDIUMTEXT NULL COMMENT '封面图'");

            Exec(conn, "UPDATE `banner` SET `image_url` = NULL WHERE `image_url` LIKE 'data:image/%;base64,AAAA%';");
            Exec(conn, "UPDATE `banner` SET `video_url` = NULL WHERE `video_url` LIKE 'data:video/%;base64,AAAA%';");
            Exec(conn, "UPDATE `local_association` SET `logo_url` = NULL WHERE `logo_url` LIKE 'data:image/%;base64,AAAA%';");
            Exec(conn, "UPDATE `member_unit` SET `logo_url` = NULL WHERE `logo_url` LIKE 'data:image/%;base64,AAAA%';");
            Exec(conn, "UPDATE `news_article` SET `cover_url` = NULL WHERE `cover_url` LIKE 'data:image/%;base64,AAAA%';");
            Exec(conn, "UPDATE `event` SET `logo_url` = NULL WHERE `logo_url` LIKE 'data:image/%;base64,AAAA%';");
            Exec(conn, "UPDATE `event` SET `banner_url` = NULL WHERE `banner_url` LIKE 'data:image/%;base64,AAAA%';");
            Exec(conn, "UPDATE `event` SET `cover_url` = NULL WHERE `cover_url` LIKE 'data:image/%;base64,AAAA%';");
            Exec(conn, "UPDATE `about_page` SET `logo_url` = NULL WHERE `logo_url` LIKE 'data:image/%;base64,AAAA%';");
        }
        catch
        {
        }
    }
    finally
    {
        db.Database.CloseConnection();
    }

    if (app.Environment.IsDevelopment())
    {
        var hasAdminRole = db.AdminRoles.AsNoTracking().Any(x => x.Code == "admin");
        if (!hasAdminRole)
        {
            db.AdminRoles.Add(new AdminRole { Code = "admin", Name = "系统管理员", CreatedAt = DateTime.UtcNow });
            db.SaveChanges();
        }

        var adminRoleId = db.AdminRoles.AsNoTracking().Where(x => x.Code == "admin").Select(x => x.Id).FirstOrDefault();
        var hasAdminUser = db.AdminUsers.AsNoTracking().Any(x => x.Username == "admin");
        if (!hasAdminUser)
        {
            var u = new AdminUser { Username = "admin", PasswordHash = AdminPasswordHasher.Hash("123456"), Status = 1, CreatedAt = DateTime.UtcNow };
            db.AdminUsers.Add(u);
            db.SaveChanges();
            if (adminRoleId != 0)
            {
                db.AdminUserRoles.Add(new AdminUserRole { AdminUserId = u.Id, AdminRoleId = adminRoleId, CreatedAt = DateTime.UtcNow });
                db.SaveChanges();
            }
        }
    }
    if (!db.Events.Any())
    if (!db.Events.Any())
    {
        db.Events.AddRange(
            new Event
            {
                EventType = 1,
                SignupScope = 1,
                Title = "春季校园跳绳公开赛（长沙赛区）",
                CoverUrl = "/banner-1.svg",
                SignupStartAt = DateTime.UtcNow,
                SignupEndAt = DateTime.UtcNow.AddDays(10),
                EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
                Location = "长沙市体育馆",
                DescriptionHtml = "<p>团体与个人项目齐开，支持线上报名与审核缴费。</p>",
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new Event
            {
                EventType = 1,
                SignupScope = 1,
                Title = "春季团体速度赛 · 省赛",
                CoverUrl = "/banner-2.svg",
                SignupStartAt = DateTime.UtcNow.AddDays(5),
                SignupEndAt = DateTime.UtcNow.AddDays(15),
                EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(25)),
                Location = "湖南省体育中心",
                DescriptionHtml = "<p>项目丰富、规则透明，电子证书赛后发放。</p>",
                Status = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }
        );
        db.SaveChanges();
    }

    if (!db.EventGroups.Any())
    {
        var firstEventId = db.Events.OrderBy(x => x.Id).Select(x => x.Id).FirstOrDefault();
        if (firstEventId != 0)
        {
            db.EventGroups.AddRange(
                new EventGroup { EventId = firstEventId, Code = "A1", Name = "少年组", FeeAmount = 19900, Quota = 200, SortNo = 1 },
                new EventGroup { EventId = firstEventId, Code = "A2", Name = "青年组", FeeAmount = 19900, Quota = 200, SortNo = 2 },
                new EventGroup { EventId = firstEventId, Code = "B1", Name = "公开组", FeeAmount = 19900, Quota = 200, SortNo = 3 }
            );
            db.SaveChanges();
        }
    }

    if (!db.People.Any())
    {
        db.People.AddRange(
            new Person { FullName = "测试运动员001", Mobile = "13900001001", Gender = 1, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Person { FullName = "测试运动员002", Mobile = "13900001002", Gender = 2, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Person { FullName = "测试运动员003", Mobile = "13900001003", Gender = 1, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Person { FullName = "测试运动员004", Mobile = "13900001004", Gender = 2, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Person { FullName = "测试运动员005", Mobile = "13900001005", Gender = 1, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        db.SaveChanges();
    }

    if (!db.EventEntries.Any())
    {
        var evId = db.Events.OrderBy(x => x.Id).Select(x => x.Id).FirstOrDefault();
        var gIds = db.EventGroups.Where(x => x.EventId == evId).OrderBy(x => x.SortNo).Select(x => x.Id).ToList();
        var pIds = db.People.OrderBy(x => x.Id).Select(x => x.Id).ToList();

        if (evId != 0 && gIds.Count > 0 && pIds.Count > 0)
        {
            var now = DateTime.UtcNow;
            var list = new List<EventEntry>();
            for (var i = 0; i < Math.Min(30, pIds.Count * 3); i++)
            {
                var status = (sbyte)(i % 5 == 0 ? 0 : i % 5 == 1 ? 1 : i % 5 == 2 ? 2 : 4);
                ulong? payOrderId = status >= 3 ? 880000UL + (ulong)i : null;
                list.Add(new EventEntry
                {
                    EventId = evId,
                    GroupId = gIds[i % gIds.Count],
                    AthletePersonId = pIds[i % pIds.Count],
                    EnrollChannel = (sbyte)((i % 3) + 1),
                    EnrollUserId = 990000UL + (ulong)i,
                    Status = status,
                    AuditRemark = status == 1 ? "资料不完整，请补充信息" : null,
                    PayOrderId = payOrderId,
                    CreatedAt = now.AddMinutes(-5 * i),
                });
            }
            db.EventEntries.AddRange(list);
            db.SaveChanges();
        }
    }
}

app.Run();

