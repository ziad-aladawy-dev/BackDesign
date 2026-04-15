using HUP.Application.Mappers.Academic;
using HUP.Core.Entities.Academics;
using HUP.Repositories.Interfaces;
using HUP.Application.Services.Interfaces;
using System.Threading.Tasks;
using HUP.Application.DTOs.AcademicDtos.CourseOffering;
using Microsoft.AspNetCore.Http.HttpResults;


namespace HUP.Application.Services.Implementations
{
    public class CourseOfferingService : ICourseOfferingService
    {
        private readonly ICourseOfferingRepository _repository;

        public CourseOfferingService(ICourseOfferingRepository repository)
        {
            _repository = repository;
        }

        public async Task<CourseOfferingDto?> GetByIdAsync(Guid id, string lang)
        {
            var entity = await _repository.GetByIdWithDetailsAsync(id);
            if (entity == null)
                return null;
            
            return CourseOfferingMapper.ToDto(entity, lang);
        }

        public async Task<IEnumerable<CourseOfferingDto>> GetAllAsync(string lang)
        {
            var entities = await _repository.GetAllWithDetailsAsync();
            var dtos = entities.Select(o => CourseOfferingMapper.ToDto(o, lang)).ToList();
            return dtos;
        }

        public async Task<bool> Exists(CreateCourseOfferingDto dto)
        {
            var entity = await _repository.GetExistingAsync(dto.CourseId, dto.DepartmentId, dto.SemesterId);
            return entity != null;
        }

        public async Task AddAsync(CreateCourseOfferingDto dto)
        {
            var entity = CourseOfferingMapper.ToEntity(dto);
            entity.Id = Guid.NewGuid();
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        // public async Task Update(Guid id, CreateCourseOfferingDto dto)
        // {
        //     // Need to be implemented
        //     await _repository.SaveChangesAsync();
        // }

        public async Task SoftDelete(Guid id)
        {
            var entity = await _repository.GetByIdTrackingAsync(id);
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.Now;
            await _repository.SaveChangesAsync();
        }

        //TODO
        //fix: return value to the controller
        public async Task Remove(Guid id)
        {
            await _repository.RemoveAsync(id);
            await _repository.SaveChangesAsync();
        }
        
        public async Task<IEnumerable<CourseOfferingDto>> GetActiveCourseOfferingAsync(Guid departmentId,
            Guid semesterId, string lang)
        {
            var entities = await _repository.GetActiveCourseOfferingAsync(departmentId, semesterId);
            var dtos = entities.Select(o => CourseOfferingMapper.ToDto(o, lang)).ToList();
            return dtos;
        }

        public async Task<IEnumerable<CourseOfferingDto>> GetAvailableToRegisterAsync(Guid studentId, string lang)
        {
            var courses = await _repository.GetAvailableToRegisterAsync(studentId);
            var dtos = courses.Select(o => CourseOfferingMapper.ToDto(o, lang)).ToList();
            return dtos;
        }
    }
}

