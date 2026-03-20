using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/pay-orders")]
[Authorize]
public sealed class AdminPayOrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminPayOrdersController(AppDbContext db) => _db = db;

    private sealed record PayOrderRaw(
        ulong Id,
        sbyte BizType,
        ulong BizId,
        ulong UserId,
        int Amount,
        sbyte Status,
        string? WxOutTradeNo,
        DateTime? PaidAt,
        DateTime CreatedAt);

    private sealed record UserInfo(string RealName, string Mobile);
    private sealed record EntryInfo(string? EventTitle, string? GroupName, string? AthleteName);
    private sealed record CertInfo(string? CertNo, string? CertTypeName, string? HolderName);

    [HttpGet]
    public async Task<ApiResponse<object>> List(
        [FromQuery] int? status = null,
        [FromQuery] sbyte? bizType = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery(Name = "size")] int? size = null)
    {
        page = Math.Max(page, 1);
        if (size is not null) pageSize = size.Value;
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.PayOrders.AsNoTracking().AsQueryable();
        if (status is not null) query = query.Where(x => x.Status == status);
        if (bizType is not null) query = query.Where(x => x.BizType == bizType);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            if (ulong.TryParse(kw, out var kwId))
            {
                query = query.Where(x => x.Id == kwId || x.UserId == kwId || x.BizId == kwId);
            }
            else
            {
                query = query.Where(x => x.WxOutTradeNo != null && EF.Functions.Like(x.WxOutTradeNo, $"%{kw}%"));
            }
        }

        var total = await query.CountAsync();
        var raws = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new PayOrderRaw(x.Id, x.BizType, x.BizId, x.UserId, x.Amount, x.Status, x.WxOutTradeNo, x.PaidAt, x.CreatedAt))
            .ToListAsync();

        var userIds = raws.Select(x => x.UserId).Distinct().ToList();
        var idnRows = await _db.UserIdentitySubmits.AsNoTracking()
            .Where(x => userIds.Contains(x.UserId) && x.Status == 1)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.UserId, x.RealName, x.Mobile })
            .ToListAsync();

        var userMap = idnRows
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => new UserInfo(g.First().RealName, g.First().Mobile));

        var entryIds = raws.Where(x => x.BizType == 1).Select(x => x.BizId).Distinct().ToList();
        var entryMap = entryIds.Count == 0
            ? new Dictionary<ulong, EntryInfo>()
            : (await (
                    from e in _db.EventEntries.AsNoTracking()
                    join ev0 in _db.Events.AsNoTracking() on e.EventId equals ev0.Id into ev1
                    from ev in ev1.DefaultIfEmpty()
                    join g0 in _db.EventGroups.AsNoTracking() on e.GroupId equals g0.Id into g1
                    from g in g1.DefaultIfEmpty()
                    join p0 in _db.People.AsNoTracking() on e.AthletePersonId equals p0.Id into p1
                    from p in p1.DefaultIfEmpty()
                    where entryIds.Contains(e.Id)
                    select new
                    {
                        entryId = e.Id,
                        eventTitle = ev != null ? ev.Title : null,
                        groupName = g != null ? g.Name : null,
                        athleteName = p != null ? p.FullName : null,
                    })
                .ToListAsync())
                .ToDictionary(x => x.entryId, x => new EntryInfo(x.eventTitle, x.groupName, x.athleteName));

        var certBizIds = raws.Where(x => x.BizType == 2 || x.BizType == 3).Select(x => x.BizId).Distinct().ToList();
        var certMap = certBizIds.Count == 0
            ? new Dictionary<ulong, CertInfo>()
            : (await (
                    from c in _db.Certificates.AsNoTracking()
                    join t0 in _db.CertTypes.AsNoTracking() on c.CertTypeId equals t0.Id into t1
                    from t in t1.DefaultIfEmpty()
                    join p0 in _db.People.AsNoTracking() on c.HolderPersonId equals p0.Id into p1
                    from p in p1.DefaultIfEmpty()
                    where certBizIds.Contains(c.Id)
                    select new
                    {
                        certId = c.Id,
                        certNo = c.CertNo,
                        certTypeName = t != null ? t.Name : null,
                        holderName = p != null ? p.FullName : null,
                    })
                .ToListAsync())
                .ToDictionary(x => x.certId, x => new CertInfo(x.certNo, x.certTypeName, x.holderName));

        string BizNameOf(PayOrderRaw raw)
        {
            if (raw.BizType == 1 && entryMap.TryGetValue(raw.BizId, out var x))
            {
                var a = x.EventTitle ?? $"报名#{raw.BizId}";
                var b = x.GroupName ?? "-";
                var c = x.AthleteName ?? "-";
                return $"{a} · {b} · {c}";
            }
            if ((raw.BizType == 2 || raw.BizType == 3) && certMap.TryGetValue(raw.BizId, out var y))
            {
                var a = y.CertTypeName ?? "证书";
                var b = y.HolderName ?? "-";
                var c = y.CertNo ?? $"#{raw.BizId}";
                return $"{a} · {b} · {c}";
            }
            return raw.BizType == 1 ? $"报名#{raw.BizId}" : $"证书业务#{raw.BizId}";
        }

        string? UserNameFallbackOf(PayOrderRaw raw)
        {
            if (raw.BizType == 1 && entryMap.TryGetValue(raw.BizId, out var x))
                return x.AthleteName;
            if ((raw.BizType == 2 || raw.BizType == 3) && certMap.TryGetValue(raw.BizId, out var y))
                return y.HolderName;
            return null;
        }

        var items = raws.Select(raw =>
        {
            userMap.TryGetValue(raw.UserId, out var u);
            return new
            {
                id = raw.Id,
                bizType = raw.BizType,
                bizId = raw.BizId,
                bizName = BizNameOf(raw),
                userId = raw.UserId,
                userName = u?.RealName ?? UserNameFallbackOf(raw),
                userMobile = u?.Mobile,
                amount = raw.Amount,
                status = raw.Status,
                wxOutTradeNo = raw.WxOutTradeNo,
                paidAt = raw.PaidAt,
                createdAt = raw.CreatedAt,
            };
        }).ToList();

        return ApiResponse<object>.Ok(new { page, pageSize, total, items });
    }
}

