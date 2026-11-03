using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Friendship.Domain;
using Friendship.Domain.Entities;
using Friendship.Infrastructure;
using Friendship.Infrastructure.Services;
using Friendship.Web;
using Friendship.Web.Api;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Bind số từ form (type="number" luôn dùng dấu chấm) không phụ thuộc locale máy chủ; hiển thị dùng Fmt (vi-VN).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

builder.Services.AddDbContext<FriendshipDb>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Friendship")));
builder.Services.AddSingleton<LoginThrottle>();
builder.Services.AddScoped<TaiKhoanService>();
builder.Services.AddScoped<NhanVienService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<NguyenLieuService>();
builder.Services.AddScoped<SanPhamService>();
builder.Services.AddScoped<ThanhPhanService>();
builder.Services.AddScoped<ThietKeService>();
builder.Services.AddScoped<KhuyenMaiService>();
builder.Services.AddScoped<KhoService>();
builder.Services.AddScoped<CanhBaoService>();
builder.Services.AddScoped<KiemKeService>();
builder.Services.AddSingleton<NonceStore>();
var thuMucAnh = builder.Configuration["LuuTru:ThuMucAnh"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "anh");
builder.Services.AddSingleton(new AnhService(thuMucAnh));
builder.Services.AddScoped<KhachHangService>();
builder.Services.AddScoped<KhachHangAuthService>();
builder.Services.AddScoped<CaService>();
builder.Services.AddScoped<CaNhanVienService>();
builder.Services.AddScoped<DinhLuongService>();
builder.Services.AddScoped<ThanhToanService>();
builder.Services.AddScoped<LoBanhService>();
builder.Services.AddScoped<XaHangService>();
builder.Services.AddScoped<BanLeService>();
builder.Services.AddScoped<DonDatService>();
builder.Services.AddScoped<BepService>();
builder.Services.AddScoped<HuyHangService>();
builder.Services.AddScoped<BaoCaoService>();
builder.Services.AddFriendshipApi();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("khach", httpContext =>
        httpContext.Connection.RemoteIpAddress is not { } ip
            ? RateLimitPartition.GetNoLimiter("khong-ip")   // chỉ gặp ở TestServer; Kestrel luôn có IP
            : RateLimitPartition.GetFixedWindowLimiter(
            ip.ToString(),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/AccessDenied";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        // Mỗi yêu cầu kiểm tra lại tài khoản còn hoạt động và đúng vai trò (khóa/đổi vai trò có hiệu lực ngay).
        // ponytail: 1 truy vấn nhỏ mỗi yêu cầu; thêm cache ngắn hạn nếu số người dùng đồng thời lớn.
        o.Events.OnValidatePrincipal = async ctx =>
        {
            var ok = false;
            if (int.TryParse(ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                && Enum.TryParse<VaiTro>(ctx.Principal?.FindFirstValue(ClaimTypes.Role), out var vaiTro))
            {
                var hash = await ctx.HttpContext.RequestServices.GetRequiredService<FriendshipDb>().NhanViens
                    .Where(x => x.Id == id && x.DangHoatDong && x.VaiTro == vaiTro)
                    .Select(x => x.MatKhauHash).FirstOrDefaultAsync();
                ok = hash is not null
                    && ctx.Principal!.FindFirstValue(SignInHelper.ClaimMaXacThuc) == NhanVien.MaXacThuc(hash);
            }
            if (!ok)
            {
                ctx.RejectPrincipal();
                await ctx.HttpContext.SignOutAsync();
            }
        };
        // API trả mã trạng thái thay vì chuyển hướng sang trang HTML đăng nhập; ngoài /api giữ hành vi mặc định
        // (mặc định đã trả 401 cho fetch có X-Requested-With, màn hình bếp dựa vào đó).
        var veDangNhap = o.Events.OnRedirectToLogin;
        var khongCoQuyen = o.Events.OnRedirectToAccessDenied;
        o.Events.OnRedirectToLogin = ctx =>
        {
            if (!ctx.Request.Path.StartsWithSegments("/api")) return veDangNhap(ctx);
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        o.Events.OnRedirectToAccessDenied = ctx =>
        {
            if (!ctx.Request.Path.StartsWithSegments("/api")) return khongCoQuyen(ctx);
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    })
    .AddCookie(CustomerAuthHelper.Scheme, o =>
    {
        o.Cookie.Name = ".Friendship.Customer";
        o.LoginPath = "/Dat/DangNhap";
        o.AccessDeniedPath = "/Dat/DangNhap";
        o.ExpireTimeSpan = TimeSpan.FromDays(30);
        o.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(o =>
{
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    foreach (var ten in Policies.Tat) o.AddPolicy(ten, p => p.RequireRole(ten));
    o.AddPolicy(Policies.CanhBao, p => p.RequireRole(Policies.Kho, Policies.BanHang, Policies.QuanLy));
    o.AddPolicy(Policies.HuyHang, p => p.RequireRole(Policies.Kho, Policies.BanHang, Policies.SanXuat));
    o.AddPolicy(Policies.DeXuatNhap, p => p.RequireRole(Policies.Kho, Policies.QuanLy));
    o.AddPolicy("TonKhoPolicy", p => p.RequireRole(Policies.Kho, Policies.QuanLy, Policies.QuanTri));
    o.AddPolicy("GiaVonPolicy", p => p.RequireRole(Policies.Kho, Policies.SanXuat, Policies.QuanLy, Policies.QuanTri));
});

builder.Services.AddRazorPages(o =>
{
    o.Conventions.AuthorizeFolder("/Admin", Policies.QuanTri);
    o.Conventions.AuthorizeFolder("/QuanLy", Policies.QuanLy);
    o.Conventions.AddFolderApplicationModelConvention("/Kho", model =>
    {
        if (model.ViewEnginePath.Equals("/Kho/DeXuatNhap", StringComparison.OrdinalIgnoreCase))
            model.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(Policies.DeXuatNhap));
        else if (model.ViewEnginePath.Equals("/Kho/TonKho", StringComparison.OrdinalIgnoreCase))
            model.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter("TonKhoPolicy"));
        else if (model.ViewEnginePath.Equals("/Kho/GiaVon", StringComparison.OrdinalIgnoreCase))
            model.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter("GiaVonPolicy"));
        else
            model.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(Policies.Kho));
    });
    o.Conventions.AuthorizeFolder("/BanHang", Policies.BanHang);
    o.Conventions.AuthorizeFolder("/Bep", Policies.SanXuat);
    o.Conventions.AuthorizeFolder("/CanhBao", Policies.CanhBao);
    o.Conventions.AuthorizeFolder("/Huy", Policies.HuyHang);
    o.Conventions.AllowAnonymousToPage("/Account/Login");
    o.Conventions.AllowAnonymousToPage("/Error");
    o.Conventions.AllowAnonymousToFolder("/Dat");
    o.Conventions.AllowAnonymousToPage("/Dat/Index");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // Môi trường phát triển: tự áp migration và seed để chạy ngay.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FriendshipDb>();
    await db.Database.MigrateAsync();
    // Mật khẩu tài khoản mẫu lấy từ cấu hình; không có thì sinh ngẫu nhiên và in ra console một lần.
    DbSeeder.MatKhauMau = app.Configuration["DuLieuMau:MatKhau"] is { Length: > 0 } mk ? mk : $"Fs!{Guid.NewGuid():N}"[..14];
    if (app.Configuration["DuLieuMau:MatKhau"] is not { Length: > 0 }) app.Logger.LogWarning("DuLieuMau:MatKhau chưa đặt; mật khẩu tài khoản mẫu lần này: {MatKhau}", DbSeeder.MatKhauMau);
    try { await DbSeeder.SeedAsync(db, giaoDich: app.Configuration.GetValue("DuLieuMau:GiaoDich", true), thietKe: app.Configuration.GetValue("DuLieuMau:ThietKe", true), anh: scope.ServiceProvider.GetRequiredService<AnhService>()); }
    catch (Exception e) { app.Logger.LogWarning(e, "Không tạo được dữ liệu mẫu; ứng dụng vẫn chạy. Xóa CSDL rồi chạy lại để tạo dữ liệu mẫu mới."); }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// Lỗi ngoài endpoint filter (kiểm cookie chạm CSDL, bind tham số, serialize) dưới /api vẫn trả JSON 500, không trả trang HTML.
// Body sai định dạng (BadHttpRequestException) giữ mã 400 của nó.
app.UseWhen(c => c.Request.Path.StartsWithSegments("/api"), b => b.UseExceptionHandler(e => e.Run(c =>
    (c.Features.Get<IExceptionHandlerFeature>()?.Error is BadHttpRequestException bad
        ? Results.Problem(statusCode: bad.StatusCode, title: "Dữ liệu gửi lên không hợp lệ (sai định dạng JSON hoặc giá trị).")
        : Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Lỗi hệ thống. Vui lòng thử lại hoặc báo quản trị viên."))
        .ExecuteAsync(c))));
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// FallbackPolicy bắt đăng nhập mọi endpoint; CSS/JS phải cho ẩn danh để trang đăng nhập hiển thị đúng.
app.MapStaticAssets().AllowAnonymous();
app.MapRazorPages().WithStaticAssets();
app.MapFriendshipApi();

app.MapGet("/Dat", () => Results.Redirect("/", permanent: false)).AllowAnonymous();
app.MapGet("/Dat/Index", () => Results.Redirect("/", permanent: false)).AllowAnonymous();

app.Run();

public partial class Program { }
