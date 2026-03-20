using System;

namespace JumpRopeAss.Api.Infrastructure;

public static class DataUrlImage
{
    public static bool IsValid(string? dataUrl)
    {
        if (string.IsNullOrWhiteSpace(dataUrl)) return true;
        if (!dataUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase)) return true;

        var comma = dataUrl.IndexOf(',');
        if (comma <= 0) return false;
        var meta = dataUrl.Substring(0, comma);
        var payload = dataUrl.Substring(comma + 1);
        if (payload.Length == 0) return false;
        if (!meta.Contains(";base64", StringComparison.OrdinalIgnoreCase)) return false;

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(payload);
        }
        catch
        {
            return false;
        }

        if (bytes.Length < 12) return false;

        if (IsPng(bytes)) return true;
        if (IsJpeg(bytes)) return true;
        if (IsGif(bytes)) return true;
        if (IsWebp(bytes)) return true;
        if (IsBmp(bytes)) return true;
        if (IsIco(bytes)) return true;

        return false;
    }

    private static bool IsPng(byte[] b) =>
        b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47 && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A;

    private static bool IsJpeg(byte[] b) =>
        b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF;

    private static bool IsGif(byte[] b) =>
        b.Length >= 6 &&
        b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x38 &&
        (b[4] == 0x37 || b[4] == 0x39) && b[5] == 0x61;

    private static bool IsWebp(byte[] b) =>
        b.Length >= 12 &&
        b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46 &&
        b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50;

    private static bool IsBmp(byte[] b) =>
        b.Length >= 2 && b[0] == 0x42 && b[1] == 0x4D;

    private static bool IsIco(byte[] b) =>
        b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0x01 && b[3] == 0x00;
}

