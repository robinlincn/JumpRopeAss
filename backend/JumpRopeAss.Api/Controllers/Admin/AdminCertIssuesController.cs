using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/cert-issues")]
[Authorize]
public sealed class AdminCertIssuesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminCertIssuesController(AppDbContext db) => _db = db;

    private static string CertNoOf(ulong eventId, ulong entryId) => $"CERT-E{eventId}-EN{entryId}";

    [HttpGet]
    public async Task<ApiResponse<object>> List(
        [FromQuery] ulong? eventId = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery(Name = "size")] int? size = null)
    {
        page = Math.Max(page, 1);
        if (size is not null) pageSize = size.Value;
        pageSize = Math.Clamp(pageSize, 1, 50);

        var entries = _db.EventEntries.AsNoTracking().Where(x => x.Status == 5);
        if (eventId is not null) entries = entries.Where(x => x.EventId == eventId);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            if (ulong.TryParse(kw, out var kwId))
            {
                entries = entries.Where(x => x.Id == kwId || x.EventId == kwId || x.GroupId == kwId || x.AthletePersonId == kwId || x.EnrollUserId == kwId);
            }
            else
            {
                entries = from e in entries
                    join p0 in _db.People.AsNoTracking() on e.AthletePersonId equals p0.Id into p1
                    from p in p1.DefaultIfEmpty()
                    where p != null && EF.Functions.Like(p.FullName, $"%{kw}%")
                    select e;
            }
        }

        var query = from e in entries
            join ev0 in _db.Events.AsNoTracking() on e.EventId equals ev0.Id into ev1
            from ev in ev1.DefaultIfEmpty()
            where ev != null && ev.Status == 4
            select new { e, ev };

        query = query.Where(x => !_db.Certificates.Any(c => c.CertNo == ("CERT-E" + x.e.EventId + "-EN" + x.e.Id)));

        var total = await query.CountAsync();
        var items = await (
                from x in query
                join g0 in _db.EventGroups.AsNoTracking() on x.e.GroupId equals g0.Id into g1
                from g in g1.DefaultIfEmpty()
                join p0 in _db.People.AsNoTracking() on x.e.AthletePersonId equals p0.Id into p1
                from p in p1.DefaultIfEmpty()
                select new
                {
                    entryId = x.e.Id,
                    eventId = x.e.EventId,
                    eventTitle = x.ev.Title,
                    groupId = x.e.GroupId,
                    groupName = g != null ? g.Name : null,
                    athletePersonId = x.e.AthletePersonId,
                    athleteName = p != null ? p.FullName : null,
                    athleteMobile = p != null ? p.Mobile : null,
                    enrollUserId = x.e.EnrollUserId,
                    createdAt = x.e.CreatedAt,
                    certNo = CertNoOf(x.e.EventId, x.e.Id),
                })
            .OrderByDescending(x => x.createdAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return ApiResponse<object>.Ok(new { page, pageSize, total, items });
    }

    public sealed record ApproveRequest(ulong? CertTypeId);

    [HttpPost("{entryId:long}/approve")]
    public async Task<ApiResponse<object>> Approve([FromRoute] long entryId, [FromBody] ApproveRequest req)
    {
        var entry = await _db.EventEntries.FirstOrDefaultAsync(x => x.Id == (ulong)entryId);
        if (entry is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "报名不存在");
        if (entry.Status != 5) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "仅已确认报名可发证");

        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entry.EventId);
        if (ev is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "活动不存在");
        if (ev.Status != 4) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "活动未结束，暂不可发证");

        var certTypeId = req.CertTypeId;
        if (certTypeId is null || certTypeId == 0)
        {
            var defaultType = await _db.CertTypes.AsNoTracking()
                .Where(x => x.Status == 1)
                .OrderBy(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
            if (defaultType == 0) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "未配置证书类型");
            certTypeId = defaultType;
        }
        else
        {
            var exists = await _db.CertTypes.AsNoTracking().AnyAsync(x => x.Id == certTypeId && x.Status == 1);
            if (!exists) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "证书类型不存在或未启用");
        }

        var certNo = CertNoOf(entry.EventId, entry.Id);
        var existed = await _db.Certificates.AsNoTracking().FirstOrDefaultAsync(x => x.CertNo == certNo);
        if (existed is not null)
        {
            return ApiResponse<object>.Ok(new { created = false, certificateId = existed.Id, certNo = existed.CertNo });
        }

        var now = DateTime.UtcNow;
        var cert = new Certificate
        {
            CertNo = certNo,
            CertTypeId = certTypeId.Value,
            HolderPersonId = entry.AthletePersonId,
            IssueScene = 1,
            IssueAt = now,
            Status = 1,
            FileUrl = null,
            CreatedAt = now,
        };
        _db.Certificates.Add(cert);
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { created = true, certificateId = cert.Id, certNo = cert.CertNo });
    }
}

