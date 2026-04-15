using HUP.Application.DTOs.AcademicDtos;
using HUP.Common.Helpers;
using HUP.Core.Entities.Academics;
using Riok.Mapperly.Abstractions;

namespace HUP.Application.Mappers.Academic
{
    [Mapper(AllowNullPropertyAssignment = false)]
    public static partial class FacultyMapper
    {
        public static FacultyDto ToDto(Faculty entity, string lang)
        {
            var dto = new FacultyDto();
            dto.Id = entity.Id;
            // Faculty.Name is Enum, DisplayName is string (JSON)
            // Priority to DisplayName if exists, else Enum
            if (!string.IsNullOrEmpty(entity.DisplayName))
            {
                dto.Name = LocalizationHelper.Get<string>(entity.DisplayName, lang);
            }
            else
            {
                dto.Name = LocalizationHelper.Get(entity.Name, lang);
            }

            dto.DeanName = entity.Dean.FullName; // Assuming this is just a name, or maybe we need to lookup User.FullName?
            // Entity has `DeanName` string property, let's use it for now.
            // But usually DeanName might be JSON? Let's assume plain text or handled by generic fallback.
            // If it's a name like "Dr. Ahmed", it's likely not JSON.
            // If it's cached, we rely on what's there.

            dto.ContactInfo = entity.ContactInfo;
            return dto;
        }

        public static partial List<FacultyDto> ToDtoList(IEnumerable<Faculty> entities, string lang);
    }
}