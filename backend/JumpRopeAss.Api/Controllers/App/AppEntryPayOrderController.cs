using System.Security.Claims;
using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/event-entries")]
public sealed class AppEntryPayOrderController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppEntryPayOrderController(AppDbContext db) => _db = db;

    [Authorize]
    [HttpPost("{id:long}/pay-order")]
    public async Task<ApiResponse<object>> CreatePayOrder([FromRoute] long id)
    {
        var userId = GetUserIdOrZero();
        if (userId == 0) return ApiResponse<object>.Fail(ErrorCodes.Unauthorized, "未登录");

        var entry = await _db.EventEntries.FirstOrDefaultAsync(x => x.Id == (ulong)id && x.EnrollUserId == userId);
        if (entry is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "报名不存在");
        if (entry.Status != 2 && entry.Status != 3) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "当前状态不可缴费");
        if (entry.PayOrderId is not null) return ApiResponse<object>.Ok(new { entryId = entry.Id, payOrderId = entry.PayOrderId });

        var group = await _db.EventGroups.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entry.GroupId);
        if (group is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "组别不存在");

        var now = DateTime.UtcNow;
        var outTradeNo = await NewOutTradeNo();

        var order = new PayOrder
        {
            BizType = 1,
            BizId = entry.Id,
            UserId = userId,
            Amount = group.FeeAmount,
            Status = 0,
            WxOutTradeNo = outTradeNo,
            PaidAt = null,
            CreatedAt = now,
        };
        _db.PayOrders.Add(order);
        await _db.SaveChangesAsync();

        entry.PayOrderId = order.Id;
        entry.Status = 3;
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new
        {
            entryId = entry.Id,
            payOrder = new { id = order.Id, amount = order.Amount, status = order.Status, wxOutTradeNo = order.WxOutTradeNo },
        });
    }

    private async Task<string> NewOutTradeNo()
    {
        for (var i = 0; i < 5; i++)
        {
            var s = $"DEV{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100000, 999999)}";
            var exists = await _db.PayOrders.AsNoTracking().AnyAsync(x => x.WxOutTradeNo == s);
            if (!exists) return s;
        }
        return $"DEV{Guid.NewGuid():N}";
    }

    private ulong GetUserIdOrZero()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return ulong.TryParse(sub, out var v) ? v : 0UL;
    }
}
