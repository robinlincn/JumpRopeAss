using JumpRopeAss.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace JumpRopeAss.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ApiResponse<object> Get() => ApiResponse<object>.Ok(new { status = "ok" });
}

