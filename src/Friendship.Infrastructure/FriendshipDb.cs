using Friendship.Domain;
using Friendship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Friendship.Infrastructure;

public class FriendshipDb(DbContextOptions<FriendshipDb> options) : DbContext(options)
{
    public DbSet<NhanVien> NhanViens => Set<NhanVien>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<SanPham> SanPhams => Set<SanPham>();
    public DbSet<NguyenLieu> NguyenLieus => Set<NguyenLieu>();
    public DbSet<DinhLuong> DinhLuongs => Set<DinhLuong>();
    public DbSet<LoNguyenLieu> LoNguyenLieus => Set<LoNguyenLieu>();
    public DbSet<LoSanPham> LoSanPhams => Set<LoSanPham>();
    public DbSet<DonHang> DonHangs => Set<DonHang>();
    public DbSet<ChiTietDonHang> ChiTietDonHangs => Set<ChiTietDonHang>();
    public DbSet<KhuyenMai> KhuyenMais => Set<KhuyenMai>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
    public DbSet<CaThuNgan> CaThuNgans => Set<CaThuNgan>();
    public DbSet<PhieuXuatHuy> PhieuXuatHuys => Set<PhieuXuatHuy>();
    public DbSet<ChiTietXuatHuy> ChiTietXuatHuys => Set<ChiTietXuatHuy>();
    public DbSet<PhieuKiemKe> PhieuKiemKes => Set<PhieuKiemKe>();
    public DbSet<ChiTietKiemKe> ChiTietKiemKes => Set<ChiTietKiemKe>();
    public DbSet<ThanhPhanTrangTri> ThanhPhanTrangTris => Set<ThanhPhanTrangTri>();
    public DbSet<DinhLuongThanhPhan> DinhLuongThanhPhans => Set<DinhLuongThanhPhan>();
    public DbSet<ThietKeBanh> ThietKeBanhs => Set<ThietKeBanh>();
    public DbSet<ThietKeThanhPhan> ThietKeThanhPhans => Set<ThietKeThanhPhan>();

