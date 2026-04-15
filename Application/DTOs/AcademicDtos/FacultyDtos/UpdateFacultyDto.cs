using HUP.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HUP.Application.DTOs.AcademicDtos.FacultyDtos
{
    public class UpdateFacultyDto
    {
        [StringLength(50, MinimumLength = 2)]
        public string? Code { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        //[StringLength(200, MinimumLength = 3)]
        //public string? NameAr { get; set; }

        //[StringLength(200, MinimumLength = 3)]
        //public string? NameEn { get; set; }

        public Guid? DeanId { get; set; }

        [StringLength(500)]
        public string? ContactInfo { get; set; }

        public bool? IsActive { get; set; }
    }
}
