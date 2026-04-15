namespace HUP.Application.DTOs.AcademicDtos.UserDtos
{
    public class BulkUserActionDto
    {
        public List<Guid> UserIds { get; set; }
        public string Action { get; set; } 
        public Guid? NewRoleId { get; set; }
        public string Reason { get; set; }
    }
}
