using HUP.Application.DTOs.AcademicDtos;
using HUP.Application.Mappers.Academic;
using HUP.Application.Services.Interfaces;
using HUP.Repositories.Interfaces;

namespace HUP.Application.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repository;

        public DepartmentService(IDepartmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DepartmentDto>> GetAllAsync(string lang)
        {
            var entities = await _repository.GetAllWithDetailsAsync();
            return DepartmentMapper.ToDtoList(entities, lang);
        }

        public async Task<DepartmentDto?> GetByIdAsync(Guid id, string lang)
        {
            var entity = await _repository.GetByIdWithDetailsAsync(id);
            if (entity == null) return null;
            return DepartmentMapper.ToDto(entity, lang);
        }
    }
}