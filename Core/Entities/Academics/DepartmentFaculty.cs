namespace HUP.Core.Entities.Academics
{
    public class DepartmentFaculty
    {
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; }

        public Guid FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        public string DepartmentCode { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
