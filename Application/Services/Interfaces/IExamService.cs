using HUP.Core.Entities.Academics;

namespace HUP.Application.Services.Interfaces
{
    public interface IExamService
    {
        Task<IEnumerable<Exam>> GetStudentExamScheduleAsync(Guid studentId);
    }
}