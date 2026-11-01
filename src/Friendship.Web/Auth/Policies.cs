using Friendship.Domain;

namespace Friendship.Web.Auth;

public static class Policies
{
    public const string BanHang = nameof(VaiTro.BAN_HANG);
    public const string SanXuat = nameof(VaiTro.SAN_XUAT);
    public const string Kho = nameof(VaiTro.KHO);
    public const string QuanLy = nameof(VaiTro.QUAN_LY);
    public const string QuanTri = nameof(VaiTro.QUAN_TRI);
    public const string CanhBao = "CanhBao";
    public const string HuyHang = "HuyHang";
    public const string DeXuatNhap = "DeXuatNhap";
    public static readonly string[] Tat = [BanHang, SanXuat, Kho, QuanLy, QuanTri];
}
