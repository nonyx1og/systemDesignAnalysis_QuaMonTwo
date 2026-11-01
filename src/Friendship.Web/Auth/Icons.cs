using Microsoft.AspNetCore.Html;

namespace Friendship.Web.Auth;

// Bộ icon nét 24×24 dùng chung (menu, thanh trên, trang tổng quan). Vẽ tay theo cùng một độ dày nét.
public static class Icons
{
    static readonly Dictionary<string, string> Paths = new()
    {
        ["home"] = "<path d='M3 10.5 12 3l9 7.5'/><path d='M5 9.5V20h5v-6h4v6h5V9.5'/>",
        ["cart"] = "<circle cx='9' cy='20' r='1.25'/><circle cx='18' cy='20' r='1.25'/><path d='M2.5 3.5h2.6l2.3 11.2a1.5 1.5 0 0 0 1.5 1.2h8.6a1.5 1.5 0 0 0 1.5-1.1L21 7.5H6'/>",
        ["receipt"] = "<path d='M6 2.5h12v19l-3-2-3 2-3-2-3 2z'/><path d='M9 8h6M9 12h6M9 16h3'/>",
        ["file-plus"] = "<path d='M14 2.5H6.5A1.5 1.5 0 0 0 5 4v16a1.5 1.5 0 0 0 1.5 1.5h11A1.5 1.5 0 0 0 19 20V7.5z'/><path d='M14 2.5v5h5M12 11v6M9 14h6'/>",
        ["qr"] = "<rect x='3.5' y='3.5' width='6.5' height='6.5' rx='1'/><rect x='14' y='3.5' width='6.5' height='6.5' rx='1'/><rect x='3.5' y='14' width='6.5' height='6.5' rx='1'/><path d='M14 14h2.5v2.5H14zM18 18h2.5v2.5H18zM18 14h2.5M14 18v2.5'/>",
        ["wallet"] = "<path d='M19 7.5V5.5A1.5 1.5 0 0 0 17.5 4h-12A2.5 2.5 0 0 0 3 6.5v11A2.5 2.5 0 0 0 5.5 20h13a1.5 1.5 0 0 0 1.5-1.5v-9A1.5 1.5 0 0 0 18.5 8H5.5A2.5 2.5 0 0 1 3 6.5'/><circle cx='16.5' cy='14' r='1.1'/>",
        ["pot"] = "<path d='M4 10h16v6.5A3.5 3.5 0 0 1 16.5 20h-9A3.5 3.5 0 0 1 4 16.5z'/><path d='M2 10h20M9 6.5c0-1 .8-1.5.8-2.5M14 6.5c0-1 .8-1.5.8-2.5'/>",
        ["palette"] = "<path d='M12 3a9 9 0 1 0 0 18c1.1 0 1.8-.8 1.8-1.7 0-.5-.2-.9-.5-1.2-.3-.4-.5-.8-.5-1.3 0-1 .8-1.8 1.8-1.8h2.1A4.3 4.3 0 0 0 21 10.7C21 6.4 17 3 12 3z'/><circle cx='7.5' cy='11' r='1'/><circle cx='10.5' cy='7' r='1'/><circle cx='15.5' cy='7.5' r='1'/>",
        ["layers"] = "<path d='m12 3 9 5-9 5-9-5z'/><path d='m3 13 9 5 9-5'/>",
        ["arrow-down"] = "<path d='M12 3v12M7 10.5l5 5 5-5'/><path d='M4 20.5h16'/>",
        ["arrow-up"] = "<path d='M12 16V4M7 8.5l5-5 5 5'/><path d='M4 20.5h16'/>",
        ["clipboard"] = "<rect x='5' y='4' width='14' height='17.5' rx='1.5'/><path d='M9 4V2.5h6V4M8.5 12l2.2 2.2 4.8-4.7'/>",
        ["list"] = "<path d='M9 6h11M9 12h11M9 18h11'/><circle cx='4.5' cy='6' r='.9'/><circle cx='4.5' cy='12' r='.9'/><circle cx='4.5' cy='18' r='.9'/>",
        ["alert"] = "<path d='M10.3 4 2.6 17.5A2 2 0 0 0 4.3 20.5h15.4a2 2 0 0 0 1.7-3L13.7 4a2 2 0 0 0-3.4 0z'/><path d='M12 9.5v4M12 17h.01'/>",
        ["trash"] = "<path d='M4 6.5h16M9.5 6.5V4h5v2.5M6 6.5l1 13.5a1.5 1.5 0 0 0 1.5 1.5h7a1.5 1.5 0 0 0 1.5-1.5l1-13.5M10 11v6M14 11v6'/>",
        ["package"] = "<path d='m12 2.5 8.5 4.5v10L12 21.5 3.5 17V7z'/><path d='m3.5 7 8.5 4.5L20.5 7M12 11.5v10M7.8 4.8l8.4 4.5'/>",
        ["cake"] = "<path d='M4 21h16v-7.5a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2z'/><path d='M4 16c1.3 0 1.3 1 2.7 1s1.3-1 2.6-1 1.4 1 2.7 1 1.3-1 2.7-1 1.3 1 2.6 1 1.4-1 2.7-1M12 11.5V8M12 5.5c-.8-.8-.8-1.7 0-2.5.8.8.8 1.7 0 2.5z'/>",
        ["wheat"] = "<path d='M12 21.5V3.5M12 8 8.5 5M12 8l3.5-3M12 12.5l-3.5-3M12 12.5l3.5-3M12 17l-3.5-3M12 17l3.5-3'/>",
        ["tag"] = "<path d='M3 12.2V4.5A1.5 1.5 0 0 1 4.5 3h7.7l9 9a1.5 1.5 0 0 1 0 2.1l-7.1 7.1a1.5 1.5 0 0 1-2.1 0z'/><circle cx='8' cy='8' r='1.4'/>",
        ["percent"] = "<path d='M19 5 5 19'/><circle cx='7' cy='7' r='2.3'/><circle cx='17' cy='17' r='2.3'/>",
        ["check"] = "<circle cx='12' cy='12' r='9'/><path d='m8 12.3 2.7 2.7L16.2 9.5'/>",
        ["check-file"] = "<path d='M14 2.5H6.5A1.5 1.5 0 0 0 5 4v16a1.5 1.5 0 0 0 1.5 1.5h11A1.5 1.5 0 0 0 19 20V7.5z'/><path d='M14 2.5v5h5M9 14.5l2 2 4-4'/>",
        ["scan"] = "<path d='M3 8V5a2 2 0 0 1 2-2h3M16 3h3a2 2 0 0 1 2 2v3M21 16v3a2 2 0 0 1-2 2h-3M8 21H5a2 2 0 0 1-2-2v-3M7 12h10'/>",
        ["chart"] = "<path d='M3.5 3.5v17h17'/><path d='M8 16.5v-5M12.5 16.5v-9M17 16.5v-3'/>",
        ["trend"] = "<path d='m3 17 6-6 4 4 8-8'/><path d='M15 7h6v6'/>",
        ["boxes"] = "<rect x='3' y='12' width='8' height='8.5' rx='1'/><rect x='13' y='12' width='8' height='8.5' rx='1'/><rect x='8' y='3' width='8' height='8.5' rx='1'/>",
        ["users"] = "<circle cx='9' cy='8' r='3.5'/><path d='M2.5 20.5a6.5 6.5 0 0 1 13 0M16 4.7a3.5 3.5 0 0 1 0 6.6M18 14.3a6.5 6.5 0 0 1 3.5 6.2'/>",
        ["search"] = "<circle cx='11' cy='11' r='7'/><path d='m20.5 20.5-4.5-4.5'/>",
        ["logout"] = "<path d='M9.5 21H5.5A1.5 1.5 0 0 1 4 19.5v-15A1.5 1.5 0 0 1 5.5 3h4M16 17l5-5-5-5M21 12H9.5'/>",
        ["key"] = "<circle cx='8' cy='15' r='4.5'/><path d='m11.2 11.8 9.3-9.3M17 6l3 3M14.5 8.5l2 2'/>",
        ["menu"] = "<path d='M4 6.5h16M4 12h16M4 17.5h16'/>",
        ["chevron"] = "<path d='m9 6 6 6-6 6'/>",
        ["bell"] = "<path d='M6 9a6 6 0 0 1 12 0c0 6.5 2.5 8 2.5 8h-17S6 15.5 6 9zM10 20.5a2.2 2.2 0 0 0 4 0'/>",
    };

