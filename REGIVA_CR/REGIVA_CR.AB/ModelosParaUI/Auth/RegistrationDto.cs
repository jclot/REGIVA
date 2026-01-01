using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REGIVA_CR.AB.Attributes;

namespace REGIVA_CR.AB.ModelosParaUI.Auth
{
    public class UserRegisterDto
    {
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres.")]
        public string? FirstName { get; set; }

        [Display(Name = "Apellidos")]
        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(50, ErrorMessage = "Los apellidos no pueden exceder 50 caracteres.")]
        public string? LastName { get; set; }

        [Display(Name = "Correo Electrónico")]
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string? Email { get; set; }

        [Display(Name = "Teléfono Celular")]
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        // Regex: Exactamente 8 dígitos numéricos
        [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos numéricos (Ej: 88888888).")]
        public string? Phone { get; set; }

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        // Regex: Al menos 1 mayúscula, 1 minúscula, 1 número
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "La contraseña debe tener al menos una mayúscula, una minúscula y un número.")]
        public string? Password { get; set; }

        [Display(Name = "Confirmar Contraseña")]
        [Required(ErrorMessage = "Tienes que confirmar la contraseña primero.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Términos y Condiciones")]
        [MustBeTrue(ErrorMessage = "Debes aceptar los términos y condiciones para continuar.")]
        public bool TermsAccepted { get; set; }
    }

    public class TenantRegisterDto
    {
        [Display(Name = "Razón Social")]
        [Required(ErrorMessage = "El nombre del negocio es obligatorio.")]
        [StringLength(100)]
        public string? BusinessName { get; set; }

        [Display(Name = "Cédula Jurídica")]
        [Required(ErrorMessage = "La cédula es obligatoria.")]
        // Regex: Entre 9 y 12 dígitos (cubre física, jurídica, dimex)
        [RegularExpression(@"^[0-9]{9,12}$", ErrorMessage = "La cédula debe tener entre 9 y 12 dígitos sin guiones.")]
        public string? LegalId { get; set; }

        [Display(Name = "Teléfono Empresa")]
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos numéricos.")]
        public string? Phone { get; set; }

        [Display(Name = "Cód. Actividad")]
        [Required(ErrorMessage = "El código de actividad es obligatorio.")]
        // Regex: Hacienda suele usar 6 dígitos para códigos CAE
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "El código debe ser de 6 dígitos numéricos.")]
        public string? EconomicActivityCode { get; set; }

        public string SubscriptionPlan { get; set; } = "basic";
    }

    public class FullRegistrationDto
    {
        public UserRegisterDto? User { get; set; }
        public TenantRegisterDto? Tenant { get; set; }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Ingrese su correo.")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Ingrese su contraseña.")]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class UserSessionDto
    {
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public string? BusinessName { get; set; }
    }

    public class ForgotPasswordDto
    {
        [Display(Name = "Correo Electrónico")]
        [Required(ErrorMessage = "Por favor ingresa tu correo.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        public string? Email { get; set; }
    }

    public class UserSecurityDto
    {
        public int UserId { get; set; }
        public string? PasswordHash { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
    }

    public class UserRecoveryDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Nueva Contraseña")]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mínimo 8 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Debe tener mayúscula, minúscula y número.")]
        public string? Password { get; set; }

        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string? ConfirmPassword { get; set; }
        public DateTime TokenExpiration { get; set; }
    }

    public class VerifyEmailDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Código de Verificación")]
        [Required(ErrorMessage = "Ingresa el código de 6 dígitos.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe ser de 6 dígitos.")]
        public string Code { get; set; } = string.Empty;
    }
}
