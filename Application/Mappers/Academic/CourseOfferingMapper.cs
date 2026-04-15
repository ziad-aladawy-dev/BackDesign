using HUP.Application.DTOs.AcademicDtos.CourseOffering;
using HUP.Common.Helpers;
using Riok.Mapperly.Abstractions;
using HUP.Core.Entities.Academics;

namespace HUP.Application.Mappers.Academic
{
    [Mapper(AllowNullPropertyAssignment = false)]
    public static partial class CourseOfferingMapper
    {
        // Map nested Course properties to CourseOfferingDto
        public static CourseOfferingDto ToDto(CourseOffering courseOffering, string lang)
        {
            var dto = new CourseOfferingDto();
            dto.Id = courseOffering.Id;
            dto.CourseCode = LocalizationHelper.Get<string>(courseOffering.Course.CourseCode, lang);
            dto.CourseName = LocalizationHelper.Get<string>(courseOffering.Course.CourseName, lang);
            dto.Credits = courseOffering.Course.Credits;
            return dto;
        }
        // Map CreateCourseOfferingDto to CourseOffering entity
        public static partial CourseOffering ToEntity(CreateCourseOfferingDto dto);
    }
}
