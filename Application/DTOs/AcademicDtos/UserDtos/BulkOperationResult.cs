using HUP.Core.Models;

namespace HUP.Application.DTOs.AcademicDtos.UserDtos
{
    public class BulkOperationResult
    {
        public bool Success { get; set; }
        public int ProcessedCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public string Message { get; set; }
        public List<BulkOperationItem> Items { get; set; } = new();
    }
}
