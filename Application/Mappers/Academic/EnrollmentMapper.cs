using HUP.Application.DTOs.AcademicDtos;
using HUP.Core.Entities.Academics;
using Riok.Mapperly.Abstractions;
using HUP.Application.DTOs.AcademicDtos.Enrollment;
using HUP.Common.Helpers;

namespace HUP.Application.Mappers.Academic
{
    [Mapper(AllowNullPropertyAssignment = false)]
    public static partial class EnrollmentMapper
    {
        // Mapping for CreateEnrollmentDto
        public static partial Enrollment ToEntityFromCreateDto(CreateEnrollmentDto dto);
        public static partial CreateEnrollmentDto ToCreateDto(Enrollment entity);

        // Mapping for EnrollmentResponseDto
        public static partial Enrollment ToEntityFromResponseDto(EnrollmentResponseDto dto);

        public static EnrollmentResponseDto ToResponseDto(Enrollment entity, string lang)
        {
            return new EnrollmentResponseDto
            {
                Id = entity.Id,
                StudentId = entity.StudentId,
                EnrollmentDate = entity.EnrollmentDate,
                Status = LocalizationHelper.Get(entity.Status, lang),
                Grade = entity.finalGrade,

                CourseCode = entity.CourseOffering?.Course == null
                    ? string.Empty
                    : LocalizationHelper.Get<string>(entity.CourseOffering.Course.CourseCode, lang),

                CourseName = entity.CourseOffering?.Course == null
                    ? string.Empty
                    : LocalizationHelper.Get<string>(entity.CourseOffering.Course.CourseName, lang),
            };
        }


        //Mapping for UpdateEnrollmentDto
        // status update
        public static partial Enrollment ToEntityFromUpdateDto(UpdateEnrollmentDto dto);
        public static partial void ToUpdate(UpdateEnrollmentDto dto, Enrollment entity);

        // grades update
        public static partial Enrollment ToEntityFromUpdateGradeDto(UpdateEnrollmentDto dto);

    }
}
