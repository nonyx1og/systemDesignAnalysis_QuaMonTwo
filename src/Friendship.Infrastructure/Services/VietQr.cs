using System.Text;
using QRCoder;

namespace Friendship.Infrastructure.Services;

public static class VietQr
{
    public static string Crc16(string s)
    {
        ushort crc = 0xFFFF;
        var bytes = Encoding.UTF8.GetBytes(s);
        foreach (var b in bytes)
        {
            for (int i = 7; i >= 0; i--)
            {
                bool bit = ((b >> i) & 1) == 1;
                bool c15 = ((crc >> 15) & 1) == 1;
                crc = (ushort)(crc << 1);
                if (c15 ^ bit)
                {
                    crc = (ushort)(crc ^ 0x1021);
                }
            }
        }
        return crc.ToString("X4");
    }

    static string Tlv(string tag, string val) => $"{tag}{val.Length:D2}{val}";

    public static string ChuoiThanhToan(string maNganHang, string soTaiKhoan, decimal soTien, string noiDung)
    {
        // Tag 00: Payload Format Indicator = "01"
        // Tag 01: Point of Initiation Method = "12" (Dynamic QR có số tiền)
        // Tag 38: Merchant Account Information (NAPAS)
        //         Sub 00: GUID = "A000000727"
        //         Sub 01: Sub 00 (Mã ngân hàng BIN) + Sub 01 (Số tài khoản)
        //         Sub 02: Service Code = "QRIBFTTA"
        // Tag 53: Transaction Currency = "704" (VND)
        // Tag 54: Transaction Amount
        // Tag 58: Country Code = "VN"
        // Tag 62: Additional Data Field (Sub 08: Purpose of Transaction)
        // Tag 63: CRC = 04 + 4 ký tự hex CRC16-CCITT
        var sub01 = Tlv("00", maNganHang) + Tlv("01", soTaiKhoan);
        var tag38Value = Tlv("00", "A000000727") + Tlv("01", sub01) + Tlv("02", "QRIBFTTA");

        var sb = new StringBuilder();
        sb.Append(Tlv("00", "01"));
        sb.Append(Tlv("01", "12"));
        sb.Append(Tlv("38", tag38Value));
        sb.Append(Tlv("53", "704"));
        sb.Append(Tlv("54", ((long)soTien).ToString()));
        sb.Append(Tlv("58", "VN"));
        if (!string.IsNullOrEmpty(noiDung))
        {
            sb.Append(Tlv("62", Tlv("08", noiDung)));
        }
        sb.Append("6304");

        var phanTruocCrc = sb.ToString();
        var crc = Crc16(phanTruocCrc);
        return phanTruocCrc + crc;
    }

    public static byte[] Png(string chuoi)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(chuoi, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(data);
        return qrCode.GetGraphic(20);
    }
}
