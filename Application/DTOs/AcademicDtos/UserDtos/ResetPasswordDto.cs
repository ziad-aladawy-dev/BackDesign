using System.ComponentModel.DataAnnotations;

namespace HUP.Application.DTOs.AcademicDtos.UserDtos
{
    public class ResetPasswordDto
    {
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters if provided")]
        public string? NewPassword { get; set; }

        public bool ForcePasswordChange { get; set; } = true;
        public string? Reason { get; set; }
    }
}
