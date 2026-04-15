namespace HUP.Application.DTOs.AcademicDtos.FacultyDtos
{
    public class FacultyStatisticsDto
    {
        public int TotalFaculties { get; set; }
        public int ActiveFaculties { get; set; }
        public int InactiveFaculties { get; set; }
        public int FacultiesWithDean { get; set; }
        public int FacultiesWithoutDean { get; set; }

        public int TotalDepartments { get; set; }
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }

        public Dictionary<string, int> FacultiesByType { get; set; } = new();

        public Dictionary<string, int> TopFacultiesByStudents { get; set; } = new();
        public Dictionary<string, int> TopFacultiesByDepartments { get; set; } = new();

        public DateTime GeneratedAt { get; set; }
    }
}
