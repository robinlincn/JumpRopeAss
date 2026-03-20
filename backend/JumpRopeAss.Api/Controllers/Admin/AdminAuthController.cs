using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/auth")]
public sealed class AdminAuthController : ControllerBase
{
    private readonly JwtOptions _jwt;
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminAuthController(IOptions<JwtOptions> jwt, AppDbContext db, IWebHostEnvironment env)
    {
        _jwt = jwt.Value;
        _db = db;
        _env = env;
    }

    public sealed record LoginRequest(string Username, string Password);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ApiResponse<object>> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "账号或密码不能为空");

        if (string.IsNullOrWhiteSpace(_jwt.SigningKey))
            return ApiResponse<object>.Fail(ErrorCodes.ServerError, "JWT_SIGNING_KEY未配置");

        var username = req.Username.Trim();
        var password = req.Password;

        var user = await _db.AdminUsers.FirstOrDefaultAsync(x => x.Username == username);
        if (user is null) return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "账号或密码错误");
        if (user.Status != 1) return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "账号已停用");

        var allowLegacyPlain = _env.IsDevelopment();
        if (!AdminPasswordHasher.Verify(password, user.PasswordHash, allowLegacyPlain))
            return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "账号或密码错误");

        var roleCodes = await (
                from ur in _db.AdminUserRoles.AsNoTracking()
                join r in _db.AdminRoles.AsNoTracking() on ur.AdminRoleId equals r.Id
                where ur.AdminUserId == user.Id
                select r.Code)
            .Distinct()
            .ToListAsync();

        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
        };
        foreach (var code in roleCodes)
        {
            if (!string.IsNullOrWhiteSpace(code)) claims.Add(new Claim(ClaimTypes.Role, code));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddHours(12),
            signingCredentials: creds
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        return ApiResponse<object>.Ok(new { token = tokenStr });
    }
}

