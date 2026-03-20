using System.Text.Json;
using JumpRopeAss.Api.Contracts;
using JumpRopeAss.Api.Infrastructure;
using JumpRopeAss.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JumpRopeAss.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/system-settings")]
[Authorize]
public sealed class AdminSystemSettingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AdminSystemSettingsController(AppDbContext db) => _db = db;

    private const string KeySystem = "system";
    private const string KeyPayment = "payment";

    public sealed record SmsSettings(
        string? Provider,
        string? AccessKeyId,
        string? AccessKeySecret,
        string? SignName,
        string? TemplateCode);

    public sealed record StorageSettings(
        string? Provider,
        string? PublicBaseUrl,
        string? LocalBasePath,
        string? OssEndpoint,
        string? OssBucket,
        string? OssAccessKeyId,
        string? OssAccessKeySecret);

    public sealed record SystemSettings(
        string? SiteName,
        string? SiteShortName,
        string? SupportPhone,
        string? LogoUrl,
        string? DefaultEntryNoticeHtml,
        string? CertQueryPrefix,
        SmsSettings? Sms,
        StorageSettings? Storage);

    public sealed record PaymentSettings(
        bool Enabled,
        string? Provider,
        string? WechatMchId,
        string? WechatAppId,
        string? WechatSerialNo,
        string? WechatNotifyUrl,
        string? WechatApiV3Key,
        string? WechatPrivateKeyPem);

    public sealed record SettingsResponse(SystemSettings System, PaymentSettings Payment);

    public sealed record SettingsUpdateRequest(SystemSettings? System, PaymentSettings? Payment);

    private static T ParseOrDefault<T>(string? json, T fallback)
    {
        if (string.IsNullOrWhiteSpace(json)) return fallback;
        try
        {
            var v = JsonSerializer.Deserialize<T>(json!, JsonOpts);
            return v is null ? fallback : v;
        }
        catch
        {
            return fallback;
        }
    }

    private static string? MaskSecret(string? v)
    {
        var s = (v ?? string.Empty).Trim();
        if (s.Length == 0) return null;
        if (s.Length <= 6) return new string('*', s.Length);
        return s[..3] + new string('*', Math.Max(3, s.Length - 6)) + s[^3..];
    }

    [HttpGet]
    public async Task<ApiResponse<object>> Get()
    {
        var rows = await _db.SystemSettings.AsNoTracking()
            .Where(x => x.Key == KeySystem || x.Key == KeyPayment)
            .ToListAsync();

        var sysRow = rows.FirstOrDefault(x => x.Key == KeySystem);
        var payRow = rows.FirstOrDefault(x => x.Key == KeyPayment);

        var system = ParseOrDefault(
            sysRow?.ValueJson,
            new SystemSettings(
                null,
                null,
                null,
                null,
                null,
                null,
                new SmsSettings("none", null, null, null, null),
                new StorageSettings("local", null, null, null, null, null, null)));
        var paymentRaw = ParseOrDefault(payRow?.ValueJson, new PaymentSettings(false, "wechat", null, null, null, null, null, null));

        var systemMasked = system with
        {
            Sms = system.Sms is null
                ? null
                : system.Sms with { AccessKeySecret = MaskSecret(system.Sms.AccessKeySecret) },
            Storage = system.Storage is null
                ? null
                : system.Storage with { OssAccessKeySecret = MaskSecret(system.Storage.OssAccessKeySecret) }
        };

        var payment = paymentRaw with
        {
            WechatApiV3Key = MaskSecret(paymentRaw.WechatApiV3Key),
            WechatPrivateKeyPem = paymentRaw.WechatPrivateKeyPem is null ? null : "已保存（留空不修改）",
        };

        return ApiResponse<object>.Ok(new SettingsResponse(systemMasked, payment));
    }

    [HttpPut]
    public async Task<ApiResponse<object>> Update([FromBody] SettingsUpdateRequest req)
    {
        var now = DateTime.UtcNow;

        var sysRow = await _db.SystemSettings.FirstOrDefaultAsync(x => x.Key == KeySystem);
        var payRow = await _db.SystemSettings.FirstOrDefaultAsync(x => x.Key == KeyPayment);

        var curSystem = ParseOrDefault(
            sysRow?.ValueJson,
            new SystemSettings(
                null,
                null,
                null,
                null,
                null,
                null,
                new SmsSettings("none", null, null, null, null),
                new StorageSettings("local", null, null, null, null, null, null)));
        var curPayment = ParseOrDefault(payRow?.ValueJson, new PaymentSettings(false, "wechat", null, null, null, null, null, null));

        var nextSystem = req.System is null
            ? curSystem
            : new SystemSettings(
                req.System.SiteName ?? curSystem.SiteName,
                req.System.SiteShortName ?? curSystem.SiteShortName,
                req.System.SupportPhone ?? curSystem.SupportPhone,
                req.System.LogoUrl ?? curSystem.LogoUrl,
                req.System.DefaultEntryNoticeHtml ?? curSystem.DefaultEntryNoticeHtml,
                req.System.CertQueryPrefix ?? curSystem.CertQueryPrefix,
                req.System.Sms is null
                    ? curSystem.Sms
                    : new SmsSettings(
                        req.System.Sms.Provider ?? curSystem.Sms?.Provider,
                        req.System.Sms.AccessKeyId ?? curSystem.Sms?.AccessKeyId,
                        string.IsNullOrWhiteSpace(req.System.Sms.AccessKeySecret) ? curSystem.Sms?.AccessKeySecret : req.System.Sms.AccessKeySecret!.Trim(),
                        req.System.Sms.SignName ?? curSystem.Sms?.SignName,
                        req.System.Sms.TemplateCode ?? curSystem.Sms?.TemplateCode),
                req.System.Storage is null
                    ? curSystem.Storage
                    : new StorageSettings(
                        req.System.Storage.Provider ?? curSystem.Storage?.Provider,
                        req.System.Storage.PublicBaseUrl ?? curSystem.Storage?.PublicBaseUrl,
                        req.System.Storage.LocalBasePath ?? curSystem.Storage?.LocalBasePath,
                        req.System.Storage.OssEndpoint ?? curSystem.Storage?.OssEndpoint,
                        req.System.Storage.OssBucket ?? curSystem.Storage?.OssBucket,
                        req.System.Storage.OssAccessKeyId ?? curSystem.Storage?.OssAccessKeyId,
                        string.IsNullOrWhiteSpace(req.System.Storage.OssAccessKeySecret) ? curSystem.Storage?.OssAccessKeySecret : req.System.Storage.OssAccessKeySecret!.Trim()));

        PaymentSettings nextPayment;
        if (req.Payment is null)
        {
            nextPayment = curPayment;
        }
        else
        {
            var apiV3Key = string.IsNullOrWhiteSpace(req.Payment.WechatApiV3Key) ? curPayment.WechatApiV3Key : req.Payment.WechatApiV3Key!.Trim();
            var privateKeyPem = string.IsNullOrWhiteSpace(req.Payment.WechatPrivateKeyPem) ? curPayment.WechatPrivateKeyPem : req.Payment.WechatPrivateKeyPem;

            nextPayment = new PaymentSettings(
                req.Payment.Enabled,
                req.Payment.Provider ?? curPayment.Provider,
                req.Payment.WechatMchId ?? curPayment.WechatMchId,
                req.Payment.WechatAppId ?? curPayment.WechatAppId,
                req.Payment.WechatSerialNo ?? curPayment.WechatSerialNo,
                req.Payment.WechatNotifyUrl ?? curPayment.WechatNotifyUrl,
                apiV3Key,
                privateKeyPem);
        }

        var sysJson = JsonSerializer.Serialize(nextSystem, JsonOpts);
        var payJson = JsonSerializer.Serialize(nextPayment, JsonOpts);

        if (sysRow is null)
        {
            _db.SystemSettings.Add(new SystemSetting { Key = KeySystem, ValueJson = sysJson, CreatedAt = now, UpdatedAt = now });
        }
        else
        {
            sysRow.ValueJson = sysJson;
            sysRow.UpdatedAt = now;
        }

        if (payRow is null)
        {
            _db.SystemSettings.Add(new SystemSetting { Key = KeyPayment, ValueJson = payJson, CreatedAt = now, UpdatedAt = now });
        }
        else
        {
            payRow.ValueJson = payJson;
            payRow.UpdatedAt = now;
        }

        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(new { ok = true });
    }
}
