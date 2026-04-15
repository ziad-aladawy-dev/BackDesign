using HUP.Core.Entities.Shared;
using HUP.Core.Enums;

namespace HUP.Core.Entities.Academics
{
    public class Course : BaseEntity
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credits { get; set; }
        public Guid? PrerequisiteId { get; set; }
        public CourseType CourseType { get; set; }

        public Course Prerequisite { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<CourseOffering> CourseOfferings { get; set; }
        public ICollection<ProgramPlan> Programs { get; set; }

    }
}