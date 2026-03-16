using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/identity-submits")]
[Authorize]
public sealed class AdminIdentityController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminIdentityController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ApiResponse<object>> List([FromQuery] int? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.UserIdentitySubmits.AsNoTracking().AsQueryable();
        if (status is not null) query = query.Where(x => x.Status == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new
            {
                id = x.Id,
                userId = x.UserId,
                realName = x.RealName,
                mobile = x.Mobile,
                status = x.Status,
                rejectReason = x.RejectReason,
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
        var entity = await _db.UserIdentitySubmits.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "认证记录不存在");
        if (entity.Status != 0) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "当前状态不可审核");

        entity.Status = 1;
        entity.RejectReason = null;
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { id, afterStatus = entity.Status });
    }

    [HttpPost("{id:ulong}/reject")]
    public async Task<ApiResponse<object>> Reject([FromRoute] ulong id, [FromBody] RejectRequest req)
    {
        var entity = await _db.UserIdentitySubmits.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "认证记录不存在");
        if (entity.Status != 0) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "当前状态不可审核");

        entity.Status = 2;
        entity.RejectReason = req.Reason;
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { id, afterStatus = entity.Status });
    }
}

