using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/identity-submits")]
[Authorize]
public sealed class AdminIdentityController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminIdentityController(AppDbContext db) => _db = db;

    private sealed record IdentityRaw(ulong Id, ulong UserId, string RealName, string IdCardNo, string Mobile, sbyte Status, string? RejectReason, DateTime CreatedAt);

    private static string MaskIdCard(string raw)
    {
        var s = (raw ?? string.Empty).Trim();
        if (s.Length <= 8) return s;
        var head = s[..4];
        var tail = s[^4..];
        return head + new string('*', s.Length - 8) + tail;
    }

    [HttpGet]
    public async Task<ApiResponse<object>> List(
        [FromQuery] int? status = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery(Name = "size")] int? size = null)
    {
        page = Math.Max(page, 1);
        if (size is not null) pageSize = size.Value;
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.UserIdentitySubmits.AsNoTracking().AsQueryable();
        if (status is not null) query = query.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            if (ulong.TryParse(kw, out var kwId))
                query = query.Where(x => x.Id == kwId || x.UserId == kwId);
            else
                query = query.Where(x => EF.Functions.Like(x.RealName, $"%{kw}%") || EF.Functions.Like(x.Mobile, $"%{kw}%"));
        }

        var total = await query.CountAsync();
        var raws = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new IdentityRaw(x.Id, x.UserId, x.RealName, x.IdCardNo, x.Mobile, x.Status, x.RejectReason, x.CreatedAt))
            .ToListAsync();

        var items = raws.Select(x => new
        {
            id = x.Id,
            userId = x.UserId,
            realName = x.RealName,
            idCardNoMasked = MaskIdCard(x.IdCardNo),
            mobile = x.Mobile,
            status = x.Status,
            rejectReason = x.RejectReason,
            createdAt = x.CreatedAt,
        }).ToList();

        return ApiResponse<object>.Ok(new { page, pageSize, total, items });
    }

    public sealed record ApproveRequest(string? Remark);
    public sealed record RejectRequest(string Reason);

    [HttpPost("{id:long}/approve")]
    public async Task<ApiResponse<object>> Approve([FromRoute] long id, [FromBody] ApproveRequest req)
    {
        var entity = await _db.UserIdentitySubmits.FirstOrDefaultAsync(x => x.Id == (ulong)id);
        if (entity is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "认证记录不存在");
        if (entity.Status != 0) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "当前状态不可审核");

        entity.Status = 1;
        entity.RejectReason = null;
        if (ulong.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
            entity.ReviewedByAdminId = adminId;
        entity.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { id, afterStatus = entity.Status });
    }

    [HttpPost("{id:long}/reject")]
    public async Task<ApiResponse<object>> Reject([FromRoute] long id, [FromBody] RejectRequest req)
    {
        var entity = await _db.UserIdentitySubmits.FirstOrDefaultAsync(x => x.Id == (ulong)id);
        if (entity is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "认证记录不存在");
        if (entity.Status != 0) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "当前状态不可审核");

        entity.Status = 2;
        entity.RejectReason = req.Reason;
        if (ulong.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
            entity.ReviewedByAdminId = adminId;
        entity.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { id, afterStatus = entity.Status });
    }
}