    public static IHtmlContent Svg(string ten, int size = 20, string? css = null) =>
        new HtmlString($"<svg class='ico{(css is null ? "" : " " + css)}' width='{size}' height='{size}' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='1.75' stroke-linecap='round' stroke-linejoin='round' aria-hidden='true' focusable='false'>{Paths.GetValueOrDefault(ten, Paths["home"])}</svg>");

    // Logo nongnongBakery: ô vuông bo góc màu hồng, hình bánh kem nhiều tầng nét trắng.
    public static IHtmlContent Logo(int size = 28) =>
        new HtmlString($"<svg class='logo-mark' width='{size}' height='{size}' viewBox='0 0 32 32' aria-hidden='true' focusable='false'><rect width='32' height='32' rx='8' fill='#E0457B'/><g fill='none' stroke='#fff' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'><path d='M8 24.5h16v-5a1.5 1.5 0 0 0-1.5-1.5h-13A1.5 1.5 0 0 0 8 19.5z'/><path d='M10.5 18v-3a1.5 1.5 0 0 1 1.5-1.5h8a1.5 1.5 0 0 1 1.5 1.5v3'/><path d='M16 13.5V11'/><path d='M16 8.6c-.7-.7-.7-1.5 0-2.2.7.7.7 1.5 0 2.2z' fill='#fff'/></g></svg>");
}
