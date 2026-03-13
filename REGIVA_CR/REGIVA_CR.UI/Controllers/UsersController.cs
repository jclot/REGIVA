using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REGIVA_CR.AB.LogicaDeNegocio.Auth;
using REGIVA_CR.AB.ModelosParaUI.Organization;
using System.Security.Claims;

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelInvite(string email)
    {
        int tenantId = int.Parse(User.FindFirst("TenantId")!.Value);

        try
        {
            await _accountLN.CancelInvitationAsync(tenantId, email);
            TempData["SuccessMessage"] = $"La invitación para {email} ha sido cancelada.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error al cancelar la invitación: " + ex.Message;
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int userId, string newRole)
    {
        if (!TryGetContext(out int tenantId, out int currentUserId, out string currentRole))
        {
            return RedirectToAction("Index", "Home");
        }

        if (!IsPrivileged(currentRole))
        {
            return Unauthorized();
        }

        TeamMemberDto? member = await _accountLN.GetTeamMemberAsync(tenantId, userId);
        if (member == null)
        {
            TempData["ErrorMessage"] = "No se encontró el usuario seleccionado.";
            return RedirectToAction("Index");
        }

        if (!CanManageMember(currentRole, currentUserId, member))
        {
            return Unauthorized();
        }

        string role = (newRole ?? string.Empty).Trim().ToLowerInvariant();
        if (!GetAssignableRoles(currentRole).Contains(role))
        {
            TempData["ErrorMessage"] = "Rol no permitido para tu nivel de permisos.";
            return RedirectToAction("Index");
        }

        await _accountLN.UpdateTeamMemberRoleAsync(tenantId, userId, role);

        string actorName = User.FindFirst("FullName")?.Value ?? "Administrador";
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _accountLN.LogActivityAsync(
            userId,
            tenantId,
            "Rol Actualizado",
            $"Rol cambiado a {role} por {actorName}.",
            ipAddress
        );

        TempData["SuccessMessage"] = $"Rol actualizado para {member.FullName}.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetMemberActive(int userId, bool isActive)
    {
        if (!TryGetContext(out int tenantId, out int currentUserId, out string currentRole))
        {
            return RedirectToAction("Index", "Home");
        }

        if (!IsPrivileged(currentRole))
        {
            return Unauthorized();
        }

        TeamMemberDto? member = await _accountLN.GetTeamMemberAsync(tenantId, userId);
        if (member == null)
        {
            TempData["ErrorMessage"] = "No se encontró el usuario seleccionado.";
            return RedirectToAction("Index");
        }

        if (!CanManageMember(currentRole, currentUserId, member))
        {
            return Unauthorized();
        }

        await _accountLN.SetTeamMemberActiveAsync(tenantId, userId, isActive);

        string actorName = User.FindFirst("FullName")?.Value ?? "Administrador";
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        string action = isActive ? "Acceso Reactivado" : "Acceso Suspendido";
        string description = isActive
            ? $"Acceso reactivado por {actorName}."
            : $"Acceso suspendido por {actorName}.";

        await _accountLN.LogActivityAsync(userId, tenantId, action, description, ipAddress);

        TempData["SuccessMessage"] = isActive
            ? $"Acceso reactivado para {member.FullName}."
            : $"Acceso suspendido para {member.FullName}.";

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int userId)
    {
        if (!TryGetContext(out int tenantId, out int currentUserId, out string currentRole))
        {
            return RedirectToAction("Index", "Home");
        }

        if (!IsPrivileged(currentRole))
        {
            return Unauthorized();
        }

        TeamMemberDto? member = await _accountLN.GetTeamMemberAsync(tenantId, userId);
        if (member == null)
        {
            TempData["ErrorMessage"] = "No se encontró el usuario seleccionado.";
            return RedirectToAction("Index");
        }

        if (!CanManageMember(currentRole, currentUserId, member))
        {
            return Unauthorized();
        }

        string memberRole = (member.Role ?? "user").ToLowerInvariant();
        if (memberRole == "owner")
        {
            TempData["ErrorMessage"] = "No puedes eliminar a un usuario con rol dueño.";
            return RedirectToAction("Index");
        }

        await _accountLN.RemoveTeamMemberAsync(tenantId, userId);

        string actorName = User.FindFirst("FullName")?.Value ?? "Administrador";
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _accountLN.LogActivityAsync(
            userId,
            tenantId,
            "Eliminado del Equipo",
            $"El usuario fue removido del equipo por {actorName}.",
            ipAddress
        );

        TempData["SuccessMessage"] = $"Usuario {member.FullName} eliminado del equipo.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> GetActivityLog(int userId)
    {
        if (!TryGetContext(out int tenantId, out int currentUserId, out string currentRole))
        {
            return Unauthorized();
        }

        if (!IsPrivileged(currentRole))
        {
            return Unauthorized();
        }

        TeamMemberDto? member = await _accountLN.GetTeamMemberAsync(tenantId, userId);
        if (member == null)
        {
            return NotFound();
        }

        if (!CanManageMember(currentRole, currentUserId, member))
        {
            return Forbid();
        }

        List<REGIVA_CR.AB.ModelosParaUI.Auth.UserActivityDto> logs =
            await _accountLN.GetUserActivityLogsAsync(userId, 200);

        return Json(logs);
    }

    private static bool IsPrivileged(string role) =>
        role == "owner" || role == "admin";

    private static HashSet<string> GetAssignableRoles(string role)
    {
        if (role == "owner")
        {
            return new HashSet<string> { "admin", "accountant", "user" };
        }

        if (role == "admin")
        {
            return new HashSet<string> { "accountant", "user" };
        }

        return new HashSet<string>();
    }

    private static bool CanManageMember(string currentRole, int currentUserId, TeamMemberDto member)
    {
        if (member.UserId == currentUserId) return false;

        string memberRole = (member.Role ?? "user").ToLowerInvariant();

        if (currentRole == "owner")
        {
            return memberRole != "owner";
        }

        if (currentRole == "admin")
        {
            return memberRole != "owner" && memberRole != "admin";
        }

        return false;
    }

    private bool TryGetContext(out int tenantId, out int currentUserId, out string currentRole)
    {
        currentRole = (User.FindFirst(ClaimTypes.Role)?.Value ?? "user").ToLowerInvariant();

        bool tenantOk = int.TryParse(User.FindFirst("TenantId")?.Value, out tenantId);
        bool userOk = int.TryParse(User.FindFirst("UserId")?.Value, out currentUserId);

        return tenantOk && userOk;
    }

}
