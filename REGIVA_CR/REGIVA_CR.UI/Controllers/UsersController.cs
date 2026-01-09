using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REGIVA_CR.AB.LogicaDeNegocio.Auth;
using REGIVA_CR.AB.ModelosParaUI.Organization;

namespace REGIVA_CR.UI.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly IAccountLN _accountLN;

    public UsersController(IAccountLN accountLN)
    {
        _accountLN = accountLN;
    }

    public async Task<IActionResult> Index()
    {
        string? tenantIdStr = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdStr)) return RedirectToAction("Index", "Home");

        int tenantId = int.Parse(tenantIdStr);

        OrganizationViewModel model = await _accountLN.GetOrganizationDataAsync(tenantId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite(OrganizationViewModel model)
    {
        int tenantId = int.Parse(User.FindFirst("TenantId")!.Value);

        try
        {
            if (string.IsNullOrEmpty(model.NewInvite.Email))
                throw new Exception("El correo es obligatorio.");

            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            string urlFormat = $"{baseUrl}/Account/AcceptInvite?token={{0}}";
            await _accountLN.InviteUserAsync(tenantId, model.NewInvite, urlFormat);

            TempData["SuccessMessage"] = $"Invitación enviada correctamente a {model.NewInvite.Email}";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction("Index");
    }
}