using HUP.Application.Mappers.Academic;
using HUP.Application.Services.Interfaces;
using HUP.Application.DTOs.AcademicDtos.Enrollment;
using HUP.Core.Entities.Academics;
using HUP.Repositories.Interfaces;
using System.Threading.Tasks;
using HUP.Application.DTOs.AcademicDtos;
using HUP.Common.Helpers;
using HUP.Application.Validators.Interfaces;
using HUP.Core.Enums;
using HUP.Core.Interfaces;

namespace HUP.Application.Services.Implementations
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _repository;
        private readonly IStudentRepository _studentRepo;
        private readonly IProgramPlanRepository _planRepo;
        private readonly ICourseOfferingRepository _offeringRepo;
        private readonly IScheduleRepository _scheduleRepo;
        private readonly IEnrollmentValidator _validator;
        private readonly ITransactionService _transactionService;
        private readonly ISemesterRepository _semesterRepo;

        public EnrollmentService(
            IEnrollmentRepository repository,
            IStudentRepository studentRepo,
            IProgramPlanRepository planRepo,
            ICourseOfferingRepository offeringRepo,
            IScheduleRepository scheduleRepo,
            IEnrollmentValidator validator,
            ITransactionService transactionService,
            ISemesterRepository semesterRepo)
        {
            _repository = repository;
            _studentRepo = studentRepo;
            _planRepo = planRepo;
            _offeringRepo = offeringRepo;
            _scheduleRepo = scheduleRepo;
            _validator = validator;
            _transactionService = transactionService;
            _semesterRepo = semesterRepo;
        }

        public async Task AddAsync(List<CreateEnrollmentDto> dtos)
        {
            await _transactionService.ExecuteInTransactionAsync(async () =>
            {
                // Validate Business Rules for the whole batch
                await _validator.ValidateEnrollmentAsync(dtos);

                foreach (var dto in dtos)
                {
                    // Perform Atomic Seat Booking
                    var booked = await _scheduleRepo.TryBookSeatAsync(dto.ScheduleId);
                    if (!booked)
                    {
                        throw new InvalidOperationException($"Seat unavailable for schedule {dto.ScheduleId}.");
                    }

                    // Create Enrollment Record
                    var enrollment = EnrollmentMapper.ToEntityFromCreateDto(dto);
                    enrollment.Id = Guid.NewGuid();
                    enrollment.ScheduleId = dto.ScheduleId;
                    enrollment.EnrollmentDate = DateTime.Now;
                    enrollment.CreatedAt = DateTime.Now;
                    enrollment.Status = EnrollmentStatus.Registered;

                    await _repository.AddAsync(enrollment);
                }

                await _repository.SaveChangesAsync();
            });
        }

        public async Task<bool> CanStudentEnroll(Guid studentId)
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

        public async Task<IEnumerable<EnrollmentResponseDto>> GetAllAsync(string lang)
        {
            var entities =  await _repository.GetAllWithDetailsAsync();
            var dtos = entities.Select(e => EnrollmentMapper.ToResponseDto(e, lang));
            return dtos;
        }

        public async Task<EnrollmentResponseDto> GetByIdAsync(Guid id, string lang)
        {
            var entity = await _repository.GetByIdWithDetailsAsync(id);
            var dto = EnrollmentMapper.ToResponseDto(entity, lang);
            return dto;
        }

        public async Task<bool> Exists(CreateEnrollmentDto dto)
        {
            var entity = await _repository.GetExistingAsync(dto.StudentId, dto.CourseOfferingId);
            return entity != null;
        }

        public async Task Remove(Guid id)
        {
            var enrollment = await _repository.GetByIdTrackingAsync(id);
            if (enrollment != null)
            {
                await _validator.ValidateDropAsync(id, enrollment.StudentId);
                await _repository.RemoveAsync(id);
                await _repository.SaveChangesAsync();
            }
        }
        public async Task SoftDelete(Guid id)
        {
            var enrollment = await _repository.GetByIdTrackingAsync(id);
            if (enrollment != null)
            {
                await _validator.ValidateDropAsync(id, enrollment.StudentId);

                enrollment.IsDeleted = true;
                enrollment.UpdatedAt = DateTime.Now;
                await _repository.SaveChangesAsync();
            }
        }

        public async Task Update(Guid id, UpdateEnrollmentDto dto)
        {
            var enrollment = await _repository.GetByIdTrackingAsync(id);
            enrollment.UpdatedAt = DateTime.Now;
            EnrollmentMapper.ToUpdate(dto, enrollment);
            await _repository.SaveChangesAsync();
        }
        

        public async Task<List<SemesterTranscriptDto>> GetStudentGradesAsync(Guid studentId, string lang)
        {
            var models = await _repository.GetStudentSemesterGradeModelsAsync(studentId);
            var student = await _studentRepo.GetByIdReadOnly(studentId);

            var departmentId = student.DepartmentId;

            decimal cumulativePoints = 0;
            decimal cumulativeHours = 0;

            var computedCourses = new List<SemesterGradesDto>();

            foreach (var m in models)
            {
                var totalGrade = m.ClassGrade + m.MidtermGrade + m.FinalGrade;

                var programPlan = await _planRepo.GetByIdReadOnly(departmentId, m.CourseId);
                var maxGrade = programPlan.FinalGrade;

                var grade = getGrade(totalGrade / maxGrade * 100);
                var gradePts = GetGradePoints(grade);
                var creditPts = gradePts * m.CourseCredits;

                cumulativePoints += creditPts;
                cumulativeHours += m.CourseCredits;

                computedCourses.Add(new SemesterGradesDto
                {
                    SemesterId = m.SemesterId,
                    SemesterName = LocalizationHelper.Get<string>(m.SemesterName, lang),
                    CourseCode = LocalizationHelper.Get<string>(m.CourseCode,lang),
                    CourseName = LocalizationHelper.Get<string>(m.CourseName,lang),
                    TotalGrade = totalGrade,
                    Grade = grade,
                    CreditHours = m.CourseCredits,
                    GradePoints = gradePts,
                    CreditPoints = creditPts
                });
            }

            var cumulativeGPA = cumulativeHours == 0
                ? 0
                : cumulativePoints / cumulativeHours;

            var grouped = computedCourses.GroupBy(c => c.SemesterName);

            var transcript = new List<SemesterTranscriptDto>();

            foreach (var semGroup in grouped)
            {
                var semCourses = semGroup.ToList();

                decimal semPoints = semCourses.Sum(c => c.CreditPoints);
                decimal semHours = semCourses.Sum(c => c.CreditHours);

                var semesterDto = new SemesterTranscriptDto
                {
                    SemesterName = semGroup.Key,
                    Courses = semCourses,

                    SemesterGPA = semHours == 0 ? 0 : semPoints / semHours,
                    CumulativeGPA = cumulativeGPA
                };

                transcript.Add(semesterDto);
            }

            return transcript;
        }

        public async Task<IEnumerable<EnrollmentResponseDto>> GetRegisteredByStudentAsync(Guid studentId, string lang, EnrollmentFilterDto filter)
        {
            var enrollments = await _repository.GetFilteredAsync(studentId, filter);

            return enrollments.Select(e => EnrollmentMapper.ToResponseDto(e, lang));
        }

        public string getGrade(decimal grade)
        {
            return grade switch
            {
                >= 90 => "A+",
                >= 85 => "A",
                >= 80 => "B+",
                >= 75 => "B",
                >= 70 => "C+",
                >= 65 => "C",
                >= 60 => "D+",
                >= 50 => "D",
                _     => "F"
            };
        }
        public decimal GetGradePoints(string grade)
        {
            return grade switch
            {
                "A+" => 4.0m,
                "A"  => 3.75m,
                "B+" => 3.4m,
                "B"  => 3.1m,
                "C+" => 2.8m,
                "C"  => 2.5m,
                "D+" => 2.2m,
                "D"  => 2.0m,
                "F"  => 0.0m,
                _    => 0.0m
            };
        }
    }
}
