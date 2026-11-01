using Friendship.Infrastructure.Services;
using Friendship.Web.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Friendship.Web.Components;

public class CanhBaoBadgeViewComponent(CanhBaoService canhBao) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
        => View(await canhBao.DemAsync(UserClaimsPrincipal.VaiTroHienTai()));
}
