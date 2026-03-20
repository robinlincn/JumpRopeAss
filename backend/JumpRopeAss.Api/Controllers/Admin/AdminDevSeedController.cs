using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/dev/seed")]
[Authorize]
public sealed class AdminDevSeedController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminDevSeedController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public sealed record SeedEntriesRequest(ulong? EventId = null, int AthleteCount = 12, int EntryCount = 30);
    public sealed record SeedIdentityRequest(int Count = 20);
    public sealed record SeedPayOrdersRequest(int Count = 30);
    public sealed record SeedCertificatesRequest(int Count = 20);
    public sealed record SeedAdminUsersRolesRequest(int UserCount = 3);
    public sealed record SeedNewsRequest(int Count = 15);

    private bool EnsureDev() => _env.IsDevelopment();

    [HttpPost("entries")]
    public async Task<ApiResponse<object>> SeedEntries([FromBody] SeedEntriesRequest req)
    {
        if (!EnsureDev())
        {
            return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "仅开发环境可用");
        }

        var athleteCount = Math.Clamp(req.AthleteCount, 1, 50);
        var entryCount = Math.Clamp(req.EntryCount, 1, 200);

        var ev = req.EventId is not null
            ? await _db.Events.FirstOrDefaultAsync(x => x.Id == req.EventId.Value)
            : await _db.Events.OrderByDescending(x => x.Id).FirstOrDefaultAsync();

        if (ev is null)
        {
            ev = new Event
            {
                EventType = 1,
                SignupScope = 1,
                Title = "【测试】报名审核演示活动",
                NeedAudit = 1,
                NeedPay = 1,
                SignupStartAt = DateTime.UtcNow.AddDays(-7),
                SignupEndAt = DateTime.UtcNow.AddDays(7),
                EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
                Location = "测试场馆",
                DescriptionHtml = "<p>用于报名审核/缴费流程的测试数据</p>",
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
        }

        var codes = new[] { "A1", "A2", "B1" };
        var existingGroups = await _db.EventGroups
            .Where(x => x.EventId == ev.Id && codes.Contains(x.Code))
            .ToListAsync();

        var createdGroupCount = 0;
        foreach (var code in codes)
        {
            if (existingGroups.Any(x => x.Code == code)) continue;
            _db.EventGroups.Add(new EventGroup
            {
                EventId = ev.Id,
                Code = code,
                Name = code == "A1" ? "少年组" : code == "A2" ? "青年组" : "公开组",
                FeeAmount = 19900,
                Quota = 200,
                SortNo = createdGroupCount + 1,
            });
            createdGroupCount++;
        }
        if (createdGroupCount > 0) await _db.SaveChangesAsync();

        var groups = await _db.EventGroups
            .Where(x => x.EventId == ev.Id && codes.Contains(x.Code))
            .OrderBy(x => x.SortNo)
            .ToListAsync();

        var mobiles = Enumerable.Range(0, athleteCount).Select(i => $"1390000{(1000 + i):D4}").ToArray();
        var existedPeople = await _db.People
            .Where(x => x.Mobile != null && mobiles.Contains(x.Mobile))
            .ToListAsync();

        var createdPeopleCount = 0;
        foreach (var m in mobiles)
        {
            if (existedPeople.Any(x => x.Mobile == m)) continue;
            _db.People.Add(new Person
            {
                FullName = $"测试运动员{m[^4..]}",
                Mobile = m,
                Gender = 1,
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
            createdPeopleCount++;
        }
        if (createdPeopleCount > 0) await _db.SaveChangesAsync();

        var people = await _db.People
            .Where(x => x.Mobile != null && mobiles.Contains(x.Mobile))
            .OrderBy(x => x.Id)
            .ToListAsync();

        const ulong enrollUserIdBase = 990000;
        var existingEnrollUserIds = await _db.EventEntries
            .Where(x => x.EventId == ev.Id && x.EnrollUserId >= enrollUserIdBase && x.EnrollUserId < enrollUserIdBase + (ulong)entryCount)
            .Select(x => x.EnrollUserId)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var createdEntryCount = 0;
        for (var i = 0; i < entryCount; i++)
        {
            var enrollUserId = enrollUserIdBase + (ulong)i;
            if (existingEnrollUserIds.Contains(enrollUserId)) continue;

            var person = people[i % people.Count];
            var group = groups[i % groups.Count];
            var status = (sbyte)(i % 5 == 0 ? 0 : i % 5 == 1 ? 1 : i % 5 == 2 ? 2 : 4);
            var auditRemark = status == 1 ? "资料不完整，请补充信息" : null;
            ulong? payOrderId = status >= 3 ? 880000UL + (ulong)i : null;

            _db.EventEntries.Add(new EventEntry
            {
                EventId = ev.Id,
                GroupId = group.Id,
                AthletePersonId = person.Id,
                EnrollChannel = (sbyte)((i % 3) + 1),
                EnrollUserId = enrollUserId,
                Status = status,
                AuditRemark = auditRemark,
                PayOrderId = payOrderId,
                CreatedAt = now.AddMinutes(-5 * i),
            });
            createdEntryCount++;
        }
        if (createdEntryCount > 0) await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new
        {
            eventId = ev.Id,
            createdGroups = createdGroupCount,
            createdPeople = createdPeopleCount,
            createdEntries = createdEntryCount,
        });
    }

    [HttpPost("admin-users-roles")]
    public async Task<ApiResponse<object>> SeedAdminUsersRoles([FromBody] SeedAdminUsersRolesRequest req)
    {
        if (!EnsureDev()) return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "仅开发环境可用");

        var roles = new[]
        {
            new { Code = "admin", Name = "系统管理员" },
            new { Code = "finance", Name = "财务" },
            new { Code = "cert", Name = "证书管理员" },
        };

        var existedRoleCodes = await _db.AdminRoles.AsNoTracking().Select(x => x.Code).ToListAsync();
        var createdRoleCount = 0;
        foreach (var r in roles)
        {
            if (existedRoleCodes.Contains(r.Code)) continue;
            _db.AdminRoles.Add(new AdminRole { Code = r.Code, Name = r.Name, CreatedAt = DateTime.UtcNow });
            createdRoleCount++;
        }
        if (createdRoleCount > 0) await _db.SaveChangesAsync();

        var allRoles = await _db.AdminRoles.AsNoTracking().ToListAsync();
        var usernames = Enumerable.Range(1, Math.Clamp(req.UserCount, 1, 10)).Select(i => $"dev_user_{i}").ToArray();
        var existedUsers = await _db.AdminUsers.AsNoTracking().Where(x => usernames.Contains(x.Username)).ToListAsync();

        var createdUserCount = 0;
        foreach (var u in usernames)
        {
            if (existedUsers.Any(x => x.Username == u)) continue;
            _db.AdminUsers.Add(new AdminUser { Username = u, PasswordHash = AdminPasswordHasher.Hash("123456"), Status = 1, CreatedAt = DateTime.UtcNow });
            createdUserCount++;
        }
        if (createdUserCount > 0) await _db.SaveChangesAsync();

        var users = await _db.AdminUsers.AsNoTracking().Where(x => usernames.Contains(x.Username)).OrderBy(x => x.Id).ToListAsync();
        var existingLinks = await _db.AdminUserRoles.AsNoTracking()
            .Where(x => users.Select(u => u.Id).Contains(x.AdminUserId))
            .Select(x => new { x.AdminUserId, x.AdminRoleId })
            .ToListAsync();

        var createdLinkCount = 0;
        for (var i = 0; i < users.Count; i++)
        {
            var user = users[i];
            var role = allRoles[i % allRoles.Count];
            if (existingLinks.Any(x => x.AdminUserId == user.Id && x.AdminRoleId == role.Id)) continue;
            _db.AdminUserRoles.Add(new AdminUserRole { AdminUserId = user.Id, AdminRoleId = role.Id, CreatedAt = DateTime.UtcNow });
            createdLinkCount++;
        }
        if (createdLinkCount > 0) await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { createdRoles = createdRoleCount, createdUsers = createdUserCount, createdUserRoles = createdLinkCount });
    }

    [HttpPost("identity-submits")]
    public async Task<ApiResponse<object>> SeedIdentitySubmits([FromBody] SeedIdentityRequest req)
    {
        if (!EnsureDev()) return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "仅开发环境可用");

        var count = Math.Clamp(req.Count, 1, 200);
        var mobiles = Enumerable.Range(0, count).Select(i => $"1380000{(2000 + i):D4}").ToArray();
        var existedMobiles = await _db.UserIdentitySubmits.AsNoTracking()
            .Where(x => mobiles.Contains(x.Mobile))
            .Select(x => x.Mobile)
            .ToListAsync();

        var created = 0;
        var now = DateTime.UtcNow;
        for (var i = 0; i < count; i++)
        {
            var mobile = mobiles[i];
            if (existedMobiles.Contains(mobile)) continue;
            var status = (sbyte)(i % 5 == 0 ? 0 : i % 5 == 1 ? 2 : 1);
            _db.UserIdentitySubmits.Add(new UserIdentitySubmit
            {
                UserId = 700000UL + (ulong)i,
                RealName = $"测试用户{(i + 1):D3}",
                IdCardNo = $"4301{(20000101 + i):D8}{(1000 + i):D4}",
                Mobile = mobile,
                Status = status,
                RejectReason = status == 2 ? "身份证号与姓名不一致" : null,
                ReviewedAt = status == 0 ? null : now.AddHours(-i),
                ReviewedByAdminId = status == 0 ? null : 1,
                CreatedAt = now.AddMinutes(-10 * i),
            });
            created++;
        }
        if (created > 0) await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { created });
    }

    [HttpPost("pay-orders")]
    public async Task<ApiResponse<object>> SeedPayOrders([FromBody] SeedPayOrdersRequest req)
    {
        if (!EnsureDev()) return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "仅开发环境可用");

        var count = Math.Clamp(req.Count, 1, 200);
        var baseNo = $"DEV{DateTime.UtcNow:yyyyMMdd}";
        var existedNos = await _db.PayOrders.AsNoTracking()
            .Where(x => x.WxOutTradeNo != null && EF.Functions.Like(x.WxOutTradeNo, $"{baseNo}%"))
            .Select(x => x.WxOutTradeNo!)
            .ToListAsync();

        var created = 0;
        var now = DateTime.UtcNow;
        for (var i = 0; i < count; i++)
        {
            var outTradeNo = $"{baseNo}{i:D6}";
            if (existedNos.Contains(outTradeNo)) continue;
            var status = (sbyte)(i % 4);
            _db.PayOrders.Add(new PayOrder
            {
                BizType = (sbyte)((i % 3) + 1),
                BizId = 500000UL + (ulong)i,
                UserId = 700000UL + (ulong)(i % 50),
                Amount = 9900 + (i % 5) * 1000,
                Status = status,
                WxOutTradeNo = outTradeNo,
                PaidAt = status == 1 ? now.AddMinutes(-3 * i) : null,
                CreatedAt = now.AddMinutes(-5 * i),
            });
            created++;
        }
        if (created > 0) await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { created });
    }

    [HttpPost("certificates")]
    public async Task<ApiResponse<object>> SeedCertificates([FromBody] SeedCertificatesRequest req)
    {
        if (!EnsureDev()) return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "仅开发环境可用");

        var count = Math.Clamp(req.Count, 1, 200);

        var needTypes = new[]
        {
            new { Code = "athlete_level", Name = "运动员等级证书" },
            new { Code = "coach_cert", Name = "教练员证书" },
        };

        var existedTypeCodes = await _db.CertTypes.AsNoTracking().Select(x => x.Code).ToListAsync();
        var createdTypes = 0;
        foreach (var t in needTypes)
        {
            if (existedTypeCodes.Contains(t.Code)) continue;
            _db.CertTypes.Add(new CertType { Code = t.Code, Name = t.Name, Status = 1 });
            createdTypes++;
        }
        if (createdTypes > 0) await _db.SaveChangesAsync();

        if (!await _db.People.AsNoTracking().AnyAsync())
        {
            _db.People.AddRange(
                new Person { FullName = "测试持证人001", Mobile = "13900002001", Gender = 1, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Person { FullName = "测试持证人002", Mobile = "13900002002", Gender = 2, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            await _db.SaveChangesAsync();
        }

        var types = await _db.CertTypes.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
        var people = await _db.People.AsNoTracking().OrderBy(x => x.Id).Take(100).ToListAsync();
        var certNoPrefix = $"CERT{DateTime.UtcNow:yyyyMMdd}";
        var existedNos = await _db.Certificates.AsNoTracking()
            .Where(x => EF.Functions.Like(x.CertNo, $"{certNoPrefix}%"))
            .Select(x => x.CertNo)
            .ToListAsync();

        var created = 0;
        var now = DateTime.UtcNow;
        for (var i = 0; i < count; i++)
        {
            var certNo = $"{certNoPrefix}{i:D6}";
            if (existedNos.Contains(certNo)) continue;
            var holder = people[i % people.Count];
            var type = types[i % types.Count];
            _db.Certificates.Add(new Certificate
            {
                CertNo = certNo,
                CertTypeId = type.Id,
                HolderPersonId = holder.Id,
                IssueScene = (sbyte)((i % 2) + 1),
                IssueAt = now.AddDays(-i),
                Status = (sbyte)(i % 10 == 0 ? 2 : 1),
                FileUrl = null,
                CreatedAt = now.AddDays(-i),
            });
            created++;
        }
        if (created > 0) await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { createdTypes, created });
    }

    [HttpPost("news")]
    public async Task<ApiResponse<object>> SeedNews([FromBody] SeedNewsRequest req)
    {
        if (!EnsureDev()) return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "仅开发环境可用");

        var templates = new List<(string Title, string CoverUrl, string Tags, string Summary, string ContentHtml)>
        {
            (
                "全民健身｜把“日常走路”走出训练效果：强度、频率与误区",
                "https://images.unsplash.com/photo-1549576490-b0b4831ef60a?auto=format&fit=crop&w=1200&q=80",
                "全民健身,走路,健康",
                "同样是走路，强度与节奏不同，心肺收益差别很大。用最简单的方法把日常通勤变成可持续的训练。",
                "<p>走路是最容易坚持的运动之一，但想获得更明显的心肺收益，关键在于<strong>强度</strong>与<strong>频率</strong>。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1549576490-b0b4831ef60a?auto=format&fit=crop&w=1400&q=80\" alt=\"walking\" /></p>" +
                "<h3>怎么判断强度？</h3><ul><li>能说话但不太能唱歌：通常是适中强度</li><li>每周累计 150 分钟以上更稳妥</li></ul>" +
                "<p>参考：<a href=\"https://www.who.int/news-room/fact-sheets/detail/physical-activity\" target=\"_blank\" rel=\"noreferrer\">WHO Physical activity</a></p>"
            ),
            (
                "体育科普｜跳绳入门：新手 4 周循序计划（含热身与放松）",
                "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?auto=format&fit=crop&w=1200&q=80",
                "跳绳,心肺,训练计划",
                "跳绳很“高效”，但也更需要循序渐进。给新手一套容易坚持、又能降低伤害风险的 4 周计划。",
                "<p>跳绳可以在较短时间内提升心肺与协调性，但新手不要一上来就追求连续长时间。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1517836357463-d25dfeac3438?auto=format&fit=crop&w=1400&q=80\" alt=\"jump rope\" /></p>" +
                "<h3>4 周计划（示例）</h3><ul><li>第 1 周：每次 8-10 分钟，间歇为主</li><li>第 2 周：总时长 +20%</li><li>第 3 周：加入节奏变化</li><li>第 4 周：尝试连续段 + 技术动作</li></ul>" +
                "<p>要点：鞋底缓震、地面选择、防止小腿过度紧张。</p>"
            ),
            (
                "科学运动｜跑步不伤膝：从配速到力量训练的 5 个关键",
                "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=1200&q=80",
                "跑步,伤病预防,力量训练",
                "想跑得久，别只盯着配速。跑姿、力量、恢复三件事做好，膝盖更轻松。",
                "<p>“伤膝”往往不是跑步本身，而是<strong>训练结构</strong>与<strong>恢复</strong>出了问题。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=1400&q=80\" alt=\"running\" /></p>" +
                "<h3>5 个关键</h3><ul><li>每周里程递增别太猛</li><li>加入臀中肌、股四头肌力量训练</li><li>避免长期单一配速</li><li>跑后拉伸+补水</li><li>疼痛持续及时减量或就医</li></ul>"
            ),
            (
                "营养补给｜运动前后怎么吃？一张“碳水+蛋白”速查表",
                "https://images.unsplash.com/photo-1490645935967-10de6ba17061?auto=format&fit=crop&w=1200&q=80",
                "营养,补给,训练恢复",
                "训练前吃对让你更有力，训练后吃对让你恢复更快。用简单搭配解决“吃什么”。",
                "<p>不需要复杂计算，记住两个原则：训练前补能量，训练后补恢复。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1490645935967-10de6ba17061?auto=format&fit=crop&w=1400&q=80\" alt=\"nutrition\" /></p>" +
                "<h3>速查表</h3><ul><li>训练前：香蕉/面包 + 酸奶</li><li>训练后：米饭/面条 + 鸡蛋/瘦肉</li></ul>"
            ),
            (
                "居家训练｜10 分钟核心力量：不靠器械也能练出稳定",
                "https://images.unsplash.com/photo-1517832207067-4db24a2ae47c?auto=format&fit=crop&w=1200&q=80",
                "居家训练,核心,力量",
                "核心不是“练腹肌”，而是让你更稳定、更不容易受伤。10 分钟也能有效。",
                "<p>核心训练的目标是提高稳定与控制，而不是追求动作数量。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1517832207067-4db24a2ae47c?auto=format&fit=crop&w=1400&q=80\" alt=\"core\" /></p>" +
                "<h3>动作建议</h3><ul><li>平板支撑：30-45 秒</li><li>死虫：每侧 8-10 次</li><li>臀桥：12-15 次</li></ul>"
            ),
            (
                "青少年体能｜课后 20 分钟：提高速度与灵敏的小游戏",
                "https://images.unsplash.com/photo-1521412644187-c49fa049e84d?auto=format&fit=crop&w=1200&q=80",
                "青少年,体能,敏捷",
                "课后短时高质量活动，比“拼时长”更重要。用游戏方式练速度、灵敏与协调。",
                "<p>青少年体能训练更适合“短、快、多样”。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1521412644187-c49fa049e84d?auto=format&fit=crop&w=1400&q=80\" alt=\"kids\" /></p>" +
                "<h3>推荐玩法</h3><ul><li>折返跑接力</li><li>步点梯训练</li><li>反应追逐游戏</li></ul>"
            ),
            (
                "体育安全｜运动前 5 分钟热身：把受伤风险降下来",
                "https://images.unsplash.com/photo-1517649763962-0c623066013b?auto=format&fit=crop&w=1200&q=80",
                "热身,安全,训练",
                "热身不是“可有可无”。用 5 分钟激活关节与心率，让训练更安全更顺。",
                "<p>热身的目的：提高体温、激活肌群、让心率逐步上来。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1517649763962-0c623066013b?auto=format&fit=crop&w=1400&q=80\" alt=\"warm up\" /></p>" +
                "<h3>5 分钟模板</h3><ul><li>原地小跑 60 秒</li><li>髋/踝关节动态活动 90 秒</li><li>动态拉伸 90 秒</li><li>专项动作 60 秒</li></ul>"
            ),
            (
                "久坐提醒｜每天“动起来”的 3 个小策略（办公室/学习场景）",
                "https://images.unsplash.com/photo-1521737604893-d14cc237f11d?auto=format&fit=crop&w=1200&q=80",
                "久坐,健康管理,日常运动",
                "久坐带来的代谢压力不容忽视。用可执行的小策略把运动融入一天。",
                "<p>久坐与多种健康风险相关，关键是把“中断久坐”变成习惯。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1521737604893-d14cc237f11d?auto=format&fit=crop&w=1400&q=80\" alt=\"office\" /></p>" +
                "<h3>3 个策略</h3><ul><li>每 45-60 分钟站起来 2-3 分钟</li><li>午休走 10 分钟</li><li>楼梯替代电梯</li></ul>"
            ),
            (
                "补水指南｜运动时怎么喝水？看懂“口渴”这件事",
                "https://images.unsplash.com/photo-1526401485004-2fda9f4e3b7b?auto=format&fit=crop&w=1200&q=80",
                "补水,运动表现,健康",
                "不是越喝越好，也不是等渴了才喝。掌握“时机+量”让训练更舒服。",
                "<p>长时间运动或高温环境下更要注意补水与电解质。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1526401485004-2fda9f4e3b7b?auto=format&fit=crop&w=1400&q=80\" alt=\"water\" /></p>" +
                "<h3>简单建议</h3><ul><li>运动前 1-2 小时适量喝水</li><li>运动中小口多次</li><li>大量出汗可补电解质</li></ul>"
            ),
            (
                "睡眠与运动｜训练越努力，越要睡得好：恢复的 4 个要点",
                "https://images.unsplash.com/photo-1455642305367-68834a8e0c30?auto=format&fit=crop&w=1200&q=80",
                "睡眠,恢复,训练计划",
                "睡眠质量决定了训练收益的“兑现速度”。用可执行的方法提升恢复。",
                "<p>训练后肌肉修复、神经系统恢复都离不开睡眠。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1455642305367-68834a8e0c30?auto=format&fit=crop&w=1400&q=80\" alt=\"sleep\" /></p>" +
                "<h3>4 个要点</h3><ul><li>固定作息</li><li>睡前减少强刺激</li><li>训练别太晚</li><li>白天适度晒太阳</li></ul>"
            ),
            (
                "大众体育｜从“打卡”到“习惯”：让运动坚持下去的 6 个小技巧",
                "https://images.unsplash.com/photo-1518611012118-f0c5b62b8ad7?auto=format&fit=crop&w=1200&q=80",
                "习惯养成,全民健身,计划",
                "坚持不是靠意志力硬扛，而是把运动做成低门槛、高反馈的日常。",
                "<p>把目标拆小，把触发条件做强，把反馈做快。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1518611012118-f0c5b62b8ad7?auto=format&fit=crop&w=1400&q=80\" alt=\"habit\" /></p>" +
                "<h3>6 个技巧</h3><ul><li>固定时间固定地点</li><li>准备好装备</li><li>从 10 分钟开始</li><li>记录与复盘</li><li>找同伴</li><li>允许“低配完成”</li></ul>"
            ),
            (
                "力量训练｜新手“全身训练”模板：一周 2-3 次就够用",
                "https://images.unsplash.com/photo-1517963879433-6ad2b056d712?auto=format&fit=crop&w=1200&q=80",
                "力量训练,新手,全身训练",
                "力量训练并不等于健美。用全身训练提升基础力量与体态稳定。",
                "<p>建议从基础动作开始：深蹲、推、拉、髋铰链、核心。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1517963879433-6ad2b056d712?auto=format&fit=crop&w=1400&q=80\" alt=\"strength\" /></p>" +
                "<h3>模板示例</h3><ul><li>深蹲 3×8-10</li><li>俯卧撑 3×尽量多</li><li>弹力带划船 3×10-12</li><li>臀桥 3×12</li></ul>"
            ),
            (
                "体测科普｜心率区间怎么用？把“累”变成可量化的训练",
                "https://images.unsplash.com/photo-1519861531473-9200262188bf?auto=format&fit=crop&w=1200&q=80",
                "心率,体测,训练方法",
                "同样的训练时间，心率区间不同，训练效果可能完全不同。用简单规则上手心率训练。",
                "<p>心率区间是把训练强度量化的一种方式，新手只要先掌握“轻松区/适中区”。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1519861531473-9200262188bf?auto=format&fit=crop&w=1400&q=80\" alt=\"heart rate\" /></p>" +
                "<h3>简单规则</h3><ul><li>轻松区：能聊天</li><li>适中区：能说短句</li><li>高强度：只能说单词</li></ul>"
            ),
            (
                "运动心理｜紧张、焦虑怎么办？赛前 3 分钟呼吸练习",
                "https://images.unsplash.com/photo-1506126613408-eca07ce68773?auto=format&fit=crop&w=1200&q=80",
                "心理调节,呼吸,赛前准备",
                "赛前紧张很常见，学会用呼吸把注意力拉回当下，让身体更稳定。",
                "<p>呼吸练习可以帮助降低过度唤醒，让心率逐步稳定。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1506126613408-eca07ce68773?auto=format&fit=crop&w=1400&q=80\" alt=\"breathing\" /></p>" +
                "<h3>3 分钟做法</h3><ul><li>吸气 4 秒</li><li>停 2 秒</li><li>呼气 6 秒</li><li>循环 8-10 次</li></ul>"
            ),
            (
                "体态训练｜肩颈不适？每天 8 分钟改善久坐圆肩",
                "https://images.unsplash.com/photo-1540206395-68808572332f?auto=format&fit=crop&w=1200&q=80",
                "体态,肩颈,拉伸",
                "肩颈不适很多来自久坐与圆肩。用短时训练改善胸椎活动与肩胛控制。",
                "<p>先放松紧张肌群，再激活薄弱肌群，最后用动作整合。</p>" +
                "<p><img src=\"https://images.unsplash.com/photo-1540206395-68808572332f?auto=format&fit=crop&w=1400&q=80\" alt=\"posture\" /></p>" +
                "<h3>动作建议</h3><ul><li>胸小肌拉伸</li><li>墙天使</li><li>弹力带外旋</li></ul>"
            )
        };

        var count = Math.Clamp(req.Count, 1, templates.Count);
        var existedTitles = await _db.NewsArticles.AsNoTracking()
            .Where(x => templates.Select(t => t.Title).Contains(x.Title))
            .Select(x => x.Title)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var created = 0;
        for (var i = 0; i < count; i++)
        {
            var t = templates[i];
            if (existedTitles.Contains(t.Title)) continue;

            var entity = new NewsArticle
            {
                Title = t.Title,
                CoverUrl = t.CoverUrl,
                Summary = t.Summary,
                ContentType = "text",
                ContentHtml = t.ContentHtml,
                VideoUrl = null,
                Tags = t.Tags,
                ViewCount = 200 + (i * 37),
                Status = 1,
                PublishAt = now.AddDays(-i),
                CreatedAt = now.AddDays(-i),
                UpdatedAt = now.AddDays(-i),
            };
            _db.NewsArticles.Add(entity);
            created++;
        }

        if (created > 0) await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { created });
    }
}

