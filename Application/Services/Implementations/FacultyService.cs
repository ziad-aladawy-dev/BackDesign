using HUP.Application.DTOs.AcademicDtos;
using HUP.Application.Mappers.Academic;
using HUP.Application.Services.Interfaces;
using HUP.Repositories.Interfaces;

namespace HUP.Application.Services.Implementations
{
    public class FacultyService : IFacultyService
    {
        private readonly IFacultyRepository _repository;

        public FacultyService(IFacultyRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<FacultyDto>> GetAllAsync(string lang)
        {
            var entities = await _repository.GetAllWithDetailsAsync();
            return FacultyMapper.ToDtoList(entities, lang);
        }

        public async Task<FacultyDto?> GetByIdAsync(Guid id, string lang)
        {
            var entity = await _repository.GetByIdReadOnly(id);
            if (entity == null) return null;
            return FacultyMapper.ToDto(entity, lang);
        }
    }
}