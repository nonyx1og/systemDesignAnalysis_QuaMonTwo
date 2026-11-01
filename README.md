# Hệ thống Quản lý Tiệm bánh Friendship

Website nội bộ và đặt bánh tùy chỉnh cho tiệm bánh, viết bằng ASP.NET Core (Razor Pages) và SQL Server. Đây là bài tập môn Phân tích thiết kế hệ thống.

## Chức năng chính
- **Khách hàng**: đặt bánh, thiết kế bánh tùy chỉnh 2D, theo dõi thiết kế.
- **Bán hàng**: ca thu ngân, bán lẻ, đơn đặt trước, trả hàng.
- **Sản xuất**: duyệt thiết kế, đơn cần làm, ghi nhận mẻ bánh.
- **Kho**: nhập, xuất, kiểm kê, cảnh báo hạn sử dụng.
- **Quản lý và quản trị**: sản phẩm, nguyên liệu, khuyến mãi, duyệt phiếu, báo cáo, tài khoản và phân quyền.

## Yêu cầu
- .NET SDK 10
- SQL Server hoặc LocalDB (đi kèm Visual Studio)
- `dotnet-ef`: `dotnet tool install -g dotnet-ef`

## Chạy thử
1. Cấu hình chuỗi kết nối `ConnectionStrings:Friendship`. Nên dùng user-secrets hoặc biến môi trường, không ghi thông tin thật vào file đã commit:
   ```bash
   dotnet user-secrets set "ConnectionStrings:Friendship" "<chuỗi kết nối>" --project src/Friendship.Web
   ```
2. Chạy ứng dụng:
   ```bash
   dotnet run --project src/Friendship.Web
   ```
3. Ở môi trường Development, ứng dụng tự tạo CSDL và dữ liệu mẫu trong lần chạy đầu. Mật khẩu tài khoản mẫu đặt qua user-secrets:
   ```bash
   dotnet user-secrets set "DuLieuMau:MatKhau" "<mật khẩu>" --project src/Friendship.Web
   ```
   Nếu không đặt, ứng dụng sinh mật khẩu ngẫu nhiên và in ra console lần chạy đầu.

## Migration
```bash
dotnet ef migrations add <Ten> --project src/Friendship.Infrastructure --startup-project src/Friendship.Web
```

## Cấu trúc
- `src/`: mã nguồn ứng dụng
