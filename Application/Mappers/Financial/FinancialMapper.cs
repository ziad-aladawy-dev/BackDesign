using HUP.Application.DTOs.FinancialDtos;
using HUP.Core.Entities.Financial;
using HUP.Common.Helpers;

namespace HUP.Application.Mappers.Financial
{
    public static class FinancialMapper
    {
        public static StudentFeeDto ToDto(StudentFee entity, string lang)
        {
            return new StudentFeeDto
            {
                Id = entity.Id,
                FeeName = LocalizationHelper.Get<string>(entity.Fee?.Name, lang) ?? "Unknown Fee",
                Description = LocalizationHelper.Get<string>(entity.Fee?.Description, lang),
                Amount = entity.Amount,
                PaidAmount = entity.PaidAmount,
                RemainingAmount = entity.Amount - entity.PaidAmount,
                Status = entity.Status.ToString(),
                DueDate = entity.DueDate,
                SemesterName = LocalizationHelper.Get<string>(entity.Fee?.Semester?.SemesterName, lang),
                Type = entity.Fee?.Type ?? Core.Enums.Financial.FeeType.Other
            };
        }
    }
}
