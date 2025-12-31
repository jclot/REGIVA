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
    }
}
