using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/about")]
[Authorize]
public sealed class AdminAboutController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminAboutController(AppDbContext db) => _db = db;

    // DTO for frontend data
    public class AboutInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? LogoUrl { get; set; }
        public string? Overview { get; set; }
        public string? History { get; set; }
        public string? Honors { get; set; }
    }

    [HttpGet]
    public async Task<ApiResponse<AboutInfoDto>> Get()
    {
        var page = await _db.AboutPages.FirstOrDefaultAsync(x => x.Code == "about_info");
        if (page is null)
        {
            return ApiResponse<AboutInfoDto>.Ok(new AboutInfoDto());
        }

        return ApiResponse<AboutInfoDto>.Ok(new AboutInfoDto
        {
            Name = page.Name ?? string.Empty,
            Address = page.Address,
            LogoUrl = page.LogoUrl,
            Overview = page.OverviewHtml,
            History = page.HistoryHtml,
            Honors = page.HonorsHtml,
        });
    }

    [HttpPost]
    public async Task<ApiResponse<object>> Update([FromBody] AboutInfoDto dto)
    {
        if (!DataUrlImage.IsValid(dto.LogoUrl)) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "Logo图片数据无效，请重新上传");
        var page = await _db.AboutPages.FirstOrDefaultAsync(x => x.Code == "about_info");
        if (page is null)
        {
            page = new AboutPage
            {
                Code = "about_info",
                Title = "协会基本信息",
                UpdatedAt = DateTime.UtcNow
            };
            _db.AboutPages.Add(page);
        }

        page.Name = dto.Name;
        page.Address = dto.Address;
        page.LogoUrl = dto.LogoUrl;
        page.OverviewHtml = dto.Overview;
        page.HistoryHtml = dto.History;
        page.HonorsHtml = dto.Honors;
        page.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id = page.Id });
    }
}
