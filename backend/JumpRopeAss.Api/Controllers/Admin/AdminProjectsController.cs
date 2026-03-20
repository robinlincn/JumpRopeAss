using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/projects")]
[Authorize]
public class AdminProjectsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminProjectsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ApiResponse<object>> GetList([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var query = _db.ProjectCatalogs.AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(x => x.Name.Contains(keyword));

        var total = await query.CountAsync();
        var items = await query.OrderBy(x => x.Id).Skip((page - 1) * size).Take(size).ToListAsync();

        return ApiResponse<object>.Ok(new { total, items });
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResponse<ProjectCatalog>> GetDetail(long id)
    {
        var item = await _db.ProjectCatalogs.FindAsync((ulong)id);
        if (item == null) return ApiResponse<ProjectCatalog>.Fail(ErrorCodes.NotFound, "项目不存在");
        return ApiResponse<ProjectCatalog>.Ok(item);
    }

    [HttpPost]
    public async Task<ApiResponse<ProjectCatalog>> Create([FromBody] ProjectDto dto)
    {
        var item = new ProjectCatalog
        {
            Code = dto.Code,
            Name = dto.Name,
            ParticipantCount = dto.ParticipantCount ?? 1,
            Status = dto.Status ?? 1,
            CreatedAt = DateTime.Now
        };
        _db.ProjectCatalogs.Add(item);
        await _db.SaveChangesAsync();
        return ApiResponse<ProjectCatalog>.Ok(item);
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResponse<ProjectCatalog>> Update(long id, [FromBody] ProjectDto dto)
    {
        var item = await _db.ProjectCatalogs.FindAsync((ulong)id);
        if (item == null) return ApiResponse<ProjectCatalog>.Fail(ErrorCodes.NotFound, "项目不存在");

        item.Code = dto.Code;
        item.Name = dto.Name;
        item.ParticipantCount = dto.ParticipantCount ?? 1;
        item.Status = dto.Status ?? 1;

        await _db.SaveChangesAsync();
        return ApiResponse<ProjectCatalog>.Ok(item);
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResponse<object>> Delete(long id)
    {
        var item = await _db.ProjectCatalogs.FindAsync((ulong)id);
        if (item == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "项目不存在");

        _db.ProjectCatalogs.Remove(item);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }
}

public class ProjectDto
{
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public int? ParticipantCount { get; set; }
    public sbyte? Status { get; set; }
}
