using HUP.Application.DTOs.FinancialDtos;

namespace HUP.Application.Services.Interfaces
{
    public interface IFinancialService
    {
        Task<FinancialSummaryDto> GetStudentFinancialSummaryAsync(Guid studentId, string lang);
    }
}
