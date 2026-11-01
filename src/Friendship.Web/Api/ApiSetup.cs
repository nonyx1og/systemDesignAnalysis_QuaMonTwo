using System.Collections;
using System.Text.Json.Serialization;
using Friendship.Domain;
using Microsoft.AspNetCore.Antiforgery;

namespace Friendship.Web.Api;

public static class ApiSetup
{
    public const string HeaderCsrf = "X-CSRF-TOKEN";

    public static void AddFriendshipApi(this IServiceCollection s)
    {
        // Enum chỉ nhận tên: số (kể cả ngoài phạm vi, ví dụ vai trò 99) bị từ chối thay vì được lưu.
        s.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
        s.AddAntiforgery(o => o.HeaderName = HeaderCsrf);
    }

    public static void MapFriendshipApi(this WebApplication app)
    {
        var api = app.MapGroup("/api").AddEndpointFilter(BaoVeAsync);
        api.MapAuthApi();
        api.MapDanhMucApi();
        api.MapBanHangApi();
        api.MapBepApi();
        api.MapKhoApi();
        api.MapHuyBaoCaoApi();
        api.MapTaiKhoanApi();
        api.MapDatApi();
        app.MapDatAnhApi();
    }

    /// <summary>Yêu cầu ghi phải có token chống giả mạo (cookie xác thực tự đi kèm nên phải chặn CSRF);
    /// lỗi nghiệp vụ thành 400 (không tìm thấy thành 404), lỗi khác thành 500 — đều là ProblemDetails tiếng Việt.</summary>
    internal static async ValueTask<object?> BaoVeAsync(EndpointFilterInvocationContext c, EndpointFilterDelegate next)
    {
        var http = c.HttpContext;
        if (!HttpMethods.IsGet(http.Request.Method))
        {
            try { await http.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(http); }
            catch (AntiforgeryValidationException)
            {
                return Results.Problem(statusCode: StatusCodes.Status400BadRequest,
                    title: $"Thiếu hoặc sai mã chống giả mạo. Gọi GET /api/auth/csrf rồi gửi token trong header {HeaderCsrf}.");
            }
        }
        if (c.Arguments.Any(CoPhanTuRong))
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Danh sách gửi lên có phần tử rỗng (null).");
        try { return await next(c); }
        catch (NghiepVuException e)
        {
            // 404 chỉ khi tài nguyên trên URL ({id}) không tồn tại; id sai trong body là lỗi dữ liệu gửi lên → 400.
            var ma = http.Request.RouteValues.ContainsKey("id") && e.Message.StartsWith("Không tìm thấy")
                ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            return Results.Problem(statusCode: ma, title: e.Message);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // Lỗi không lường trước: ghi log, trả JSON (không để trang HTML /Error hay chi tiết lỗi lọt ra client API).
            http.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Friendship.Api").LogError(e, "Lỗi API {Path}", http.Request.Path);
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Lỗi hệ thống. Vui lòng thử lại hoặc báo quản trị viên.");
        }
    }

    /// <summary>Body là danh sách (hoặc DTO của API chứa danh sách) có phần tử null: service không kiểm nên chặn ở biên.</summary>
    static bool CoPhanTuRong(object? arg) => arg switch
    {
        null or string => false,
        IEnumerable ds => ds.Cast<object?>().Any(x => x is null),
        _ when arg.GetType().Namespace == typeof(ApiSetup).Namespace =>
            arg.GetType().GetProperties().Any(p => p.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(p.PropertyType)
                && p.GetValue(arg) is IEnumerable ds && ds.Cast<object?>().Any(x => x is null)),
        _ => false
    };
}
