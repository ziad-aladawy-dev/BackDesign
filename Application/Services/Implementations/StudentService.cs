using HUP.Application.DTOs.AcademicDtos;
using HUP.Application.DTOs.AcademicDtos.Student;
using HUP.Application.Mappers.Identity;
using HUP.Application.Services.Interfaces;
using HUP.Common.Helpers;
using HUP.Core.Entities.Identity;
using HUP.Core.Enums;
using HUP.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace HUP.Application.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _hasher;

    public StudentService(IStudentRepository studentRepository, IUserRepository userRepository, IPasswordHasher<User> hasher)
    {
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _hasher = hasher;
    }
    
    public async Task<StudentProfileDto> GetStudentProfile(Guid userId , string lang)
    {
        var student = await _studentRepository.GetByIdWithDetailsAsync(userId);
        if (student == null)
            return null;
        var profile = StudentMapper.ToStudentProfile(student, lang);
        return profile;
    }
    
    public async Task AddStudent(CreateStudentDto dto)
    {
        var student = StudentMapper.ToCreateStudent(dto);
        var user = student.User;
        user.CreatedAt = DateTime.UtcNow;
        var hashed = _hasher.HashPassword(user, dto.UserInfo.PasswordHash);
        user.PasswordHash = hashed;
        user.Id = new Guid();
        await _userRepository.AddAsync(user);
        student.UserId = user.Id;
        await _studentRepository.AddAsync(student);
        await _studentRepository.SaveChangesAsync();
    }
    
    public async Task<bool> UpdateStudentStatus(StudentStatusDto statusDto)
    {
        var student = await _studentRepository.GetByIdTrackingAsync(statusDto.StudentId);
        if (student == null)
            return false;
        student = StudentMapper.UpdateStatus(statusDto);
        await _studentRepository.SaveChangesAsync();
        return true;
    }

    public async Task<string> UploadProfilePhotoAsync(Guid studentId, IFormFile file)
    {
        // 1. Validation
        if (file == null || file.Length == 0)
            throw new ArgumentException("File cannot be empty.");

        if (file.Length > 5 * 1024 * 1024) // 5MB
            throw new ArgumentException("File size exceeds 5MB limit.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("Invalid file type. Only JPEG and PNG are allowed.");

        // 2. Storage Path
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "students");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{studentId}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // 3. Cleanup Old Photo (if exists, though overwrite works for same ext, safety for different ext)
        // Check DB for existing photo path if extension differs, or just delete matches
        var student = await _studentRepository.GetByIdTrackingAsync(studentId);
        if (student == null)
            throw new KeyNotFoundException("Student not found.");

        if (!string.IsNullOrEmpty(student.ProfileImage))
        {
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", student.ProfileImage.TrimStart('/'));
            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }
        }

        // 4. Save File
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 5. Update DB
        var relativePath = $"/uploads/students/{uniqueFileName}";
        student.ProfileImage = relativePath;
        await _studentRepository.SaveChangesAsync(); // Using repo save which calls context.SaveChanges

        return relativePath;
    }
}
