using HUP.Core.Entities.Shared;
using HUP.Core.Entities.Academics;
using HUP.Core.Enums.Financial;

namespace HUP.Core.Entities.Financial
{
    public class StudentFee : BaseEntity
    {
        public Guid StudentId { get; set; }
        public Guid FeeId { get; set; }
        public decimal Amount { get; set; } // Final calculated amount (e.g. Rate * Credits)
        public decimal PaidAmount { get; set; }
        public FeeStatus Status { get; set; }
        public DateTime DueDate { get; set; }

        public Student Student { get; set; }
        public Fee Fee { get; set; }
        public ICollection<Payment> Payments { get; set; }
    }
}
