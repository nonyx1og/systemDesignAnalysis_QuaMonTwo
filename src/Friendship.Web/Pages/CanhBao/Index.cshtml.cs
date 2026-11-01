using Friendship.Domain;
using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Friendship.Web.Pages.CanhBao;

public class IndexModel(CanhBaoService canhBao, NguyenLieuService nguyenLieu) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Tab { get; set; }
    [BindProperty(SupportsGet = true)] public int? Loc { get; set; }
    public bool ThayNguyenLieu { get; set; }
    public bool ThayBanh { get; set; }
    public bool ChoPhepLapPhieuHuy { get; set; }
    public string TabHienTai { get; set; } = "nl";
    public List<CanhBaoNguyenLieu> LoNguyenLieu { get; set; } = [];
    public List<CanhBaoBanh> LoBanh { get; set; } = [];
    public List<NguyenLieuTon> DanhSachNguyenLieu { get; set; } = [];

    public async Task OnGetAsync()
    {
        var vaiTro = User.VaiTroHienTai();
        ThayNguyenLieu = vaiTro is VaiTro.KHO or VaiTro.QUAN_LY;
        ThayBanh = vaiTro is VaiTro.BAN_HANG or VaiTro.QUAN_LY;
        ChoPhepLapPhieuHuy = vaiTro is VaiTro.KHO or VaiTro.BAN_HANG;     // UC17 A1: lập phiếu từ màn hình cảnh báo
        TabHienTai = (Tab, ThayNguyenLieu, ThayBanh) switch
        {
            ("banh", _, true) => "banh",
            ("nl", true, _) => "nl",
            (_, true, _) => "nl",
            _ => "banh"
        };
        if (TabHienTai == "nl")
        {
            LoNguyenLieu = await canhBao.NguyenLieuAsync(Loc);
            DanhSachNguyenLieu = await nguyenLieu.TimAsync(null);
        }
        else LoBanh = await canhBao.BanhAsync(Loc);
    }
}
