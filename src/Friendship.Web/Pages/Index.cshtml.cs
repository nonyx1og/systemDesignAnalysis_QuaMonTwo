using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages;

public class IndexModel(CanhBaoService canhBao, NguyenLieuService nguyenLieu, KiemKeService kiemKe,
    DonDatService donDat, BepService bep, CaService ca, HuyHangService huy) : PageModel
{
    public bool LaKho { get; set; }
    public bool LaQuanLy { get; set; }
    public bool LaBanHang { get; set; }
    public bool LaSanXuat { get; set; }
    public int LoHetHan { get; set; }
    public int LoCanDate { get; set; }
    public int NguyenLieuDuoiMuc { get; set; }
    public int PhieuChoDuyet { get; set; }
    public int PhieuHuyChoDuyet { get; set; }
    public bool CoCa { get; set; }
    public int DonChoGiao { get; set; }
    public int DonCanLam { get; set; }

    public async Task OnGetAsync()
    {
        var vaiTro = User.VaiTroHienTai();
        LaKho = vaiTro == VaiTro.KHO;
        LaQuanLy = vaiTro == VaiTro.QUAN_LY;
        LaBanHang = vaiTro == VaiTro.BAN_HANG;
        LaSanXuat = vaiTro == VaiTro.SAN_XUAT;
        if (LaKho || LaQuanLy)
        {
            var lo = await canhBao.NguyenLieuAsync();
            LoHetHan = lo.Count(x => x.TrangThai == TrangThaiDate.HET_HAN);
            LoCanDate = lo.Count(x => x.TrangThai == TrangThaiDate.CAN_DATE);
        }
        if (LaKho) NguyenLieuDuoiMuc = (await nguyenLieu.TimAsync(null)).Count(x => x.DuoiMuc);
        if (LaQuanLy) PhieuChoDuyet = (await kiemKe.TimAsync(TrangThaiPhieu.CHO_DUYET)).Count;
        if (LaQuanLy) PhieuHuyChoDuyet = (await huy.TimAsync(TrangThaiPhieu.CHO_DUYET)).Count;
        if (LaBanHang)
        {
            CoCa = await ca.DangMoAsync(User.NhanVienId()) is not null;
            DonChoGiao = await donDat.DemAsync(TrangThaiDon.CHO_GIAO);
        }
        if (LaSanXuat) DonCanLam = (await bep.DonCanLamAsync()).Count;
    }
}
