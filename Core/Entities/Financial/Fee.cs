using HUP.Core.Entities.Shared;
using HUP.Core.Entities.Academics;
using HUP.Core.Enums.Financial;

namespace HUP.Core.Entities.Financial
{
    public class Fee : BaseEntity
    {
        public string Name { get; set; } // e.g. "Tuition Fall 2024", "Bus Fee"
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool IsPerCredit { get; set; } // If true, Amount is per credit hour
        public Guid DepartmentId { get; set; }
        public FeeType Type { get; set; }
        public Guid SemesterId { get; set; }
        public Department Department { get; set; }
        public Semester Semester { get; set; }
    }
}
