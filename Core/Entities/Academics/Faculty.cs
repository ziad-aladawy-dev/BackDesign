using HUP.Core.Entities.Identity;
using HUP.Core.Entities.Shared;
using HUP.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HUP.Core.Entities.Academics
{
    public class Faculty : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;
        //[Required, MaxLength(200)]
        //public string NameAr { get; set; } = string.Empty;

        //[Required, MaxLength(200)]
        //public string NameEn { get; set; } = string.Empty;

        public FacultyTitle Name { get; set; }
        public string DisplayName { get; set; }
        public Guid? DeanId { get; set; } // <<======
        //public string DeanName { get; set; } // xx?
        public string? ContactInfo { get; set; }

        public User? Dean { get; set; }
        //public ICollection<Department> Departments { get; set; }
        public ICollection<DepartmentFaculty> DepartmentFaculties { get; set; } = new List<DepartmentFaculty>();
        public ICollection<Student>? Students { get; set; }
    }
}