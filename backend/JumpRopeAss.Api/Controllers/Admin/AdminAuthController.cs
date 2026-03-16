using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/auth")]
public sealed class AdminAuthController : ControllerBase
{
    private readonly JwtOptions _jwt;

    public AdminAuthController(IOptions<JwtOptions> jwt) => _jwt = jwt.Value;

    public sealed record LoginRequest(string Username, string Password);

    [HttpPost("login")]
    public ApiResponse<object> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "账号或密码不能为空");

        if (string.IsNullOrWhiteSpace(_jwt.SigningKey))
            return ApiResponse<object>.Fail(ErrorCodes.ServerError, "JWT_SIGNING_KEY未配置");

        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, req.Username),
            new(ClaimTypes.Role, "admin"),
        };

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

