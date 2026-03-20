using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/dev")]
[Authorize]
public sealed class AdminDevToolsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminDevToolsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private bool EnsureDev() => _env.IsDevelopment();

    public sealed record CleanupUserRequest(ulong UserId);

    [HttpPost("cleanup-user")]
    public async Task<ApiResponse<object>> CleanupUser([FromBody] CleanupUserRequest req)
    {
        if (!EnsureDev()) return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "仅开发环境可用");
        if (req.UserId == 0) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "UserId非法");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var entries = await _db.EventEntries.Where(x => x.EnrollUserId == req.UserId).ToListAsync();
        var entryIds = entries.Select(x => x.Id).ToList();
        var athletePersonIds = entries.Select(x => x.AthletePersonId).Distinct().ToList();

        var payOrders = await _db.PayOrders
            .Where(x => x.UserId == req.UserId || (x.BizType == 1 && entryIds.Contains(x.BizId)))
            .ToListAsync();
        var payOrderIds = payOrders.Select(x => x.Id).ToList();

        var wechatNotifies = payOrderIds.Count == 0
            ? new List<Models.PayWechatNotify>()
            : await _db.PayWechatNotifies.Where(x => x.PayOrderId != null && payOrderIds.Contains(x.PayOrderId.Value)).ToListAsync();

        var certificates = athletePersonIds.Count == 0
            ? new List<Models.Certificate>()
            : await _db.Certificates.Where(x => athletePersonIds.Contains(x.HolderPersonId)).ToListAsync();

        var identitySubmits = await _db.UserIdentitySubmits.Where(x => x.UserId == req.UserId).ToListAsync();

        if (wechatNotifies.Count > 0) _db.PayWechatNotifies.RemoveRange(wechatNotifies);
        if (payOrders.Count > 0) _db.PayOrders.RemoveRange(payOrders);
        if (entries.Count > 0) _db.EventEntries.RemoveRange(entries);
        if (certificates.Count > 0) _db.Certificates.RemoveRange(certificates);
        if (identitySubmits.Count > 0) _db.UserIdentitySubmits.RemoveRange(identitySubmits);

        var deleted = await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return ApiResponse<object>.Ok(new
        {
            userId = req.UserId,
            deletedRows = deleted,
            entryCount = entries.Count,
            payOrderCount = payOrders.Count,
            payWechatNotifyCount = wechatNotifies.Count,
            identitySubmitCount = identitySubmits.Count,
            certificateCount = certificates.Count,
        });
    }
}

