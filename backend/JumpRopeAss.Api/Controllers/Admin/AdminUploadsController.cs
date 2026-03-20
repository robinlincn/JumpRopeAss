using JumpRopeAss.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/uploads")]
[Authorize]
public class AdminUploadsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    private static readonly HashSet<string> AllowedExts = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    public AdminUploadsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ApiResponse<object>> UploadImage([FromForm] IFormFile file)
    {
        if (file == null || file.Length <= 0) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "请选择文件");
        if (file.Length > 10 * 1024 * 1024) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "图片过大，请选择小于10MB的图片");

        var ext = Path.GetExtension(file.FileName) ?? "";
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExts.Contains(ext))
            return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "仅支持 jpg/jpeg/png/webp/gif 图片");

        if (!string.IsNullOrWhiteSpace(file.ContentType) && !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "文件类型不正确");

        var root = Path.Combine(_env.ContentRootPath, "upload");
        var dateDir = DateTime.Now.ToString("yyyyMMdd");
        var dir = Path.Combine(root, dateDir);
        Directory.CreateDirectory(dir);

        var name = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(dir, name);
        await using (var fs = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(fs);
        }

        var url = $"/upload/{dateDir}/{name}";
        return ApiResponse<object>.Ok(new { url });
    }
}
