using JumpRopeAss.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.Extensions.Options;

namespace JumpRopeAss.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public sealed class HealthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IOptions<MySqlOptions> _mysql;

    public HealthController(AppDbContext db, IOptions<MySqlOptions> mysql)
    {
        _db = db;
        _mysql = mysql;
    }

    [HttpGet]
    public ApiResponse<object> Get() => ApiResponse<object>.Ok(new { status = "ok" });

    [HttpGet("db")]
    public async Task<ApiResponse<object>> Db()
    {
        try
        {
            var can = await _db.Database.CanConnectAsync();
            if (!can) return ApiResponse<object>.Fail(ErrorCodes.ServerError, "db connect failed");
            var info = _mysql.Value;
            return ApiResponse<object>.Ok(new { status = "ok", host = info.Host, port = info.Port, database = info.Database });
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail(ErrorCodes.ServerError, ex.Message);
        }
    }
}

