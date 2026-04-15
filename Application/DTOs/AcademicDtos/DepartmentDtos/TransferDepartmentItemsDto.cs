namespace HUP.Application.DTOs.AcademicDtos.DepartmentDtos
{
    public class TransferDepartmentItemsDto
    {
        public Guid SourceDepartmentId { get; set; }
        public Guid TargetDepartmentId { get; set; }
        public bool TransferStudents { get; set; }
        public bool TransferInstructors { get; set; }
        public string? Reason { get; set; }
    }
}
