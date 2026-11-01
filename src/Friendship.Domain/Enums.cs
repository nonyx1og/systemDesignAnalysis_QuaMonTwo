namespace Friendship.Domain;

public enum VaiTro { BAN_HANG, SAN_XUAT, KHO, QUAN_LY, QUAN_TRI }
public enum LoaiDon { DAT_TRUOC, BAN_LE }
public enum TrangThaiDon { CHO_COC, DA_COC, DANG_LAM, CHO_GIAO, DA_GIAO, DA_THANH_TOAN, DA_HUY, BUNG_COC, DA_TRA }
public enum LoaiThanhToan { COC, TAT_TOAN, THANH_TOAN_DU, HOAN_COC, HOAN_TIEN }
public enum PhuongThucTT { TIEN_MAT, CHUYEN_KHOAN }
public enum LoaiGiam { PHAN_TRAM, SO_TIEN }
public enum TrangThaiDate { CON_HAN, CAN_DATE, HET_HAN }
public enum LyDoHuy { QUA_HAN, LOI_SAN_XUAT, KHACH_BUNG_COC, KHACH_TRA_HANG }
public enum TrangThaiPhieu { NHAP, CHO_DUYET, DA_DUYET, TU_CHOI }
public enum TrangThaiCa { DANG_MO, DA_CHOT }
public enum TinhTrangKhuyenMai { NGUNG, SAP_TOI, HET_HAN, DANG_AP_DUNG }
public enum NhomThanhPhan { COT, VIEN, TOPPING, IN_AN, TANG }
public enum TrangThaiThietKe { NHAP, CHO_DUYET, CAN_SUA, DA_BAO_GIA, DA_DAT, TU_CHOI, HET_HAN, KHACH_HUY }

public static class VaiTroExtensions
{
    public static string TenHienThi(this VaiTro v) => v switch
    {
        VaiTro.BAN_HANG => "Nhân viên bán hàng",
        VaiTro.SAN_XUAT => "Nhân viên sản xuất",
        VaiTro.KHO => "Nhân viên kho",
        VaiTro.QUAN_LY => "Quản lý cửa hàng",
        VaiTro.QUAN_TRI => "Quản trị viên",
        _ => v.ToString()
    };
}
