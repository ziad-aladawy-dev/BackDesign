using HUP.Application.DTOs.AcademicDtos.Student;
using HUP.Common.Helpers;
using HUP.Core.Entities.Academics;
using Riok.Mapperly.Abstractions;

namespace HUP.Application.Mappers.Identity
{
    [Mapper(AllowNullPropertyAssignment = false)]
    public static partial class StudentMapper
    {
        public static StudentProfileDto ToStudentProfile(Student student, string lang)
        {
            var profile = MapStudentFields(student, lang);
            profile.ProfileInfo = UserMapper.ToProfileDto(student.User, lang);
            return profile;
        }

        public static StudentProfileDto MapStudentFields(Student student, string lang)
        {
            var dto = new StudentProfileDto();
            dto.AcademicStatus = LocalizationHelper.Get(student.AcademicStatus, lang);
            dto.Cgpa = student.Cgpa;
            dto.Group = student.Group;
            dto.Level = student.Level;
            dto.ProfileImage = student.ProfileImage;
            return dto;
        }

        public static Student ToCreateStudent(CreateStudentDto dto)
        {
            var student = new Student();
            student = MapEntityFields(dto);
            student.User = UserMapper.ToCreateEntity(dto.UserInfo);
            return student;
        }

        public static partial Student MapEntityFields(CreateStudentDto dto);

        public static partial Student UpdateStatus(StudentStatusDto statusDto);
    }
}