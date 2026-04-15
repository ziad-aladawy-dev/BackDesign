using HUP.Application.DTOs.AcademicDtos.CourseOffering;
using HUP.Application.DTOs.AcademicDtos.Schedule;
using HUP.Common.Helpers;
using Riok.Mapperly.Abstractions;
using HUP.Core.Entities.Academics;
using HUP.Core.Models;

namespace HUP.Application.Mappers.Academic
{
    [Mapper(AllowNullPropertyAssignment = false)]
    public static partial class ScheduleMapper
    {
        [MapProperty(nameof(ScheduleSlotCreateDto.StartTime), nameof(Schedule.StartTime), StringFormat = "HH:mm")]
        [MapProperty(nameof(ScheduleSlotCreateDto.EndTime), nameof(Schedule.EndTime), StringFormat = "HH:mm")]
        public static partial Schedule ToEntity(ScheduleSlotCreateDto createDto);

        private static TimeSpan MapDateTimeToTimeSpan(DateTime dateTime) => dateTime.TimeOfDay;

        public static ScheduleSlotDto ToDto(Schedule entity, string lang)
        {
            var slot = new ScheduleSlotDto();
            slot.CourseName = LocalizationHelper.Get<string>(entity.CourseOffering?.Course?.CourseName?? "", lang);
            slot.StaffName = LocalizationHelper.Get<string>(entity.StaffName ?? "", lang);
            slot.CourseCode = LocalizationHelper.Get<string>(entity.CourseOffering?.Course?.CourseCode ?? "", lang);      
            slot.DayOfWeek = LocalizationHelper.Get(entity.DayOfWeek, lang);
            slot.StartTime = entity.StartTime;
            slot.EndTime = entity.EndTime;
            slot.Group = entity.Group ?? "";
            slot.Hall = LocalizationHelper.Get<string>(entity.Hall ?? "", lang);
            slot.SlotId = entity.Id;
            slot.AvailableSeats = entity.AvailableSeats;
            return slot;
        }
    }
}