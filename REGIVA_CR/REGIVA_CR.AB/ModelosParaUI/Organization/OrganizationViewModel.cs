using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REGIVA_CR.AB.ModelosParaUI.Organization
{
    public class OrganizationViewModel
    {
        public int TenantId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public List<TeamMemberDto> Members { get; set; } = new();
        public List<PendingInviteViewDto> PendingInvites { get; set; } = new();
        public CreateInviteDto NewInvite { get; set; } = new();
    }

    public class TeamMemberDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class PendingInviteViewDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }

    public class CreateInviteDto
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "user";
    }
    public class InvitationDto
    {
        public int TenantId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
