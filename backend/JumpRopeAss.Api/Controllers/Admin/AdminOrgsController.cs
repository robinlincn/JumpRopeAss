using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/orgs")]
[Authorize]
public sealed class AdminOrgsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminOrgsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ApiResponse<object>> GetList([FromQuery] sbyte? type, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var query = _db.Orgs.AsQueryable();
        if (type.HasValue) query = query.Where(x => x.OrgType == type.Value);
        if (!string.IsNullOrWhiteSpace(keyword)) query = query.Where(x => x.Name.Contains(keyword));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(x => x.UpdatedAt).ThenByDescending(x => x.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return ApiResponse<object>.Ok(new { total, items });
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResponse<Org>> GetDetail(long id)
    {
        var item = await _db.Orgs.FindAsync((ulong)id);
        if (item == null) return ApiResponse<Org>.Fail(ErrorCodes.NotFound, "机构不存在");
        return ApiResponse<Org>.Ok(item);
    }

    [HttpPost]
    public async Task<ApiResponse<Org>> Create([FromBody] OrgDto dto)
    {
        var now = DateTime.Now;
        var item = new Org
        {
            OrgType = dto.OrgType,
            Name = dto.Name,
            ShortName = dto.ShortName,
            Province = dto.Province,
            City = dto.City,
            District = dto.District,
            Address = dto.Address,
            ContactName = dto.ContactName,
            ContactPhone = dto.ContactPhone,
            Status = dto.Status ?? 1,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _db.Orgs.Add(item);
        await _db.SaveChangesAsync();
        return ApiResponse<Org>.Ok(item);
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResponse<Org>> Update(long id, [FromBody] OrgDto dto)
    {
        var item = await _db.Orgs.FindAsync((ulong)id);
        if (item == null) return ApiResponse<Org>.Fail(ErrorCodes.NotFound, "机构不存在");

        item.OrgType = dto.OrgType;
        item.Name = dto.Name;
        item.ShortName = dto.ShortName;
        item.Province = dto.Province;
        item.City = dto.City;
        item.District = dto.District;
        item.Address = dto.Address;
        item.ContactName = dto.ContactName;
        item.ContactPhone = dto.ContactPhone;
        item.Status = dto.Status ?? item.Status;
        item.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        return ApiResponse<Org>.Ok(item);
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResponse<object>> Delete(long id)
    {
        var item = await _db.Orgs.FindAsync((ulong)id);
        if (item == null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "机构不存在");

        _db.Orgs.Remove(item);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id });
    }
}

public sealed class OrgDto
{
    public sbyte OrgType { get; set; }
    public string Name { get; set; } = null!;
    public string? ShortName { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Address { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public sbyte? Status { get; set; }
}

