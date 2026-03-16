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
    public async Task<ApiResponse<object>> List([FromQuery] ulong? eventId = null, [FromQuery] int? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.EventEntries.AsNoTracking().AsQueryable();
        if (eventId is not null) query = query.Where(x => x.EventId == eventId);
        if (status is not null) query = query.Where(x => x.Status == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new
            {
                id = x.Id,
                eventId = x.EventId,
                groupId = x.GroupId,
                athletePersonId = x.AthletePersonId,
                enrollChannel = x.EnrollChannel,
                enrollUserId = x.EnrollUserId,
                status = x.Status,
                auditRemark = x.AuditRemark,
                payOrderId = x.PayOrderId,
                createdAt = x.CreatedAt,
            })
            .ToListAsync();

        return ApiResponse<object>.Ok(new { page, pageSize, total, items });
    }

    public sealed record ApproveRequest(string? Remark);
    public sealed record RejectRequest(string Reason);

    [HttpPost("{id:ulong}/approve")]
    public async Task<ApiResponse<object>> Approve([FromRoute] ulong id, [FromBody] ApproveRequest req)
    {
        var entity = await _db.EventEntries.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "报名不存在");
        if (entity.Status != 0) return ApiResponse<object>.Fail(ErrorCodes.EntryStatusInvalid, "当前状态不可审核");

        entity.Status = 2;
        entity.AuditRemark = req.Remark;
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { id, afterStatus = entity.Status });
    }

    [HttpPost("{id:ulong}/reject")]
    public async Task<ApiResponse<object>> Reject([FromRoute] ulong id, [FromBody] RejectRequest req)
    {
        var entity = await _db.EventEntries.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "报名不存在");
        if (entity.Status != 0) return ApiResponse<object>.Fail(ErrorCodes.EntryStatusInvalid, "当前状态不可审核");

        entity.Status = 1;
        entity.AuditRemark = req.Reason;
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { id, afterStatus = entity.Status });
    }
}

