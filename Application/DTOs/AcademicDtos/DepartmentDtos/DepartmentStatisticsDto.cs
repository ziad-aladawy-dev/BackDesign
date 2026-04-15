namespace HUP.Application.DTOs.AcademicDtos.DepartmentDtos
{
    public class DepartmentStatisticsDto
    {
        public int TotalDepartments { get; set; }
        public int ActiveDepartments { get; set; }
        public int InactiveDepartments { get; set; }
        public int DepartmentsWithHead { get; set; }
        public int DepartmentsWithoutHead { get; set; }
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public double AverageStudentsPerDepartment { get; set; }
        public double AverageInstructorsPerDepartment { get; set; }
        public Dictionary<string, int> TopDepartmentsByStudents { get; set; } = new();
        public Dictionary<string, int> TopDepartmentsByInstructors { get; set; } = new();
        public Dictionary<string, int> DepartmentsByFaculty { get; set; } = new(); 
        public DateTime GeneratedAt { get; set; }
    }
}
