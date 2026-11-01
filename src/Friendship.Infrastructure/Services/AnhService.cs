using System.Text.RegularExpressions;
using Friendship.Domain;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace Friendship.Infrastructure.Services;

public partial class AnhService(string thuMuc)
{
    public const long KichThuocToiDa = ThietKeRule.AnhDungLuongToiDa;
    public const int CanhToiDa = ThietKeRule.AnhKichThuocToiDa;

    [GeneratedRegex(@"^[0-9a-f]{32}\.(png|jpg)$", RegexOptions.IgnoreCase)]
    private static partial Regex TenFileRe();

    public async Task<string> LuuAsync(Stream nguon)
    {
        using var ms = new MemoryStream();
        var buffer = new byte[65536];
        long total = 0;
        int read;
        while ((read = await nguon.ReadAsync(buffer)) > 0)
        {
            total += read;
            if (total > KichThuocToiDa)
                throw new NghiepVuException("Ảnh tối đa 5 MB.");
            ms.Write(buffer, 0, read);
        }

        if (total == 0)
            throw new NghiepVuException("Ảnh trống.");

        ms.Position = 0;
        IImageFormat? format = null;
        try
        {
            format = await Image.DetectFormatAsync(ms);
        }
        catch (UnknownImageFormatException)
        {
            throw new NghiepVuException("Chỉ nhận ảnh JPG hoặc PNG.");
        }

        ms.Position = 0;
        if (format is not (PngFormat or JpegFormat))
            throw new NghiepVuException("Chỉ nhận ảnh JPG hoặc PNG.");

        ms.Position = 0;
        var info = await Image.IdentifyAsync(ms);
        ms.Position = 0;
        if (info is null)
            throw new NghiepVuException("Chỉ nhận ảnh JPG hoặc PNG.");

        if (info.Width > CanhToiDa || info.Height > CanhToiDa)
            throw new NghiepVuException("Ảnh quá lớn (tối đa 4096×4096 px).");

        var ext = format is PngFormat ? ".png" : ".jpg";
        var tenFile = $"{Guid.NewGuid():N}{ext}";
        Directory.CreateDirectory(thuMuc);
        var duongDan = Path.Combine(thuMuc, tenFile);

        using var img = await Image.LoadAsync(ms);
        // Ảnh chụp điện thoại thường có GPS trong EXIF; trang khách hiển thị lại ảnh nên phải xóa metadata.
        img.Metadata.ExifProfile = null;
        img.Metadata.IptcProfile = null;
        img.Metadata.XmpProfile = null;
        await img.SaveAsync(duongDan);
        return tenFile;
    }

    public Stream? Mo(string tenFile)
    {
        if (string.IsNullOrWhiteSpace(tenFile) || !TenFileRe().IsMatch(tenFile))
            return null;

        var duongDan = Path.Combine(thuMuc, tenFile);
        if (!File.Exists(duongDan))
            return null;

        return File.OpenRead(duongDan);
    }

    public static string ContentType(string tenFile) => Path.GetExtension(tenFile).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        _ => "application/octet-stream"
    };
}
