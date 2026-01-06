using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REGIVA_CR.AB.ModelosParaUI.Auth;

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
    }
}
