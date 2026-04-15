using HUP.Application.DTOs.FinancialDtos;
using HUP.Application.Mappers.Financial;
using HUP.Application.Services.Interfaces;
using HUP.Repositories.Interfaces;

namespace HUP.Application.Services.Implementations
{
    public class FinancialService : IFinancialService
    {
        private readonly IFinancialRepository _financialRepo;

        public FinancialService(IFinancialRepository financialRepo)
        {
            _financialRepo = financialRepo;
        }

        public async Task<FinancialSummaryDto> GetStudentFinancialSummaryAsync(Guid studentId, string lang)
        {
            var fees = await _financialRepo.GetStudentFeesAsync(studentId);

            var feeDtos = fees.Select(f => FinancialMapper.ToDto(f, lang)).ToList();

            var totalBilled = feeDtos.Sum(f => f.Amount);
            var totalPaid = feeDtos.Sum(f => f.PaidAmount);
            var totalOutstanding = feeDtos.Sum(f => f.RemainingAmount);

            return new FinancialSummaryDto
            {
                TotalBilled = totalBilled,
                TotalPaid = totalPaid,
                TotalOutstanding = totalOutstanding,
                Fees = feeDtos
            };
        }
    }
}
