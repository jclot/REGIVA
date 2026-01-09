using REGIVA_CR.AB.ModelosParaUI.Auth;
using REGIVA_CR.AB.ModelosParaUI.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REGIVA_CR.AB.AccesoADatos.Auth
{
    public interface IAccountAD
    {
        Task<int> RegisterUserAndTenantAsync(FullRegistrationDto data, string passwordHash);
        Task<UserSessionDto?> GetUserByEmailAsync(string email);
        Task<string?> GetPasswordHashAsync(string email);

        Task<bool> UserExistsAsync(string email);
        Task<bool> PhoneExistsAsync(string phone);
        Task<bool> TenantExistsAsync(string legalId);

        Task<UserSecurityDto?> GetUserSecurityInfoAsync(string email);
        Task UpdateUserLoginStatsAsync(int userId, int failedAttempts, DateTime? lockoutEnd);

        Task SavePasswordResetTokenAsync(string email, string token);
        Task<UserRecoveryDto?> GetUserByResetTokenAsync(string token);
        Task UpdatePasswordAsync(int userId, string newPasswordHash);

        Task SetVerificationCodeAsync(string email, string code);
        Task<bool> VerifyEmailAsync(string email, string code);
        Task<bool> IsEmailVerifiedAsync(string email);

        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task UpdateUserProfileAsync(UpdateProfileDto model);
        Task<bool> ValidateCurrentPasswordAsync(int userId, string currentPassword);
        Task<UpdateProfileDto?> GetUserForEditAsync(int userId);

        Task LogActivityAsync(int userId, int? tenantId, string type, string description, string? ipAddress);

        Task SavePasswordToHistoryAsync(int userId, string passwordHash);
        Task<bool> IsPasswordInHistoryAsync(int userId, string newPassword, int limit = 4);

        Task<List<UserActivityDto>> GetActivityLogsAsync(int userId, int limit);

        Task SoftDeleteUserAsync(int userId);

        Task<OrganizationViewModel> GetOrganizationDetailsAsync(int tenantId);
        Task SaveInvitationAsync(InvitationDto inviteDto);
        Task<InvitationDto?> GetInvitationByTokenAsync(string token);
        Task<int> CreateUserFromInviteAsync(AcceptInviteDto model, string passwordHash);
        Task LinkUserToTenantAsync(int userId, int tenantId, string role);
        Task MarkInvitationAsAcceptedAsync(string token);
    }
}

