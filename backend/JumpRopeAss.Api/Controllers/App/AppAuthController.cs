using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JumpRopeAss.Api.Controllers.App;

[ApiController]
[Route("api/v1/app/auth")]
public sealed class AppAuthController : ControllerBase
{
    private readonly JwtOptions _jwt;
    private readonly IWebHostEnvironment _env;

    public AppAuthController(IOptions<JwtOptions> jwt, IWebHostEnvironment env)
    {
        _jwt = jwt.Value;
        _env = env;
    }

    public sealed record DevLoginRequest(ulong UserId);

    [HttpPost("dev-login")]
    public ApiResponse<object> DevLogin([FromBody] DevLoginRequest req)
    {
        if (!_env.IsDevelopment())
            return ApiResponse<object>.Fail(ErrorCodes.Forbidden, "仅开发环境可用");

        if (req.UserId == 0)
            return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "UserId非法");

        if (string.IsNullOrWhiteSpace(_jwt.SigningKey))
            return ApiResponse<object>.Fail(ErrorCodes.ServerError, "JWT_SIGNING_KEY未配置");

        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, req.UserId.ToString()),
            new(ClaimTypes.Role, "app"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddHours(24),
            signingCredentials: creds
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        return ApiResponse<object>.Ok(new { token = tokenStr, userId = req.UserId });
    }
}

