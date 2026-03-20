using System.Security.Claims;
using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/identity")]
public sealed class AppIdentityController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppIdentityController(AppDbContext db) => _db = db;

    [Authorize]
    [HttpGet("status")]
    public async Task<ApiResponse<object>> Status()
    {
        var userId = GetUserIdOrZero();
        if (userId == 0) return ApiResponse<object>.Fail(ErrorCodes.Unauthorized, "未登录");

        var latest = await _db.UserIdentitySubmits.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.Id, x.Status, x.RejectReason, x.CreatedAt, x.RealName, x.Mobile, x.IdCardNo })
            .FirstOrDefaultAsync();

        return ApiResponse<object>.Ok(new { latest });
    }

    public sealed record SubmitRequest(string RealName, string IdCardNo, string Mobile);

    [Authorize]
    [HttpPost("submit")]
    public async Task<ApiResponse<object>> Submit([FromBody] SubmitRequest req)
    {
        var userId = GetUserIdOrZero();
        if (userId == 0) return ApiResponse<object>.Fail(ErrorCodes.Unauthorized, "未登录");

        var hasPending = await _db.UserIdentitySubmits.AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.Status == 0);
        if (hasPending) return ApiResponse<object>.Fail(ErrorCodes.IdnPendingExists, "已提交认证，请等待审核");

        var entity = new UserIdentitySubmit
        {
            UserId = userId,
            RealName = req.RealName.Trim(),
            IdCardNo = req.IdCardNo.Trim(),
            Mobile = req.Mobile.Trim(),
            Status = 0,
            CreatedAt = DateTime.UtcNow,
        };

        _db.UserIdentitySubmits.Add(entity);
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { submitId = entity.Id, status = entity.Status });
    }

    private ulong GetUserIdOrZero()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return ulong.TryParse(sub, out var v) ? v : 0UL;
    }
}

