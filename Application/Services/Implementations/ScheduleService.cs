using HUP.Application.DTOs.AcademicDtos.Schedule;
using HUP.Application.Mappers.Academic;
using HUP.Application.Services.Interfaces;
using HUP.Core.Entities.Academics;
using HUP.Repositories.Interfaces;

namespace HUP.Application.Services.Implementations;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _schedulerepository;
    private readonly ICourseOfferingRepository _courseOfferingRepository;

    public ScheduleService(IScheduleRepository scheduleRepository,  ICourseOfferingRepository courseOfferingRepository)
    {
        _schedulerepository = scheduleRepository;
        _courseOfferingRepository = courseOfferingRepository;
    }
    public async Task Create(ScheduleSlotCreateDto createDto)
    {
        var slot = ScheduleMapper.ToEntity(createDto);
        slot.Id = Guid.NewGuid();
        slot.CreatedAt = DateTime.Now;
        slot.AvailableSeats = createDto.TotalSeats; // Initialize available seats
        await _schedulerepository.AddAsync(slot);
    }

    public async Task<IEnumerable<ScheduleSlotDto>> GetSlotsByStudentEnrollments(Guid studentId, string lang)
    {
        var slots = await _schedulerepository.GetByStudentEnrollmentsAsync(studentId);
        return slots.Select(s => ScheduleMapper.ToDto(s, lang));
    }

    public async Task<IEnumerable<ScheduleSlotDto>> GetAvailableScheduleForEnrollment(Guid studentId, string lang)
    {
        var availableCourses = await _courseOfferingRepository.GetAvailableToRegisterAsync(studentId);
        var slots = await _schedulerepository.GetAvailableSlotsAsync();
        var courseOfferingIds = availableCourses
            .Select(c => c.Id)
            .ToHashSet();

        var availableSchedule = slots
            .Where(s => courseOfferingIds.Contains(s.CourseOfferingId));
        
        return availableSchedule.Select(s => ScheduleMapper.ToDto(s, lang));
    }

    public Task Update(ScheduleSlotCreateDto createDto)
    {
        throw new NotImplementedException();
    }

    public async Task SoftDelete(Guid id)
    {
        // Using GenericRepository's non-virtual RemoveAsync which handles Soft Delete if entity is BaseEntity.
        // But GenericRepository.RemoveAsync requires an ID and fetches it.
        // ScheduleService previously used GetByIdTracking.
        // If we use RemoveAsync from repository, it will do the job.
        await _schedulerepository.RemoveAsync(id);
        await _schedulerepository.SaveChangesAsync();
    }

    public async Task Remove(Guid id)
    {
         // Hard delete
         // GenericRepository.RemoveAsync does soft delete if BaseEntity.
         // If we want hard delete, we need a specific method or modify GenericRepo.
         // The requirement was "Soft Delete" in previous context, but GenericRepo handles it.
         // If this method implies HARD delete, we might need a specific HardDelete in repo.
         // But for now, let's assume RemoveAsync is what's available.
         // Actually, GenericRepository checks 'if (entity is BaseEntity softDeletable)'.
         // If we want hard delete, we need to bypass that check or use a different method.
         // For now, mapping Remove to RemoveAsync (Soft) is safe default unless specified otherwise.
         // Wait, the generic implementation forces soft delete if BaseEntity.
         // If hard delete is needed, we need a new method on GenericRepo 'HardRemoveAsync'.
         // I will leave it as is for now as this is a cleanup task, not adding new Hard Delete features unless requested.
         await _schedulerepository.RemoveAsync(id);
         await _schedulerepository.SaveChangesAsync();
    }
}