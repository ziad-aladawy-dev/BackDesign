using HUP.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HUP.Application.DTOs.AcademicDtos.FacultyDtos
{
    public class CreateFacultyDto
    {
        [Required(ErrorMessage = "Faculty code is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Code must be between 2 and 50 characters")]
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; }

        public string DisplayName { get; set; }

        //[Required(ErrorMessage = "Arabic name is required")]
        //[StringLength(200, MinimumLength = 3, ErrorMessage = "Arabic name must be between 3 and 200 characters")]
        //public string NameAr { get; set; } = string.Empty;

        //[Required(ErrorMessage = "English name is required")]
        //[StringLength(200, MinimumLength = 3, ErrorMessage = "English name must be between 3 and 200 characters")]
        //public string NameEn { get; set; } = string.Empty;

        public Guid? DeanId { get; set; }

        [StringLength(500)]
        public string? ContactInfo { get; set; }
    }
}
