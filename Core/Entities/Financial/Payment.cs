using HUP.Core.Entities.Shared;
using HUP.Core.Enums.Financial;

namespace HUP.Core.Entities.Financial
{
    public class Payment : BaseEntity
    {
        public Guid StudentFeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod Method { get; set; } // Online, Cash, BankTransfer
        public string ReferenceNumber { get; set; } // Transaction ID

        public StudentFee StudentFee { get; set; }
    }
}
