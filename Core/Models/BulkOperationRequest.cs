namespace HUP.Core.Models
{
    public class BulkOperationRequest
    {
        public List<Guid> Ids { get; set; } = new();
        public string Operation { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
}
