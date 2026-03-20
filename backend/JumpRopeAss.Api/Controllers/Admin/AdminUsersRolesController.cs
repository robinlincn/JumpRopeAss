using System.Text.Json;
using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin")]
[Authorize]
public sealed class AdminUsersRolesController : ControllerBase
{
    private readonly AppDbContext _db;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AdminUsersRolesController(AppDbContext db) => _db = db;

    private static List<string> ParsePerms(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try
        {
            var arr = JsonSerializer.Deserialize<List<string>>(json, JsonOpts) ?? new List<string>();
            return arr.Select(x => (x ?? string.Empty).Trim()).Where(x => x.Length > 0).Distinct().OrderBy(x => x).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    [HttpGet("admin-roles")]
    public async Task<ApiResponse<object>> ListRoles()
    {
        var items = await _db.AdminRoles.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                id = x.Id,
                name = x.Name,
                code = x.Code,
                permissions = ParsePerms(x.PermissionsJson),
                createdAt = x.CreatedAt,
            })
            .ToListAsync();

        return ApiResponse<object>.Ok(new { items });
    }

    public sealed record CreateRoleRequest(string Name, string Code, List<string>? Permissions);

    [HttpPost("admin-roles")]
    public async Task<ApiResponse<object>> CreateRole([FromBody] CreateRoleRequest req)
    {
        var name = (req.Name ?? string.Empty).Trim();
        var code = (req.Code ?? string.Empty).Trim();
        if (name.Length == 0 || code.Length == 0) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "角色名称/编码必填");

