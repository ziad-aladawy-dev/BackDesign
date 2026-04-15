namespace HUP.Application.DTOs.AcademicDtos.UserDtos
{
    public class BulkOperationItem
    {
        public Guid UserId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
    }
}
