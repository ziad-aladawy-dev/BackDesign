using HUP.Core.Enums.Financial;

namespace HUP.Application.DTOs.FinancialDtos
{
    public class StudentFeeDto
    {
        public Guid Id { get; set; }
        public string FeeName { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public string SemesterName { get; set; }
        public FeeType Type { get; set; }
    }

    public class FinancialSummaryDto
    {
        public decimal TotalBilled { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
        public IEnumerable<StudentFeeDto> Fees { get; set; }
    }
}
