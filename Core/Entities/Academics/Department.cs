using HUP.Core.Entities.Financial;
using HUP.Core.Entities.Identity;
using HUP.Core.Entities.Shared;

namespace HUP.Core.Entities.Academics
{
    public class Department : BaseEntity
    {
        public string DepartmentName { get; set; }
        //public string DepartmentCode { get; set; }
        //public Guid FacultyId { get; set; }
        public Guid? HeadOfDepartmentId { get; set; }
        public string BaseDepartmentCode { get; set; }
        //public int DurationInYears { get; set; }
        public int CompulsoryHours { get; set; }
        public int ElectiveHours { get; set; }

        public Staff? HeadOfDepartment;
        public ICollection<DepartmentFaculty> DepartmentFaculties { get; set; } = new List<DepartmentFaculty>();
        public ICollection<Fee> Fees { get; set; }
        public ICollection<CourseOffering> CourseOfferings { get; set; }
        //public Faculty Faculty { get; set; }
        public ICollection<Staff> StaffMembers { get; set; }
        public ICollection<ProgramPlan> Programs { get; set; }
    }
}