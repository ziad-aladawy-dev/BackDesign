using HUP.Application.Services.Interfaces;
using HUP.Core.Entities.Academics;
using HUP.Repositories.Interfaces;

namespace HUP.Application.Services.Implementations
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public ExamService(IExamRepository examRepository, IEnrollmentRepository enrollmentRepository)
        {
            _examRepository = examRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<IEnumerable<Exam>> GetStudentExamScheduleAsync(Guid studentId)
        {
            // 1. Get student's enrolled courses
            var enrollments = await _enrollmentRepository.GetByStudentId(studentId);
            var courseIds = enrollments.Select(e => e.CourseOffering.CourseId).ToList();

            // 2. Fetch exams for these courses
            // Assuming IExamRepository has a method to fetch by CourseIds
            return await _examRepository.GetByCoursesAsync(courseIds);
        }
    }
}