using HUP.Application.DTOs.AcademicDtos.Enrollment;
using HUP.Application.Validators.Interfaces;
using HUP.Core.Entities.Academics;
using HUP.Repositories.Interfaces;

namespace HUP.Application.Validators.Implementations
{
    public class EnrollmentValidator : IEnrollmentValidator
    {
        private readonly IStudentRepository _studentRepo;
        private readonly ICourseOfferingRepository _offeringRepo;
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly ISemesterRepository _semesterRepo;
        private readonly IScheduleRepository _scheduleRepo;

        public EnrollmentValidator(
            IStudentRepository studentRepo,
            ICourseOfferingRepository offeringRepo,
            IEnrollmentRepository enrollmentRepo,
            ISemesterRepository semesterRepo,
            IScheduleRepository scheduleRepo)
        {
            _studentRepo = studentRepo;
            _offeringRepo = offeringRepo;
            _enrollmentRepo = enrollmentRepo;
            _semesterRepo = semesterRepo;
            _scheduleRepo = scheduleRepo;
        }

        public async Task ValidateEnrollmentAsync(List<CreateEnrollmentDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                throw new InvalidOperationException("No enrollments to validate.");

            var studentId = dtos.First().StudentId;

            // 1. Check if batch contains duplicates
            var duplicateCourses = dtos.GroupBy(d => d.CourseOfferingId).Where(g => g.Count() > 1).ToList();
            if (duplicateCourses.Any())
            {
                throw new InvalidOperationException("Batch contains duplicate course offerings.");
            }

            // 2. GPA Window Check
            if (!await CanStudentEnroll(studentId))
            {
                throw new InvalidOperationException("Enrollment is not yet open for your GPA tier.");
            }

            var student = await _studentRepo.GetByIdReadOnly(studentId);
            if (student == null)
                throw new InvalidOperationException($"Student {studentId} not found.");

            var studentGroup = student.Group;

            // Prepare list of target schedules to check for conflicts within the batch
            var batchSchedules = new List<Schedule>();

            foreach (var dto in dtos)
            {
                // 3. Duplicate Check against DB
                var existingEnrollment = await _enrollmentRepo.GetExistingAsync(studentId, dto.CourseOfferingId);
                if (existingEnrollment != null)
                {
                    throw new InvalidOperationException($"Student is already enrolled in course offering {dto.CourseOfferingId}.");
                }

                // Retrieve with Schedules
                var courseOffering = await _offeringRepo.GetWithSchedulesAsync(dto.CourseOfferingId);
                if (courseOffering == null)
                    throw new InvalidOperationException($"Course offering {dto.CourseOfferingId} not found.");

                if (courseOffering.Semester == null)
                    throw new InvalidOperationException($"Course offering {dto.CourseOfferingId} does not have a semester associated.");

                // Check Schedule Exists
                //if (courseOffering.Schedules == null || !courseOffering.Schedules.Any())
                //    throw new InvalidOperationException($"Course offering {dto.CourseOfferingId} does not have any schedules.");
                var targetSchedule = await _scheduleRepo.GetByIdReadOnly(dto.ScheduleId);
                if (targetSchedule == null)
                    throw new InvalidOperationException($"Schedule {dto.ScheduleId} not found.");

                if (targetSchedule.CourseOfferingId != dto.CourseOfferingId)
                    throw new InvalidOperationException($"Schedule does not belong to this course offering.");

                //var targetSchedule = courseOffering.Schedules.FirstOrDefault(s => s.Id == dto.ScheduleId);
                if (targetSchedule == null)
                    throw new InvalidOperationException($"Schedule {dto.ScheduleId} not found or does not belong to course offering {dto.CourseOfferingId}.");

                // 4. Prerequisite Check
                if (courseOffering.Course != null && courseOffering.Course.PrerequisiteId != null)
                {
                    var hasPassed = await _enrollmentRepo.HasPassedPrerequisiteAsync(studentId, courseOffering.Course.PrerequisiteId.Value);
                    if (!hasPassed)
                    {
                        throw new InvalidOperationException($"Prerequisite not met for course {courseOffering.Course.CourseCode}.");
                    }
                }

                // 5. Capacity Check
                if (targetSchedule.AvailableSeats <= 0)
                {
                    throw new InvalidOperationException($"Seat unavailable for schedule {targetSchedule.DayOfWeek} {targetSchedule.StartTime}.");
                }

                // Collect schedules for conflict check
                batchSchedules.Add(targetSchedule);

                // Check conflicts within the DB existing enrollments
                var currentEnrollments = await _enrollmentRepo.GetByStudentAndSemesterAsync(studentId, courseOffering.Semester.SemesterName);

                foreach (var enrolled in currentEnrollments)
                {
                    // For existing enrollments we check against their Schedule
                    if (enrolled.Schedule != null)
                    {
                        var existingSlot = enrolled.Schedule;
                        if (targetSchedule.DayOfWeek == existingSlot.DayOfWeek)
                        {
                            if (targetSchedule.StartTime < existingSlot.EndTime && targetSchedule.EndTime > existingSlot.StartTime)
                            {
                                throw new InvalidOperationException($"Time conflict with course {enrolled.CourseOffering?.Course?.CourseCode} on {targetSchedule.DayOfWeek}.");
                            }
                        }
                    }
                    else
                    {
                         // Fallback to older matching by group if ScheduleId is null (e.g. older data)
                         var enrolledSchedules = enrolled.CourseOffering?.Schedules?.Where(s => s.Group == studentGroup);
                         if (enrolledSchedules != null)
                         {
                             foreach (var existingSlot in enrolledSchedules)
                             {
                                 if (targetSchedule.DayOfWeek == existingSlot.DayOfWeek)
                                 {
                                     if (targetSchedule.StartTime < existingSlot.EndTime && targetSchedule.EndTime > existingSlot.StartTime)
                                     {
                                         throw new InvalidOperationException($"Time conflict with course {enrolled.CourseOffering?.Course?.CourseCode} on {targetSchedule.DayOfWeek}.");
                                     }
                                 }
                             }
                         }
                    }
                }
            }

            // 6. Conflict Check within the batch itself
            for (int i = 0; i < batchSchedules.Count; i++)
            {
                for (int j = i + 1; j < batchSchedules.Count; j++)
                {
                    var s1 = batchSchedules[i];
                    var s2 = batchSchedules[j];

                    if (s1.DayOfWeek == s2.DayOfWeek)
                    {
                        if (s1.StartTime < s2.EndTime && s1.EndTime > s2.StartTime)
                        {
                            throw new InvalidOperationException($"Time conflict between batch items on {s1.DayOfWeek}.");
                        }
                    }
                }
            }
        }

