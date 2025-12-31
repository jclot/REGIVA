using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REGIVA_CR.AB.ModelosParaUI.Auth;

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
    }
}

