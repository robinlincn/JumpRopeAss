using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/entries")]
[Authorize]
public sealed class AdminEntriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminEntriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ApiResponse<object>> List(
        [FromQuery] ulong? eventId = null,
        [FromQuery] int? status = null,
        [FromQuery] sbyte? enrollChannel = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery(Name = "size")] int? size = null)
    {
        page = Math.Max(page, 1);
        if (size is not null) pageSize = size.Value;
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.EventEntries.AsNoTracking().AsQueryable();
        if (eventId is not null) query = query.Where(x => x.EventId == eventId);
        if (status is not null) query = query.Where(x => x.Status == status);
        if (enrollChannel is not null) query = query.Where(x => x.EnrollChannel == enrollChannel);
        if (!string.IsNullOrWhiteSpace(keyword) && ulong.TryParse(keyword.Trim(), out var kwId))
        {
            query = query.Where(x =>
                x.Id == kwId ||
                x.AthletePersonId == kwId ||
                x.EnrollUserId == kwId ||
                x.PayOrderId == kwId);
        }

        var total = await query.CountAsync();
        var items = await (
                from x in query
                join ev0 in _db.Events.AsNoTracking() on x.EventId equals ev0.Id into ev1
                from ev in ev1.DefaultIfEmpty()
                join g0 in _db.EventGroups.AsNoTracking() on x.GroupId equals g0.Id into g1
                from g in g1.DefaultIfEmpty()
                join p0 in _db.People.AsNoTracking() on x.AthletePersonId equals p0.Id into p1
                from p in p1.DefaultIfEmpty()
                select new
                {
                    id = x.Id,
                    eventId = x.EventId,
                    eventTitle = ev != null ? ev.Title : null,
                    groupId = x.GroupId,
                    groupName = g != null ? g.Name : null,
                    athletePersonId = x.AthletePersonId,
                    athleteName = p != null ? p.FullName : null,
                    enrollChannel = x.EnrollChannel,
                    enrollUserId = x.EnrollUserId,
                    status = x.Status,
                    auditRemark = x.AuditRemark,
                    payOrderId = x.PayOrderId,
                    createdAt = x.CreatedAt,
                })
            .OrderByDescending(x => x.createdAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return ApiResponse<object>.Ok(new { page, pageSize, total, items });
    }

    public sealed record ApproveRequest(string? Remark);
    public sealed record RejectRequest(string Reason);

    [HttpPost("{id:long}/approve")]
    public async Task<ApiResponse<object>> Approve([FromRoute] long id, [FromBody] ApproveRequest req)
    {
        var entity = await _db.EventEntries.FirstOrDefaultAsync(x => x.Id == (ulong)id);
        if (entity is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "报名不存在");
        if (entity.Status != 0) return ApiResponse<object>.Fail(ErrorCodes.EntryStatusInvalid, "当前状态不可审核");

        entity.Status = 2;
        entity.AuditRemark = req.Remark;
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { id, afterStatus = entity.Status });
    }

    [HttpPost("{id:long}/reject")]
    public async Task<ApiResponse<object>> Reject([FromRoute] long id, [FromBody] RejectRequest req)
    {
        var entity = await _db.EventEntries.FirstOrDefaultAsync(x => x.Id == (ulong)id);
        if (entity is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "报名不存在");
        if (entity.Status != 0) return ApiResponse<object>.Fail(ErrorCodes.EntryStatusInvalid, "当前状态不可审核");

        entity.Status = 1;
        entity.AuditRemark = req.Reason;
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { id, afterStatus = entity.Status });
    }
}