        public async Task ValidateDropAsync(Guid enrollmentId, Guid studentId)
        {
            var enrollment = await _enrollmentRepo.GetByIdWithDetailsAsync(enrollmentId);
            if (enrollment == null)
                throw new KeyNotFoundException("Enrollment not found.");

            if (enrollment.StudentId != studentId)
                throw new UnauthorizedAccessException("Cannot drop another student's course.");

            var activeSemester = await _semesterRepo.GetActiveSemesterAsync();
            if (activeSemester == null)
                throw new InvalidOperationException("No active semester.");

            // 1. Drop Deadline Check
            if (DateTime.UtcNow > activeSemester.DropDeadline)
            {
                throw new InvalidOperationException("Drop deadline has passed.");
            }

            // 2. Minimum Credits Check (e.g. 12 credits)
            // Fetch all current enrollments
            var currentEnrollments = await _enrollmentRepo.GetByStudentAndSemesterAsync(studentId, activeSemester.SemesterName);
            var currentCredits = currentEnrollments.Sum(e => e.CourseOffering?.Course?.Credits ?? 0);
            var courseCredits = enrollment.CourseOffering?.Course?.Credits ?? 0;

            if (currentCredits - courseCredits < 12)
            {
                // Warning: Business rule might vary (e.g. withdrawal vs drop).
                // For now, enforcing min credits for "Drop".
                throw new InvalidOperationException("Cannot drop course. Total credits would fall below minimum load (12).");
            }
        }

        private async Task<bool> CanStudentEnroll(Guid studentId)
        {
            var student = await _studentRepo.GetByIdReadOnly(studentId);
            if (student == null) return false;

            var activeSemester = await _semesterRepo.GetActiveSemesterAsync();
            if (activeSemester == null) return false;

            var startTime = activeSemester.StartDate;
            var now = DateTime.UtcNow;

            var gpa = student.Cgpa;

            if (gpa >= 3.8m)
            {
                return now >= startTime;
            }
            else if (gpa >= 3.5m)
            {
                return now >= startTime.AddHours(2);
            }
            else
            {
                return now >= startTime.AddHours(4);
            }
        }
    }
}
