using System.Security.Claims;
using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/pay-orders")]
public sealed class AppPayOrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AppPayOrdersController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [Authorize]
    [HttpGet("{id:long}")]
    public async Task<ApiResponse<object>> Get([FromRoute] long id)
    {
        var userId = GetUserIdOrZero();
        if (userId == 0) return ApiResponse<object>.Fail(ErrorCodes.Unauthorized, "未登录");

        var order = await _db.PayOrders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == (ulong)id && x.UserId == userId);
        if (order is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "订单不存在");

        return ApiResponse<object>.Ok(new
        {
            id = order.Id,
            bizType = order.BizType,
            bizId = order.BizId,
            amount = order.Amount,
            status = order.Status,
            wxOutTradeNo = order.WxOutTradeNo,
            paidAt = order.PaidAt,
            createdAt = order.CreatedAt,
        });
    }

    [Authorize]
    [HttpPost("{id:long}/dev-pay")]
    public async Task<ApiResponse<object>> DevPay([FromRoute] long id)
    {
        if (!_env.IsDevelopment()) return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "仅开发环境可用");

        var userId = GetUserIdOrZero();
        if (userId == 0) return ApiResponse<object>.Fail(ErrorCodes.Unauthorized, "未登录");

        var order = await _db.PayOrders.FirstOrDefaultAsync(x => x.Id == (ulong)id && x.UserId == userId);
        if (order is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "订单不存在");

        if (order.Status == 1) return ApiResponse<object>.Ok(new { id = order.Id, status = order.Status });
        if (order.Status != 0) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "当前订单不可支付");

        order.Status = 1;
        order.PaidAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        if (order.BizType == 1)
        {
            var entry = await _db.EventEntries.FirstOrDefaultAsync(x => x.Id == order.BizId);
            if (entry != null)
            {
                entry.PayOrderId = order.Id;
                if (entry.Status == 2 || entry.Status == 3 || entry.Status == 4) entry.Status = 5;
                await _db.SaveChangesAsync();
            }
        }

        return ApiResponse<object>.Ok(new { id = order.Id, status = order.Status, paidAt = order.PaidAt });
    }

    private ulong GetUserIdOrZero()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return ulong.TryParse(sub, out var v) ? v : 0UL;
    }
}