        var exists = await _db.AdminRoles.AsNoTracking().AnyAsync(x => x.Code == code);
        if (exists) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "角色编码已存在");

        var perms = (req.Permissions ?? new List<string>()).Select(x => (x ?? string.Empty).Trim()).Where(x => x.Length > 0).Distinct().OrderBy(x => x).ToList();
        var now = DateTime.UtcNow;
        var role = new AdminRole { Name = name, Code = code, PermissionsJson = perms.Count == 0 ? null : JsonSerializer.Serialize(perms, JsonOpts), CreatedAt = now };
        _db.AdminRoles.Add(role);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { id = role.Id });
    }

    public sealed record UpdateRoleRequest(string? Name, string? Code, List<string>? Permissions);

    [HttpPut("admin-roles/{id}")]
    public async Task<ApiResponse<object>> UpdateRole([FromRoute] ulong id, [FromBody] UpdateRoleRequest req)
    {
        var role = await _db.AdminRoles.FirstOrDefaultAsync(x => x.Id == id);
        if (role is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "角色不存在");

        if (!string.IsNullOrWhiteSpace(req.Code))
        {
            var code = req.Code.Trim();
            var exists = await _db.AdminRoles.AsNoTracking().AnyAsync(x => x.Id != id && x.Code == code);
            if (exists) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "角色编码已存在");
            role.Code = code;
        }
        if (!string.IsNullOrWhiteSpace(req.Name)) role.Name = req.Name.Trim();

        if (req.Permissions is not null)
        {
            var perms = req.Permissions.Select(x => (x ?? string.Empty).Trim()).Where(x => x.Length > 0).Distinct().OrderBy(x => x).ToList();
            role.PermissionsJson = perms.Count == 0 ? null : JsonSerializer.Serialize(perms, JsonOpts);
        }

        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { ok = true });
    }

    [HttpDelete("admin-roles/{id}")]
    public async Task<ApiResponse<object>> DeleteRole([FromRoute] ulong id)
    {
        var role = await _db.AdminRoles.FirstOrDefaultAsync(x => x.Id == id);
        if (role is null) return ApiResponse<object>.Ok(new { ok = true });

        await using var tx = await _db.Database.BeginTransactionAsync();
        var links = await _db.AdminUserRoles.Where(x => x.AdminRoleId == id).ToListAsync();
        if (links.Count > 0) _db.AdminUserRoles.RemoveRange(links);
        _db.AdminRoles.Remove(role);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return ApiResponse<object>.Ok(new { ok = true });
    }

    [HttpGet("admin-users")]
    public async Task<ApiResponse<object>> ListUsers([FromQuery] string? keyword = null)
    {
        var query = _db.AdminUsers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            if (ulong.TryParse(kw, out var kwId))
                query = query.Where(x => x.Id == kwId);
            else
                query = query.Where(x => EF.Functions.Like(x.Username, $"%{kw}%"));
        }

        var items = await (
                from u in query
                join ur0 in _db.AdminUserRoles.AsNoTracking() on u.Id equals ur0.AdminUserId into ur1
                from ur in ur1.DefaultIfEmpty()
                join r0 in _db.AdminRoles.AsNoTracking() on ur.AdminRoleId equals r0.Id into r1
                from r in r1.DefaultIfEmpty()
                select new
                {
                    id = u.Id,
                    username = u.Username,
                    status = u.Status,
                    createdAt = u.CreatedAt,
                    roleId = r != null ? (ulong?)r.Id : null,
                    roleName = r != null ? r.Name : null,
                    roleCode = r != null ? r.Code : null,
                })
            .ToListAsync();

        var grouped = items
            .GroupBy(x => new { x.id, x.username, x.status, x.createdAt })
            .Select(g => new
            {
                id = g.Key.id,
                username = g.Key.username,
                status = g.Key.status,
                createdAt = g.Key.createdAt,
                roles = g.Where(x => x.roleId != null).Select(x => new { id = x.roleId!.Value, name = x.roleName, code = x.roleCode }).Distinct().ToList(),
                roleIds = g.Where(x => x.roleId != null).Select(x => x.roleId!.Value).Distinct().ToList(),
            })
            .OrderByDescending(x => x.createdAt)
            .ToList();

        return ApiResponse<object>.Ok(new { items = grouped });
    }

    public sealed record CreateUserRequest(string Username, string Password, sbyte Status, List<ulong>? RoleIds);

    [HttpPost("admin-users")]
    public async Task<ApiResponse<object>> CreateUser([FromBody] CreateUserRequest req)
    {
        var username = (req.Username ?? string.Empty).Trim();
        var password = req.Password ?? string.Empty;
        if (username.Length == 0 || password.Trim().Length < 6) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "账号必填，密码至少6位");

        var exists = await _db.AdminUsers.AsNoTracking().AnyAsync(x => x.Username == username);
        if (exists) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "账号已存在");

        var now = DateTime.UtcNow;
        var user = new AdminUser
        {
            Username = username,
            PasswordHash = AdminPasswordHasher.Hash(password),
            Status = req.Status == 1 ? (sbyte)1 : (sbyte)0,
            CreatedAt = now,
        };

        await using var tx = await _db.Database.BeginTransactionAsync();
        _db.AdminUsers.Add(user);
        await _db.SaveChangesAsync();

        var roleIds = (req.RoleIds ?? new List<ulong>()).Distinct().ToList();
        if (roleIds.Count > 0)
        {
            var validRoleIds = await _db.AdminRoles.AsNoTracking().Where(x => roleIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();
            foreach (var rid in validRoleIds)
            {
                _db.AdminUserRoles.Add(new AdminUserRole { AdminUserId = user.Id, AdminRoleId = rid, CreatedAt = now });
            }
            await _db.SaveChangesAsync();
        }

        await tx.CommitAsync();
        return ApiResponse<object>.Ok(new { id = user.Id });
    }

    public sealed record UpdateUserRequest(string? Username, sbyte? Status, List<ulong>? RoleIds);

    [HttpPut("admin-users/{id}")]
    public async Task<ApiResponse<object>> UpdateUser([FromRoute] ulong id, [FromBody] UpdateUserRequest req)
    {
        var user = await _db.AdminUsers.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "用户不存在");

        if (!string.IsNullOrWhiteSpace(req.Username))
        {
            var username = req.Username.Trim();
            var exists = await _db.AdminUsers.AsNoTracking().AnyAsync(x => x.Id != id && x.Username == username);
            if (exists) return ApiResponse<object>.Fail(ErrorCodes.Conflict, "账号已存在");
            user.Username = username;
        }

        if (req.Status is not null) user.Status = req.Status.Value == 1 ? (sbyte)1 : (sbyte)0;

        if (req.RoleIds is not null)
        {
            var now = DateTime.UtcNow;
            var nextIds = req.RoleIds.Distinct().ToList();
            var validRoleIds = await _db.AdminRoles.AsNoTracking().Where(x => nextIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();
            var curLinks = await _db.AdminUserRoles.Where(x => x.AdminUserId == id).ToListAsync();
            var curIds = curLinks.Select(x => x.AdminRoleId).ToHashSet();
            var nextSet = validRoleIds.ToHashSet();

            var toRemove = curLinks.Where(x => !nextSet.Contains(x.AdminRoleId)).ToList();
            if (toRemove.Count > 0) _db.AdminUserRoles.RemoveRange(toRemove);

            foreach (var rid in validRoleIds)
            {
                if (curIds.Contains(rid)) continue;
                _db.AdminUserRoles.Add(new AdminUserRole { AdminUserId = id, AdminRoleId = rid, CreatedAt = now });
            }
        }

        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { ok = true });
    }

    public sealed record ResetPasswordRequest(string Password);

    [HttpPut("admin-users/{id}/password")]
    public async Task<ApiResponse<object>> ResetPassword([FromRoute] ulong id, [FromBody] ResetPasswordRequest req)
    {
        var pwd = (req.Password ?? string.Empty).Trim();
        if (pwd.Length < 6) return ApiResponse<object>.Fail(ErrorCodes.InvalidParam, "密码至少6位");
        var user = await _db.AdminUsers.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null) return ApiResponse<object>.Fail(ErrorCodes.NotFound, "用户不存在");
        user.PasswordHash = AdminPasswordHasher.Hash(pwd);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { ok = true });
    }

    [HttpDelete("admin-users/{id}")]
    public async Task<ApiResponse<object>> DeleteUser([FromRoute] ulong id)
    {
        var user = await _db.AdminUsers.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null) return ApiResponse<object>.Ok(new { ok = true });

        await using var tx = await _db.Database.BeginTransactionAsync();
        var links = await _db.AdminUserRoles.Where(x => x.AdminUserId == id).ToListAsync();
        if (links.Count > 0) _db.AdminUserRoles.RemoveRange(links);
        _db.AdminUsers.Remove(user);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return ApiResponse<object>.Ok(new { ok = true });
    }
}

