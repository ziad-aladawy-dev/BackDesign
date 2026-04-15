namespace HUP.Application.DTOs.AcademicDtos.Student;

public class StudentStatusDto
{
    public Guid StudentId { get; set; }
    public string AcademicStatus { get; set; }
    public int Level { get; set; }
    public decimal Cgpa { get; set; }
    public string Group { get; set; }
}