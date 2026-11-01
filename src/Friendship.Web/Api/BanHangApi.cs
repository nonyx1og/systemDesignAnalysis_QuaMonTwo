using System.Security.Claims;
using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;

namespace Friendship.Web.Api;

public record DatApi(string? SoDienThoai, string? TenKhach, List<DongDat>? Dong, DateTime NgayGioLay, string? MaVoucher,
    int DiemSuDung, decimal? TienCoc, PhuongThucTT PhuongThuc, decimal? TienKhachDua);
public record DongThanhToanApi(PhuongThucTT PhuongThuc, decimal SoTien, decimal? TienKhachDua = null);
public record BanLeApi(string? SoDienThoai, string? TenKhach, List<DongBanLe>? Dong, string? MaVoucher, int DiemSuDung,
    PhuongThucTT PhuongThuc, decimal? TienKhachDua, List<DongThanhToanApi>? ThanhToans = null);
public record CocApi(decimal TienCoc, PhuongThucTT PhuongThuc, decimal? TienKhachDua);
public record GiaoApi(PhuongThucTT PhuongThuc, decimal? TienKhachDua);
public record HuyDonApi(KieuHuy Kieu, bool XacNhanDangLam);
public record TraHangApi(string? LyDo);
public record MoCaApi(decimal TienDauCa);
public record ChotCaApi(decimal TienMatThucTe, decimal ChuyenKhoanThucTe, string? GhiChu);

public static class BanHangApi
{
    public static void MapBanHangApi(this RouteGroupBuilder api)
    {
        api.MapGet("/khach-hang", async (string? sdt, KhachHangService s) =>
            await s.TimAsync(sdt) is { } k ? Results.Ok(k.ToDto()) : Results.NotFound())
            .RequireAuthorization(Policies.BanHang);

        var don = api.MapGroup("/don-hang");
        don.MapGet("/", async (string? q, TrangThaiDon? trangThai, LoaiDon? loai, DonDatService s) =>
            (await s.TimAsync(q, trangThai, loai)).Select(d => d.ToTomTat()))
            .RequireAuthorization(Policies.BanHang);
        don.MapGet("/{id:int}", async (int id, DonDatService s) =>
            await s.LayAsync(id) is { } d ? Results.Ok(d.ToDto()) : Results.NotFound())
            .RequireAuthorization(Policies.BanHang);

        don.MapPost("/dat", async (DatApi b, ClaimsPrincipal u, DonDatService s) =>
        {
            var d = await s.TaoAsync(new(u.NhanVienId(), b.SoDienThoai, b.TenKhach, b.Dong ?? [], b.NgayGioLay, b.MaVoucher,
                b.DiemSuDung, b.TienCoc, b.PhuongThuc, b.TienKhachDua));
            return Results.Created($"/api/don-hang/{d.Id}", (await s.LayAsync(d.Id))!.ToDto());
        }).RequireAuthorization(Policies.BanHang);

        don.MapPost("/ban-le", async (BanLeApi b, ClaimsPrincipal u, BanLeService s, DonDatService dd) =>
        {
            var ttList = b.ThanhToans?.Select(t => new DongThanhToan(t.PhuongThuc, t.SoTien, t.TienKhachDua)).ToList();
            var kq = await s.BanAsync(new(u.NhanVienId(), b.SoDienThoai, b.TenKhach, b.Dong ?? [], b.MaVoucher, b.DiemSuDung,
                b.PhuongThuc, b.TienKhachDua, ttList));
            return Results.Created($"/api/don-hang/{kq.Don.Id}", new { don = (await dd.LayAsync(kq.Don.Id))!.ToDto(), tienThoi = kq.TienThoi });
        }).RequireAuthorization(Policies.BanHang);

        don.MapPost("/{id:int}/thanh-toan", async (int id, CocApi b, ClaimsPrincipal u, DonDatService s) =>
        {
            await s.GhiNhanCocAsync(id, u.NhanVienId(), b.TienCoc, b.PhuongThuc, b.TienKhachDua);
            return Results.Ok((await s.LayAsync(id))!.ToDto());
        }).RequireAuthorization(Policies.BanHang);

        don.MapPost("/{id:int}/giao", async (int id, GiaoApi b, ClaimsPrincipal u, DonDatService s) =>
            Results.Ok(new { tienThoi = await s.GiaoAsync(id, u.NhanVienId(), b.PhuongThuc, b.TienKhachDua) }))
            .RequireAuthorization(Policies.BanHang);

        don.MapPost("/{id:int}/huy", async (int id, HuyDonApi b, ClaimsPrincipal u, DonDatService s) =>
        {
            await s.HuyAsync(id, u.NhanVienId(), b.Kieu, b.XacNhanDangLam);
            return Results.Ok((await s.LayAsync(id))!.ToDto());
        }).RequireAuthorization(Policies.BanHang);

        don.MapPost("/{id:int}/tra-hang", async (int id, TraHangApi b, ClaimsPrincipal u, BanLeService s, DonDatService dd) =>
        {
            await s.TraHangAsync(id, u.NhanVienId(), b?.LyDo ?? "");
            return Results.Ok((await dd.LayAsync(id))!.ToDto());
        }).RequireAuthorization(Policies.BanHang);

        don.MapPost("/quet-bung-coc", async (ClaimsPrincipal u, DonDatService s) =>
            (await s.QuetBungCocAsync(u.NhanVienId())).Select(d => d.Ma))
            .RequireAuthorization(Policies.QuanLy);

        // ----- Ca thu ngân -----
        var ca = api.MapGroup("/ca");
        ca.MapGet("/hien-tai", async (ClaimsPrincipal u, CaService s) =>
            await s.DangMoAsync(u.NhanVienId()) is { } c ? Results.Ok((await s.TongHopAsync(c.Id)).ToDto()) : Results.NotFound())
            .RequireAuthorization(Policies.BanHang);
        ca.MapPost("/mo", async (MoCaApi b, ClaimsPrincipal u, CaService s) =>
            (await s.TongHopAsync((await s.MoCaAsync(u.NhanVienId(), b.TienDauCa)).Id)).ToDto())
            .RequireAuthorization(Policies.BanHang);
        ca.MapPost("/chot", async (ChotCaApi b, ClaimsPrincipal u, CaService s) =>
            (await s.TongHopAsync((await s.ChotCaAsync(u.NhanVienId(), b.TienMatThucTe, b.ChuyenKhoanThucTe, b.GhiChu)).Id)).ToDto())
            .RequireAuthorization(Policies.BanHang);
        // BR25: nhân viên bán hàng chỉ xem ca của mình; quản lý xem mọi ca.
        ca.MapGet("/{id:int}", async (int id, ClaimsPrincipal u, CaService s) =>
        {
            TongHopCa th;
            try { th = await s.TongHopAsync(id); }
            catch (NghiepVuException) { return Results.NotFound(); }
            return u.IsInRole(Policies.QuanLy) || th.Ca.NhanVienId == u.NhanVienId() ? Results.Ok(th.ToDto()) : Results.NotFound();
        }).RequireAuthorization(p => p.RequireRole(Policies.BanHang, Policies.QuanLy));
    }
}