    protected override void ConfigureConventions(ModelConfigurationBuilder c)
    {
        c.Properties<decimal>().HavePrecision(18, 3);
        c.Properties<VaiTro>().HaveConversion<string>().HaveMaxLength(30);
        c.Properties<LoaiDon>().HaveConversion<string>().HaveMaxLength(30);
        c.Properties<TrangThaiDon>().HaveConversion<string>().HaveMaxLength(30);
        c.Properties<LoaiThanhToan>().HaveConversion<string>().HaveMaxLength(30);
        c.Properties<PhuongThucTT>().HaveConversion<string>().HaveMaxLength(30);
        c.Properties<LoaiGiam>().HaveConversion<string>().HaveMaxLength(30);
        c.Properties<LyDoHuy>().HaveConversion<string>().HaveMaxLength(30);
        c.Properties<TrangThaiPhieu>().HaveConversion<string>().HaveMaxLength(30);
        c.Properties<TrangThaiCa>().HaveConversion<string>().HaveMaxLength(30);
        c.Properties<NhomThanhPhan>().HaveConversion<string>().HaveMaxLength(30);
        c.Properties<TrangThaiThietKe>().HaveConversion<string>().HaveMaxLength(30);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Mặc định cấm xóa dây chuyền: chứng từ không bị xóa theo cha (spec 8.2).
        foreach (var fk in b.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        // Composition (◆): dòng chi tiết chỉ tồn tại trong phiếu/đơn.
        b.Entity<ChiTietDonHang>().HasOne(x => x.DonHang).WithMany(x => x.ChiTiets)
            .HasForeignKey(x => x.DonHangId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ChiTietXuatHuy>().HasOne(x => x.PhieuXuatHuy).WithMany(x => x.ChiTiets)
            .HasForeignKey(x => x.PhieuXuatHuyId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ChiTietKiemKe>().HasOne(x => x.PhieuKiemKe).WithMany(x => x.ChiTiets)
            .HasForeignKey(x => x.PhieuKiemKeId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<ThietKeThanhPhan>().HasOne(x => x.ThietKe).WithMany(x => x.ThanhPhans)
            .HasForeignKey(x => x.ThietKeId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<DinhLuongThanhPhan>().HasOne(x => x.ThanhPhan).WithMany(x => x.DinhLuongs)
            .HasForeignKey(x => x.ThanhPhanId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<DinhLuong>().HasKey(x => new { x.SanPhamId, x.NguyenLieuId });
        b.Entity<DinhLuongThanhPhan>().HasKey(x => new { x.ThanhPhanId, x.NguyenLieuId });
        b.Entity<NhanVien>().HasIndex(x => x.TenDangNhap).IsUnique();
        b.Entity<KhachHang>().HasIndex(x => x.SoDienThoai).IsUnique();
        b.Entity<KhuyenMai>().HasIndex(x => x.MaKhuyenMai).IsUnique();
        b.Entity<NguyenLieu>().HasIndex(x => x.TenGoi).IsUnique();

        // Chống ghi đè đồng thời (spec: hai người cùng thao tác một đơn/lô/ca).
        b.Entity<DonHang>().Property(x => x.RowVersion).IsRowVersion();
        b.Entity<LoNguyenLieu>().Property(x => x.RowVersion).IsRowVersion();
        b.Entity<LoSanPham>().Property(x => x.RowVersion).IsRowVersion();
        b.Entity<CaThuNgan>().Property(x => x.RowVersion).IsRowVersion();
        b.Entity<KhachHang>().Property(x => x.RowVersion).IsRowVersion();
        b.Entity<PhieuXuatHuy>().Property(x => x.RowVersion).IsRowVersion();
        b.Entity<PhieuKiemKe>().Property(x => x.RowVersion).IsRowVersion();
        b.Entity<ThietKeBanh>().Property(x => x.RowVersion).IsRowVersion();

        // UC16: không có hai phiếu kiểm kê nháp (hoặc hai phiếu chờ duyệt) cùng lúc, kể cả khi hai người bấm tạo đồng thời.
        b.Entity<PhieuKiemKe>().HasIndex(x => x.TrangThai).IsUnique()
            .HasFilter("[TrangThai] IN ('NHAP','CHO_DUYET')");

        // BR25: mỗi nhân viên bán hàng chỉ có một ca đang mở, kể cả khi bấm mở ca đồng thời.
        b.Entity<CaThuNgan>().HasIndex(x => x.NhanVienId).IsUnique().HasFilter("[TrangThai] = 'DANG_MO'");

        b.Entity<KhachHang>().ToTable(t => t.HasCheckConstraint("CK_KhachHang_Diem", "[DiemTichLuy] >= 0"));
        b.Entity<DinhLuong>().ToTable(t => t.HasCheckConstraint("CK_DinhLuong_DinhMuc", "[DinhMuc] > 0"));
        b.Entity<LoNguyenLieu>().ToTable(t => t.HasCheckConstraint("CK_LoNguyenLieu_Ton", "[SoLuongCon] >= 0"));
        b.Entity<LoSanPham>().ToTable(t => t.HasCheckConstraint("CK_LoSanPham_Ton",
            "[SoLuongTon] >= 0 AND [TyLeGiamXaHang] >= 0 AND [TyLeGiamXaHang] <= 1"));
        b.Entity<ChiTietDonHang>().ToTable(t => t.HasCheckConstraint("CK_ChiTietDonHang_SoLuong", "[SoLuong] > 0"));
        b.Entity<KhuyenMai>().ToTable(t => t.HasCheckConstraint("CK_KhuyenMai_Ngay", "[NgayKetThuc] >= [NgayBatDau]"));
        b.Entity<CaThuNgan>().ToTable(t => t.HasCheckConstraint("CK_CaThuNgan_TienDauCa", "[TienDauCa] >= 0"));
        b.Entity<ChiTietXuatHuy>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_ChiTietXuatHuy_SoLuong", "[SoLuongHuy] > 0");
            t.HasCheckConstraint("CK_ChiTietXuatHuy_Xor",
                "(CASE WHEN [LoNguyenLieuId] IS NULL THEN 0 ELSE 1 END " +
                "+ CASE WHEN [LoSanPhamId] IS NULL THEN 0 ELSE 1 END " +
                "+ CASE WHEN [DonHangId] IS NULL THEN 0 ELSE 1 END) = 1");
        });
        b.Entity<ChiTietKiemKe>().ToTable(t => t.HasCheckConstraint("CK_ChiTietKiemKe_LyDo",
            "[SoLuongThucTe] = [SoLuongHeThong] OR [LyDo] IS NOT NULL"));

        b.Entity<ThanhPhanTrangTri>(e =>
        {
            e.Property(x => x.GiaBan).HasPrecision(18, 2);
        });

        b.Entity<ThietKeBanh>(e =>
        {
            e.Property(x => x.MaTraCuu).HasMaxLength(22);
            e.HasIndex(x => x.MaTraCuu).IsUnique();
            e.Property(x => x.DuLieuJson).HasColumnType("nvarchar(max)");
            e.Property(x => x.GiaTamTinh).HasPrecision(18, 2);
            e.Property(x => x.PhuPhi).HasPrecision(18, 2);
            e.Property(x => x.GiaCuoi).HasPrecision(18, 2);
            e.Property(x => x.GiaVonDuKien).HasPrecision(18, 2);
            e.HasOne(x => x.DonHang).WithMany().HasForeignKey(x => x.DonHangId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ThietKeThanhPhan>(e =>
        {
            e.Property(x => x.DonGia).HasPrecision(18, 2);
        });

        b.Entity<ChiTietDonHang>(e =>
        {
            e.HasOne(x => x.ThietKeBanh).WithMany().HasForeignKey(x => x.ThietKeBanhId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<DinhLuongThanhPhan>().ToTable(t => t.HasCheckConstraint("CK_DinhLuongThanhPhan_DinhMuc", "[DinhMuc] > 0"));
    }
}
