using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Web.Pages.Dat;

public class MenuModel(FriendshipDb db) : PageModel
{
    public class BanhItem
    {
        public int Id { get; set; }
        public string TenBanh { get; set; } = "";
        public string KichThuoc { get; set; } = "";
        public decimal DonGia { get; set; }
        public int HanSuDungGio { get; set; }
        public string HinhAnh { get; set; } = "";
        public List<string> DanhSachThanhPhan { get; set; } = [];
        public int? LoTonHienCo { get; set; }
    }

    public List<BanhItem> DanhSachBanh { get; set; } = [];
    public string? TuKhoa { get; set; }

    public async Task OnGetAsync(string? q)
    {
        TuKhoa = q?.Trim();
        var query = db.SanPhams
            .Where(s => s.DangKinhDoanh)
            .Include(s => s.DinhLuongs)
                .ThenInclude(d => d.NguyenLieu)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(TuKhoa))
        {
            var k = TuKhoa.ToLower();
            query = query.Where(s => s.TenBanh.ToLower().Contains(k) || s.KichThuoc.ToLower().Contains(k));
        }

        var list = await query.ToListAsync();

        DanhSachBanh = list.Select(s => new BanhItem
        {
            Id = s.Id,
            TenBanh = s.TenBanh,
            KichThuoc = s.KichThuoc,
            DonGia = s.DonGia,
            HanSuDungGio = s.HanSuDungGio,
            HinhAnh = s.AnhHienThi,
            // CHỈ LẤY TÊN NGUYÊN LIỆU, TUYỆT ĐỐI KHÔNG CUNG CẤP ĐỊNH LƯỢNG (DinhMuc)
            DanhSachThanhPhan = s.DinhLuongs
                .Select(d => d.NguyenLieu.TenGoi)
                .Distinct()
                .OrderBy(x => x)
                .ToList()
        }).ToList();
    }
}
