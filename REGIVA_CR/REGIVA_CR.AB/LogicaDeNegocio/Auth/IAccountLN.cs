using REGIVA_CR.AB.ModelosParaUI.Auth;
using REGIVA_CR.AB.ModelosParaUI.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REGIVA_CR.AB.LogicaDeNegocio.Auth
{
    public interface IAccountLN
    {
        Task RegisterAsync(FullRegistrationDto data);
        Task<UserSessionDto?> LoginAsync(LoginDto data);
        Task ValidateUserAvailabilityAsync(string email, string phone);
        Task ValidateTenantAvailabilityAsync(string legalId);

        Task SendPasswordResetAsync(string email, string resetLinkBaseUrl);
        Task<bool> ResetPasswordAsync(string token, string email, string newPassword);
        Task<UserRecoveryDto?> GetUserForResetAsync(string token);

        Task SendVerificationEmailAsync(string email, string verificationLinkBaseUrl);
        Task<bool> ConfirmEmailAsync(string email, string code);
        Task<bool> IsEmailVerifiedAsync(string email);

        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task UpdateProfileAsync(UpdateProfileDto model);
        Task<UpdateProfileDto?> GetUserForEditAsync(int userId);

        Task LogActivityAsync(int userId, int? tenantId, string type, string description, string? ipAddress);
        Task<List<UserActivityDto>> GetUserActivityLogsAsync(int userId, int v);

        Task DeleteAccountAsync(int userId, string password);

        Task<OrganizationViewModel> GetOrganizationDataAsync(int tenantId);
        Task InviteUserAsync(int tenantId, CreateInviteDto model, string inviteUrlFormat);
        Task<AcceptInviteDto> ValidateInviteTokenAsync(string token);
        Task CompleteInviteAsync(AcceptInviteDto model);
        Task CancelInvitationAsync(int tenantId, string email);
    }
}
