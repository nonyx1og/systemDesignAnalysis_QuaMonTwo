using Friendship.Domain;
using Friendship.Infrastructure.Services;

namespace Friendship.Web.Api;

public static class DatBanhApi
{
    public static void MapDatApi(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/dat");
        g.MapPost("/gia", async (System.Text.Json.JsonElement body, ThietKeService thietKe) =>
        {
            var rawJson = body.GetRawText();
            var kq = await thietKe.TinhGiaAsync(rawJson);
            return Results.Ok(new { giaNen = kq.GiaNen, giaThanhPhan = kq.GiaThanhPhan, giaTamTinh = kq.GiaTamTinh });
        }).AllowAnonymous().RequireRateLimiting("khach");
    }

    public static void MapDatAnhApi(this WebApplication app)
    {
        app.MapGet("/dat/anh/{ma}/{loai}", async (string ma, string loai, ThietKeService thietKe, AnhService anh, HttpContext http) =>
        {
            var tk = await thietKe.LayTheoMaAsync(ma);
            if (tk is null) return Results.NotFound();

            if (loai == "qr")
            {
                if (tk.TrangThai != TrangThaiThietKe.DA_BAO_GIA) return Results.NotFound();
                var config = http.RequestServices.GetRequiredService<IConfiguration>();
                var maNganHang = config["VietQr:MaNganHang"] ?? "970436";
                var soTaiKhoan = config["VietQr:SoTaiKhoan"] ?? "0123456789";
                var minCoc = CocRule.Khoang(tk.GiaCuoi).Min;
                var chuoi = VietQr.ChuoiThanhToan(maNganHang, soTaiKhoan, minCoc, tk.Ma);
                var png = VietQr.Png(chuoi);
                http.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                return Results.File(png, "image/png");
            }

            string? tenFile = loai switch
            {
                "thiet-ke" => tk.AnhThietKe,
                "in" => tk.AnhInAn,
                "thanh-pham" => tk.AnhThanhPham,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(tenFile)) return Results.NotFound();

            var stream = anh.Mo(tenFile);
            if (stream is null) return Results.NotFound();

            http.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            return Results.Stream(stream, AnhService.ContentType(tenFile));
        }).AllowAnonymous();
    }
}
