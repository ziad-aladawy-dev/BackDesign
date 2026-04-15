using ClosedXML.Excel;
using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.AcademicDtos.UserDtos;
using HUP.Application.Services.Interfaces;
using HUP.Common.Helpers;
using HUP.Core.Entities.Academics;
using HUP.Core.Entities.Identity;
using HUP.Core.Enums;
using HUP.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HUP.Application.Services.Implementations
{
    public class UserManagementService : IUserManagementService
    {
        private readonly HupDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            HupDbContext context,
            IPasswordHasher<User> passwordHasher,
            ILogger<UserManagementService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        #region Query Methods

        public async Task<PaginatedResult<UserListDto>> GetUsersAsync(UserFilterDto? filter = null)
        {
            try
            {
                filter ??= new UserFilterDto();

                var query = BuildUserQuery(filter);

                var totalCount = await query.CountAsync();

                query = ApplySorting(query, filter.SortBy, filter.SortOrder);

                var items = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(u => MapToUserListDto(u))
                    .ToListAsync();

                return new PaginatedResult<UserListDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsersAsync");
                throw;
            }
        }

        public async Task<UserDetailsDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .Include(u => u.PersonalInfo)
                .Include(u => u.ContactInfo)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            return await MapToUserDetailsDto(user);
        }

        public async Task<IEnumerable<UserListDto>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<UserListDto>();

            var users = await _context.Users
                .Include(u => u.UserRole)
                .Where(u => !u.IsDeleted && (
                    u.FullName.Contains(searchTerm) ||
                    u.NationalId.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    (u.ContactInfo != null && u.ContactInfo.PhoneNumber.Contains(searchTerm))))
                .Take(50)
                .Select(u => MapToUserListDto(u))
                .ToListAsync();

            return users;
        }

        #endregion

        #region Create Methods

        public async Task<UserActionResponse> CreateUserAsync(AdminCreateUserDto dto)
        {
            try
            {
                var exists = await _context.Users
                    .AnyAsync(u => u.NationalId == dto.NationalId || u.Email == dto.Email);

                if (exists)
                {
                    return new UserActionResponse
                    {
                        Success = false,
                        Message = "User with same National ID or Email already exists"
                    };
                }

                var userId = Guid.NewGuid();

                var user = new User
                {
                    Id = userId,
                    NationalId = dto.NationalId,
                    FullName = dto.FullName,
                    //FullNameAr = dto.FullNameAr,
                    Email = dto.Email,
                    RoleId = dto.RoleId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordExpiryDate = DateTime.UtcNow.AddMonths(6),

                    PersonalInfo = new UserPersonalInfo
                    {
                        UserId = userId,
                        Gender = ParseGender(dto.Gender),
                        BirthDate = dto.BirthDate ?? DateTime.Now,
                        Religion = ParseReligion(dto.Religion),
                        Nationality = ParseNationality(dto.Nationality),
                        BirthPlace = ParseBirthPlace(dto.BirthPlace)
                    },
                    ContactInfo = new UserContact
                    {
                        UserId = userId,
                        Address = dto.Address,
                        City = ParseCity(dto.City),
                        PhoneNumber = dto.PhoneNumber,
                        AltEmail = dto.AltEmail
                    }
                };

                user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

                await _context.Users.AddAsync(user);

                switch (dto.UserType?.ToLower())
                {
                    case "student":
                        if (!dto.FacultyId.HasValue || !dto.DepartmentId.HasValue)
                        {
                            return new UserActionResponse
                            {
                                Success = false,
                                Message = "Faculty and Department are required for students"
                            };
                        }

                        var studentDepartmentExists = await _context.DepartmentFaculties
                            .AnyAsync(df => df.DepartmentId == dto.DepartmentId.Value &&
                                           df.FacultyId == dto.FacultyId.Value);

                        if (!studentDepartmentExists)
                        {
                            return new UserActionResponse
                            {
                                Success = false,
                                Message = "The selected department does not belong to the selected faculty"
                            };
                        }

                        var student = new Student
                        {
                            UserId = user.Id,
                            UniversityCode = !string.IsNullOrEmpty(dto.UniversityCode)
                                ? dto.UniversityCode
                                : GenerateUniversityCode(),
                            UniversityEmail = dto.Email,
                            ProfileImage = dto.ProfileImage,
                            FacultyID = dto.FacultyId.Value,
                            DepartmentId = dto.DepartmentId.Value,
                            AcademicStatus = dto.AcademicStatus ?? AcademicStatus.Active,
                            Level = dto.Level ?? 1,
                            Cgpa = dto.CGPA ?? 0,
                        };
                        await _context.Students.AddAsync(student);
                        break;

                    case "instructor":
                        if (!dto.DepartmentId.HasValue)
                        {
                            return new UserActionResponse
                            {
                                Success = false,
                                Message = "Department is required for instructors"
                            };
                        }

                        var instructor = new Staff
                        {
                            UserId = user.Id,
                            DepartmentId = dto.DepartmentId.Value,
                            FacultyId = dto.FacultyId,
                            Title = ParseAcademicTitle(dto.AcademicTitle) ?? StaffTitle.Instructor,
                            Category = StaffCategory.Academic
                        };
                        await _context.Staff.AddAsync(instructor);
                        break;
                }

                await _context.SaveChangesAsync();

                return new UserActionResponse
                {
                    Success = true,
                    Message = "User created successfully",
                    Data = new { UserId = user.Id }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return new UserActionResponse
                {
                    Success = false,
                    Message = $"Error creating user: {ex.Message}"
                };
            }
        }

        #endregion

        #region Update Methods

        public async Task<UserActionResponse> UpdateUserAsync(Guid userId, UpdateUserDto dto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.PersonalInfo)
                    .Include(u => u.ContactInfo)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new UserActionResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                if (!string.IsNullOrEmpty(dto.FullName))
                    user.FullName = dto.FullName;

                //if (!string.IsNullOrEmpty(dto.FullNameAr))
                //    user.FullNameAr = dto.FullNameAr;

                if (!string.IsNullOrEmpty(dto.Email))
                    user.Email = dto.Email;

                if (dto.RoleId.HasValue)
                    user.RoleId = dto.RoleId.Value;

                if (dto.IsActive.HasValue)
                    user.IsActive = dto.IsActive.Value;

                if (user.PersonalInfo == null)
                {
                    user.PersonalInfo = new UserPersonalInfo { UserId = userId };
                }

                if (!string.IsNullOrEmpty(dto.Gender))
                    user.PersonalInfo.Gender = ParseGender(dto.Gender);

                if (dto.BirthDate.HasValue)
                    user.PersonalInfo.BirthDate = dto.BirthDate.Value;

                if (!string.IsNullOrEmpty(dto.Religion))
                    user.PersonalInfo.Religion = ParseReligion(dto.Religion);

                if (!string.IsNullOrEmpty(dto.Nationality))
                    user.PersonalInfo.Nationality = ParseNationality(dto.Nationality);

                if (!string.IsNullOrEmpty(dto.BirthPlace))
                    user.PersonalInfo.BirthPlace = ParseBirthPlace(dto.BirthPlace);

                if (user.ContactInfo == null)
                {
                    user.ContactInfo = new UserContact { UserId = userId };
                }

                if (!string.IsNullOrEmpty(dto.Address))
                    user.ContactInfo.Address = dto.Address;

                if (!string.IsNullOrEmpty(dto.City))
                    user.ContactInfo.City = ParseCity(dto.City);

                if (!string.IsNullOrEmpty(dto.PhoneNumber))
                    user.ContactInfo.PhoneNumber = dto.PhoneNumber;

                if (!string.IsNullOrEmpty(dto.AltEmail))
                    user.ContactInfo.AltEmail = dto.AltEmail;

                var existingStudent = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                var existingInstructor = await _context.Staff
                    .FirstOrDefaultAsync(i => i.UserId == userId);

                string newUserType = !string.IsNullOrEmpty(dto.UserType)
                    ? dto.UserType.ToLower()
                    : (existingStudent != null ? "student" :
                       existingInstructor != null ? "instructor" : "admin");

                if (!string.IsNullOrEmpty(dto.UserType))
                {
                    string targetType = dto.UserType.ToLower();

                    if (existingStudent != null && targetType != "student")
                        _context.Students.Remove(existingStudent);

                    if (existingInstructor != null && targetType != "instructor")
                        _context.Staff.Remove(existingInstructor);
                }

                switch (newUserType)
                {
                    case "student":
                        Student student;

                        if (existingStudent != null)
                        {
                            student = existingStudent;
                        }
                        else
                        {
                            student = new Student { UserId = userId };
                            await _context.Students.AddAsync(student);
                        }

                        if (!string.IsNullOrEmpty(dto.UniversityCode))
                            student.UniversityCode = dto.UniversityCode;
                        else if (string.IsNullOrEmpty(student.UniversityCode))
                            student.UniversityCode = GenerateUniversityCode();

                        student.UniversityEmail = !string.IsNullOrEmpty(dto.Email) ? dto.Email : user.Email;

                        if (dto.FacultyId.HasValue)
                            student.FacultyID = dto.FacultyId.Value;

                        if (dto.DepartmentId.HasValue)
                        {
                            if (dto.FacultyId.HasValue)
                            {
                                var departmentExists = await _context.DepartmentFaculties
                                    .AnyAsync(df => df.DepartmentId == dto.DepartmentId.Value &&
                                                   df.FacultyId == dto.FacultyId.Value);

                                if (!departmentExists)
                                {
                                    return new UserActionResponse
                                    {
                                        Success = false,
                                        Message = "The selected department does not belong to the selected faculty"
                                    };
                                }
                            }
                            student.DepartmentId = dto.DepartmentId.Value;
                        }

                        if (!string.IsNullOrEmpty(dto.AcademicStatus))
                            student.AcademicStatus = ParseAcademicStatus(dto.AcademicStatus) ?? AcademicStatus.Active;

                        if (dto.Level.HasValue)
                            student.Level = dto.Level.Value;

                        if (dto.CGPA.HasValue)
                            student.Cgpa = dto.CGPA.Value;

                        if (!string.IsNullOrEmpty(dto.ProfileImage))
                            student.ProfileImage = dto.ProfileImage;

                        break;

                    case "instructor":
                        Staff instructor;

                        if (existingInstructor != null)
                        {
                            instructor = existingInstructor;
                        }
                        else
                        {
                            instructor = new Staff { UserId = userId };
                            await _context.Staff.AddAsync(instructor);
                        }

                        if (dto.DepartmentId.HasValue)
                            instructor.DepartmentId = dto.DepartmentId.Value;

                        if (!string.IsNullOrEmpty(dto.AcademicTitle))
                            instructor.Title = ParseAcademicTitle(dto.AcademicTitle) ?? StaffTitle.Instructor;

                        break;

                    case "admin":
                        break;
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new UserActionResponse
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = new { UserId = user.Id }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user {userId}");
                return new UserActionResponse
                {
                    Success = false,
                    Message = $"Error updating user: {ex.Message}"
                };
            }
        }

        #endregion

        #region Status Change Methods

        public async Task<UserActionResponse> ActivateUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new UserActionResponse { Success = false, Message = "User not found" };

            if (user.IsActive)
                return new UserActionResponse { Success = false, Message = "User is already active" };

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserActionResponse { Success = true, Message = "User activated successfully" };
        }

        public async Task<UserActionResponse> DeactivateUserAsync(Guid userId, string reason = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new UserActionResponse { Success = false, Message = "User not found" };

            if (!user.IsActive)
                return new UserActionResponse { Success = false, Message = "User is already inactive" };

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserActionResponse { Success = true, Message = "User deactivated successfully" };
        }

        public async Task<UserActionResponse> SoftDeleteUserAsync(Guid userId, string reason = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new UserActionResponse { Success = false, Message = "User not found" };

            if (user.IsDeleted)
                return new UserActionResponse { Success = false, Message = "User is already deleted" };

            user.IsDeleted = true;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserActionResponse { Success = true, Message = "User soft deleted successfully" };
        }

        public async Task<UserActionResponse> RestoreUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new UserActionResponse { Success = false, Message = "User not found" };

            if (!user.IsDeleted)
                return new UserActionResponse { Success = false, Message = "User is not deleted" };

            user.IsDeleted = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserActionResponse { Success = true, Message = "User restored successfully" };
        }

        public async Task<UserActionResponse> HardDeleteUserAsync(Guid userId)
        {
            try
            {
                var hasDependencies = await CheckUserDependenciesAsync(userId);
                if (hasDependencies)
                {
                    return new UserActionResponse
                    {
                        Success = false,
                        Message = "Cannot delete user with existing dependencies"
                    };
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return new UserActionResponse { Success = false, Message = "User not found" };

                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student != null)
                    _context.Students.Remove(student);

                var instructor = await _context.Staff.FirstOrDefaultAsync(i => i.UserId == userId);
                if (instructor != null)
                    _context.Staff.Remove(instructor);

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return new UserActionResponse { Success = true, Message = "User permanently deleted" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error hard deleting user {userId}");
                return new UserActionResponse
                {
                    Success = false,
                    Message = $"Error deleting user: {ex.Message}"
                };
            }
        }

        #endregion

        #region Bulk Operations

        public async Task<BulkOperationResult> BulkUpdateUsersAsync(BulkUserActionDto dto)
        {
            var result = new BulkOperationResult();

            if (dto.UserIds == null || !dto.UserIds.Any())
            {
                result.Message = "No user IDs provided";
                return result;
            }

            foreach (var userId in dto.UserIds)
            {
                var item = new BulkOperationItem { UserId = userId };

                try
                {
                    UserActionResponse actionResult = null;

                    switch (dto.Action.ToLower())
                    {
                        case "activate":
                            actionResult = await ActivateUserAsync(userId);
                            break;

                        case "deactivate":
                            actionResult = await DeactivateUserAsync(userId, dto.Reason);
                            break;

                        case "soft-delete":
                            actionResult = await SoftDeleteUserAsync(userId, dto.Reason);
                            break;

                        case "restore":
                            actionResult = await RestoreUserAsync(userId);
                            break;

                        case "change-role":
                            if (!dto.NewRoleId.HasValue)
                                throw new ArgumentException("New role ID is required");

                            actionResult = await ChangeUserRoleAsync(userId, dto.NewRoleId.Value);
                            break;

                        default:
                            item.Success = false;
                            item.Message = $"Unknown action: {dto.Action}";
                            result.FailedCount++;
                            result.Items.Add(item);
                            continue;
                    }

                    item.Success = actionResult.Success;
                    item.Message = actionResult.Message;

                    if (actionResult.Success)
                        result.SuccessCount++;
                    else
                        result.FailedCount++;
                }
                catch (Exception ex)
                {
                    item.Success = false;
                    item.Error = ex.Message;
                    result.FailedCount++;
                }

                result.Items.Add(item);
            }

            result.ProcessedCount = dto.UserIds.Count;
            result.Success = result.SuccessCount > 0;
            result.Message = $"{result.SuccessCount} of {result.ProcessedCount} users processed successfully";

            return result;
        }

        private async Task<UserActionResponse> ChangeUserRoleAsync(Guid userId, Guid newRoleId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new UserActionResponse { Success = false, Message = "User not found" };

            user.RoleId = newRoleId;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserActionResponse { Success = true, Message = "Role changed successfully" };
        }

        #endregion

        #region Password Management

        public async Task<UserActionResponse> ResetPasswordAsync(Guid userId, ResetPasswordDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new UserActionResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                string newPassword;
                bool usedNationalId = false;

                if (string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    newPassword = user.NationalId;
                    usedNationalId = true;

                    _logger.LogInformation($"No password provided for user {userId}, using National ID as default password");
                }
                else
                {
                    newPassword = dto.NewPassword;

                    if (newPassword.Length < 6)
                    {
                        return new UserActionResponse
                        {
                            Success = false,
                            Message = "Password must be at least 6 characters long if provided"
                        };
                    }
                }

                var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, newPassword);
                if (passwordVerification == PasswordVerificationResult.Success)
                {
                    return new UserActionResponse
                    {
                        Success = false,
                        Message = usedNationalId
                            ? "New password (National ID) must be different from current password"
                            : "New password must be different from current password"
                    };
                }

                user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);

                if (dto.ForcePasswordChange)
                {
                    user.PasswordExpiryDate = DateTime.UtcNow.AddDays(-1);
                }
                else
                {
                    user.PasswordExpiryDate = DateTime.UtcNow.AddMonths(6);
                }

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                string successMessage;
                if (usedNationalId)
                {
                    successMessage = "Password reset successfully using National ID. User must change password on next login.";
                }
                else
                {
                    successMessage = dto.ForcePasswordChange
                        ? "Password reset successfully. User must change password on next login."
                        : "Password reset successfully.";
                }

                string activityDescription = usedNationalId
                    ? $"Password reset to National ID. Force change: {dto.ForcePasswordChange}. Reason: {dto.Reason ?? "No reason provided"}"
                    : $"Password reset by admin. Force change: {dto.ForcePasswordChange}. Reason: {dto.Reason ?? "No reason provided"}";

                return new UserActionResponse
                {
                    Success = true,
                    Message = successMessage,
                    Data = new
                    {
                        UserId = user.Id,
                        PasswordExpiryDate = user.PasswordExpiryDate,
                        ForcePasswordChange = dto.ForcePasswordChange,
                        UsedNationalId = usedNationalId
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting password for user {userId}");
                return new UserActionResponse
                {
                    Success = false,
                    Message = $"Error resetting password: {ex.Message}"
                };
            }
        }

        public async Task<UserActionResponse> ForcePasswordChangeAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new UserActionResponse { Success = false, Message = "User not found" };

            user.PasswordExpiryDate = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserActionResponse { Success = true, Message = "Password change forced successfully" };
        }

        public async Task<UserActionResponse> ExtendPasswordExpiryAsync(Guid userId, int months)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new UserActionResponse { Success = false, Message = "User not found" };

            user.PasswordExpiryDate = DateTime.UtcNow.AddMonths(months);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserActionResponse { Success = true, Message = "Password expiry extended successfully" };
        }

        #endregion

        #region Statistics

        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserRole)
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Faculty)
                .Include(s => s.Department)
                .Where(s => !s.User.IsDeleted)
                .ToListAsync();

            var instructors = await _context.Staff
                .Include(i => i.User)
                .Include(i => i.Department)
                    .ThenInclude(d => d.DepartmentFaculties)
                        .ThenInclude(df => df.Faculty)
                .Where(i => !i.User.IsDeleted)
                .ToListAsync();

            var stats = new UserStatisticsDto
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsActive),
                InactiveUsers = users.Count(u => !u.IsActive),
                DeletedUsers = users.Count(u => u.IsDeleted),
                StudentsCount = students.Count,
                InstructorsCount = instructors.Count,
                AdminsCount = users.Count(u => u.UserRole?.Name == "Admin"),
                PasswordExpiredCount = users.Count(u => u.PasswordExpiryDate < DateTime.UtcNow),
                UsersWithMissingInfo = await CountUsersWithMissingInfoAsync(),
                GeneratedAt = DateTime.UtcNow
            };

            var roles = await _context.Roles.ToListAsync();
            foreach (var role in roles)
            {
                var count = users.Count(u => u.RoleId == role.Id);
                if (count > 0)
                    stats.UsersByRole[role.Name] = count;
            }

            var faculties = await _context.Faculties.ToListAsync();
            foreach (var faculty in faculties)
            {
                var facultyStudents = students.Count(s => s.FacultyID == faculty.Id);

                var facultyName = LocalizationHelper.Get(faculty.Name, "en");

                var facultyInstructors = instructors.Count(i =>
                    i.Department != null &&
                    i.Department.DepartmentFaculties.Any(df => df.FacultyId == faculty.Id));

                var total = facultyStudents + facultyInstructors;

                if (total > 0)
                    stats.UsersByFaculty[facultyName] = total;
            }

            var statuses = Enum.GetValues(typeof(AcademicStatus)).Cast<AcademicStatus>();
            foreach (var status in statuses)
            {
                var count = students.Count(s => s.AcademicStatus == status);
                if (count > 0)
                    stats.UsersByStatus[status.ToString()] = count;
            }

            return stats;
        }

        #endregion

        #region Filter Helpers

        public async Task<IEnumerable<UserListDto>> GetUsersByRoleAsync(Guid roleId)
        {
            var filter = new UserFilterDto { RoleIds = new List<Guid> { roleId } };
            var result = await GetUsersAsync(filter);
            return result.Items;
        }

        public async Task<IEnumerable<UserListDto>> GetUsersByFacultyAsync(Guid facultyId)
        {
            var filter = new UserFilterDto { FacultyIds = new List<Guid> { facultyId } };
            var result = await GetUsersAsync(filter);
            return result.Items;
        }

        public async Task<IEnumerable<UserListDto>> GetUsersByDepartmentAsync(Guid departmentId)
        {
            var filter = new UserFilterDto { DepartmentIds = new List<Guid> { departmentId } };
            var result = await GetUsersAsync(filter);
            return result.Items;
        }

        public async Task<IEnumerable<UserListDto>> GetUsersByTypeAsync(string userType)
        {
            var filter = new UserFilterDto { UserTypes = new List<string> { userType } };
            var result = await GetUsersAsync(filter);
            return result.Items;
        }

        public async Task<IEnumerable<UserListDto>> GetInactiveUsersAsync()
        {
            var filter = new UserFilterDto { IsActive = false };
            var result = await GetUsersAsync(filter);
            return result.Items;
        }

        public async Task<IEnumerable<UserListDto>> GetPasswordExpiredUsersAsync()
        {
            var filter = new UserFilterDto { IsPasswordExpired = true };
            var result = await GetUsersAsync(filter);
            return result.Items;
        }

        #endregion

        #region Export Methods

        public async Task<byte[]> ExportUsersToExcelAsync(UserFilterDto filter = null)
        {
            var users = await GetUsersAsync(filter ?? new UserFilterDto { PageSize = 10000 });

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Users");

                var headers = new[] {
                    "User ID", "National ID", "Full Name", "Email", "Role",
                    "User Type", "Status", "Password Expiry", "Password Status"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                int row = 2;
                foreach (var user in users.Items)
                {
                    worksheet.Cell(row, 1).Value = user.Id.ToString();
                    worksheet.Cell(row, 2).Value = user.NationalId;
                    worksheet.Cell(row, 3).Value = user.FullName;
                    worksheet.Cell(row, 4).Value = user.Email;
                    worksheet.Cell(row, 5).Value = user.RoleName;
                    worksheet.Cell(row, 6).Value = user.UserType ?? "N/A";
                    worksheet.Cell(row, 7).Value = user.IsActive ? "Active" : "Inactive";
                    worksheet.Cell(row, 8).Value = user.PasswordExpiryDate.ToString("yyyy-MM-dd") ?? "N/A";
                    worksheet.Cell(row, 9).Value = user.IsPasswordExpired ? "Expired" : "Valid";

                    if (user.IsPasswordExpired)
                        worksheet.Cell(row, 9).Style.Font.FontColor = XLColor.Red;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<byte[]> ExportUsersToCsvAsync(UserFilterDto filter = null)
        {
            var users = await GetUsersAsync(filter ?? new UserFilterDto { PageSize = 10000 });

            var csv = new StringBuilder();
            csv.AppendLine("User ID,National ID,Full Name,Email,Role,User Type,Status,Password Expiry,Password Status");

            foreach (var user in users.Items)
            {
                var passwordStatus = user.IsPasswordExpired ? "Expired" : "Valid";
                var expiryDate = user.PasswordExpiryDate.ToString("yyyy-MM-dd") ?? "";

                csv.AppendLine($"\"{user.Id}\",\"{user.NationalId}\",\"{user.FullName}\",\"{user.Email}\",\"{user.RoleName}\",\"{user.UserType ?? ""}\",\"{(user.IsActive ? "Active" : "Inactive")}\",\"{expiryDate}\",\"{passwordStatus}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<string> GenerateUsersReportAsync(UserFilterDto filter = null)
        {
            var users = await GetUsersAsync(filter ?? new UserFilterDto());
            var stats = await GetUserStatisticsAsync();

            var report = new StringBuilder();

            report.AppendLine("=== User Management Report ===");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine("=== Summary ===");
            report.AppendLine($"Total Users: {stats.TotalUsers}");
            report.AppendLine($"Active Users: {stats.ActiveUsers}");
            report.AppendLine($"Inactive Users: {stats.InactiveUsers}");
            report.AppendLine($"Students: {stats.StudentsCount}");
            report.AppendLine($"Instructors: {stats.InstructorsCount}");
            report.AppendLine($"Admins: {stats.AdminsCount}");
            report.AppendLine($"Password Expired: {stats.PasswordExpiredCount}");
            report.AppendLine($"Users with Missing Info: {stats.UsersWithMissingInfo}");
            report.AppendLine();
            report.AppendLine("=== Users by Role ===");
            foreach (var kvp in stats.UsersByRole)
                report.AppendLine($"{kvp.Key}: {kvp.Value}");
            report.AppendLine();
            report.AppendLine("=== Users by Faculty ===");
            foreach (var kvp in stats.UsersByFaculty)
                report.AppendLine($"{kvp.Key}: {kvp.Value}");
            report.AppendLine();

            return report.ToString();
        }

        #endregion

        #region Private Helper Methods
        private IQueryable<User> BuildUserQuery(UserFilterDto filter)
        {
            var query = _context.Users
                .Include(u => u.UserRole)
                .Include(u => u.PersonalInfo)
                .Include(u => u.ContactInfo)
                .Where(u => !u.IsDeleted)
                .AsQueryable();

            if (filter == null)
                return query;

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(searchTerm) ||
                    //u.FullNameAr.ToLower().Contains(searchTerm) ||
                    u.NationalId.Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    (u.ContactInfo != null && u.ContactInfo.PhoneNumber != null &&
                     u.ContactInfo.PhoneNumber.Contains(searchTerm)));
            }

            if (filter.RoleIds != null && filter.RoleIds.Any())
            {
                query = query.Where(u => filter.RoleIds.Contains(u.RoleId));
            }

            if (filter.UserTypes != null && filter.UserTypes.Any())
            {
                if (filter.UserTypes.Count == 1 && filter.UserTypes.First().ToLower() == "student")
                {
                    query = query.Where(u => _context.Students.Any(s => s.UserId == u.Id));
                }
                else if (filter.UserTypes.Count == 1 && filter.UserTypes.First().ToLower() == "instructor")
                {
                    query = query.Where(u => _context.Staff.Any(i => i.UserId == u.Id));
                }
                else if (filter.UserTypes.Count == 1 && filter.UserTypes.First().ToLower() == "admin")
                {
                    query = query.Where(u =>
                        !_context.Students.Any(s => s.UserId == u.Id) &&
                        !_context.Staff.Any(i => i.UserId == u.Id));
                }
                else
                {
                    var userTypeList = filter.UserTypes.Select(t => t.ToLower()).ToList();
                    var studentCondition = userTypeList.Contains("student");
                    var instructorCondition = userTypeList.Contains("instructor");
                    var adminCondition = userTypeList.Contains("admin");

                    query = query.Where(u =>
                        (studentCondition && _context.Students.Any(s => s.UserId == u.Id)) ||
                        (instructorCondition && _context.Staff.Any(i => i.UserId == u.Id)) ||
                        (adminCondition && !_context.Students.Any(s => s.UserId == u.Id) &&
                                        !_context.Staff.Any(i => i.UserId == u.Id))
                    );
                }
            }
            else
            {
                if (filter.FacultyIds != null && filter.FacultyIds.Any())
                {
                    query = query.Where(u =>
                        _context.Students.Where(s => filter.FacultyIds.Contains(s.FacultyID))
                            .Select(s => s.UserId).Contains(u.Id) ||
                        _context.Staff.Where(i => i.Department != null &&
                            i.Department.DepartmentFaculties.Any(df => filter.FacultyIds.Contains(df.FacultyId)))
                            .Select(i => i.UserId).Contains(u.Id));
                }

                if (filter.DepartmentIds != null && filter.DepartmentIds.Any())
                {
                    query = query.Where(u =>
                        _context.Students.Where(s => filter.DepartmentIds.Contains(s.DepartmentId))
                            .Select(s => s.UserId).Contains(u.Id) ||
                        _context.Staff.Any(i => i.UserId == u.Id && i.DepartmentId.HasValue && filter.DepartmentIds.Contains(i.DepartmentId.Value)));
                }
            }

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            if (filter.IsDeleted.HasValue)
                query = query.Where(u => u.IsDeleted == filter.IsDeleted.Value);

            if (filter.IsPasswordExpired.HasValue)
            {
                if (filter.IsPasswordExpired.Value)
                    query = query.Where(u => u.PasswordExpiryDate < DateTime.UtcNow);
                else
                    query = query.Where(u => u.PasswordExpiryDate >= DateTime.UtcNow);
            }

            if (filter.AcademicStatus.HasValue)
            {
                query = query.Where(u =>
                    _context.Students.Any(s => s.UserId == u.Id && s.AcademicStatus == filter.AcademicStatus));
            }

            if (filter.CreatedFrom.HasValue)
                query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);

            return query;
        }

        private IQueryable<User> ApplySorting(IQueryable<User> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "fullname" => isDescending ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
                "email" => isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "nationalid" => isDescending ? query.OrderByDescending(u => u.NationalId) : query.OrderBy(u => u.NationalId),
                "role" => isDescending ? query.OrderByDescending(u => u.UserRole.Name) : query.OrderBy(u => u.UserRole.Name),
                "passwordexpiry" => isDescending ? query.OrderByDescending(u => u.PasswordExpiryDate) : query.OrderBy(u => u.PasswordExpiryDate),
                _ => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
            };
        }

        private async Task<UserDetailsDto> MapToUserDetailsDto(User user)
        {
            var dto = new UserDetailsDto
            {
                Id = user.Id,
                NationalId = user.NationalId,
                FullName = user.FullName,
                //FullNameAr = user.FullNameAr,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.UserRole?.Name,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                PasswordExpiryDate = user.PasswordExpiryDate,

                Gender = user.PersonalInfo?.Gender?.ToString(),
                BirthDate = user.PersonalInfo?.BirthDate,
                Religion = user.PersonalInfo?.Religion?.ToString(),
                Nationality = user.PersonalInfo?.Nationality?.ToString(),
                BirthPlace = user.PersonalInfo?.BirthPlace?.ToString(),

                Address = user.ContactInfo?.Address,
                City = user.ContactInfo?.City?.ToString(),
                PhoneNumber = user.ContactInfo?.PhoneNumber,
                AltEmail = user.ContactInfo?.AltEmail
            };

            var student = await _context.Students
                .Include(s => s.Faculty)
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student != null)
            {
                dto.UserType = "Student";
                dto.UniversityCode = student.UniversityCode;
                dto.UniversityEmail = student.UniversityEmail;
                dto.AcademicStatus = student.AcademicStatus.ToString();
                dto.Level = student.Level;
                dto.CGPA = student.Cgpa;
                dto.ProfileImage = student.ProfileImage;
                dto.FacultyId = student.FacultyID;
                //dto.FacultyName = student.Faculty?.NameEn;
                dto.FacultyName = student.Faculty?.Name.ToString();
                dto.DepartmentId = student.DepartmentId;
                dto.DepartmentName = student.Department?.DepartmentName;
            }
            else
            {
                var instructor = await _context.Staff
                    .Include(i => i.Department)
                        .ThenInclude(d => d.DepartmentFaculties)
                            .ThenInclude(df => df.Faculty)
                    .FirstOrDefaultAsync(i => i.UserId == user.Id);

                if (instructor != null)
                {
                    dto.UserType = "Instructor";
                    dto.AcademicTitle = instructor.Title.ToString();

                    var primaryFaculty = instructor.Department?.DepartmentFaculties
                        .FirstOrDefault(df => df.IsPrimary) ??
                        instructor.Department?.DepartmentFaculties.FirstOrDefault();

                    dto.FacultyId = primaryFaculty?.FacultyId;
                    //dto.FacultyName = primaryFaculty?.Faculty?.NameEn;
                    dto.FacultyName = primaryFaculty?.Faculty?.Name.ToString();
                    dto.DepartmentId = instructor.DepartmentId;
                    dto.DepartmentName = instructor.Department?.DepartmentName;
                }
                else
                {
                    dto.UserType = "Admin";
                }
            }

            return dto;
        }

        private static UserListDto MapToUserListDto(User user)
        {
            return new UserListDto
            {
                Id = user.Id,
                NationalId = user.NationalId,
                FullName = user.FullName,
                //FullNameAr = user.FullNameAr,
                Email = user.Email,
                UserType = user.UserRole?.Name,
                RoleName = user.UserRole?.Name,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,
                PasswordExpiryDate = user.PasswordExpiryDate
            };
        }

        private async Task<int> CountUsersWithMissingInfoAsync()
        {
            var count = 0;
            var users = await _context.Users
                .Include(u => u.PersonalInfo)
                .Include(u => u.ContactInfo)
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            foreach (var user in users)
            {
                var missingInfo = await GetMissingUserInfoAsync(user);
                if (missingInfo.Any())
                    count++;
            }

            return count;
        }

        private async Task<List<string>> GetMissingUserInfoAsync(User user)
        {
            var missingInfo = new List<string>();

            if (user.PersonalInfo == null)
            {
                missingInfo.Add("PersonalInfo");
            }
            else
            {
                if (string.IsNullOrEmpty(user.PersonalInfo.Religion?.ToString()))
                    missingInfo.Add("Religion");
                if (string.IsNullOrEmpty(user.PersonalInfo.Nationality?.ToString()))
                    missingInfo.Add("Nationality");
                if (string.IsNullOrEmpty(user.PersonalInfo.BirthPlace?.ToString()))
                    missingInfo.Add("BirthPlace");
                if (!user.PersonalInfo.BirthDate.HasValue)
                    missingInfo.Add("BirthDate");
                if (!user.PersonalInfo.Gender.HasValue)
                    missingInfo.Add("Gender");
            }

            if (user.ContactInfo == null)
            {
                missingInfo.Add("ContactInfo");
            }
            else
            {
                if (string.IsNullOrEmpty(user.ContactInfo.Address))
                    missingInfo.Add("Address");
                if (string.IsNullOrEmpty(user.ContactInfo.City?.ToString()))
                    missingInfo.Add("City");
                if (string.IsNullOrEmpty(user.ContactInfo.PhoneNumber))
                    missingInfo.Add("PhoneNumber");
            }

            return missingInfo;
        }

        private async Task<bool> CheckUserDependenciesAsync(Guid userId)
        {
            var isDean = await _context.Faculties.AnyAsync(f => f.DeanId == userId);
            if (isDean) return true;

            var isDepartmentHead = await _context.Departments.AnyAsync(d => d.HeadOfDepartmentId == userId);
            if (isDepartmentHead) return true;

            var hasCreatedRoles = await _context.Roles.AnyAsync(r => r.CreatedBy == userId);
            if (hasCreatedRoles) return true;

            return false;
        }

        private string GenerateUniversityCode()
        {
            return $"STU{DateTime.Now:yyyy}{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }

        private Gender? ParseGender(string? gender)
        {
            if (string.IsNullOrEmpty(gender))
                return null;

            return gender.ToLower() switch
            {
                "male" => Gender.Male,
                "female" => Gender.Female,
                _ => null
            };
        }

        private Religion? ParseReligion(string? religion)
        {
            if (string.IsNullOrEmpty(religion))
                return null;

            return religion.ToLower() switch
            {
                "muslim" => Religion.Muslim,
                "christian" => Religion.Christian,
                _ => null
            };
        }

        private Nationality? ParseNationality(string? nationality)
        {
            if (string.IsNullOrEmpty(nationality))
                return null;

            return nationality.ToLower() switch
            {
                "egyptian" => Nationality.Egyptian,
                _ => null
            };
        }

        private BirthPlace? ParseBirthPlace(string? birthPlace)
        {
            if (string.IsNullOrEmpty(birthPlace))
                return null;

            return birthPlace.ToLower() switch
            {
                "cairo" => BirthPlace.Cairo,
                _ => null
            };
        }

        private City? ParseCity(string? city)
        {
            if (string.IsNullOrEmpty(city))
                return null;

            return city.ToLower() switch
            {
                "cairo" => City.Cairo,
                _ => null
            };
        }

        private AcademicStatus? ParseAcademicStatus(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return null;

            return status.ToLower() switch
            {
                "active" => AcademicStatus.Active,
                "graduated" => AcademicStatus.Graduated,
                //"suspended" => AcademicStatus.Suspended,
                //"probation" => AcademicStatus.Probation,
                //"withdrawn" => AcademicStatus.Withdrawn,
                _ => null
            };
        }

        private StaffTitle? ParseAcademicTitle(string? title)
        {
            if (string.IsNullOrEmpty(title))
                return null;

            return title.ToLower() switch
            {
                "professor" => StaffTitle.Professor,
                "assistantprofessor" => StaffTitle.AssistantProfessor,
                //"associateprofessor" => StaffTitle.AssociateProfessor,
                "instructor" => StaffTitle.Instructor,
                //"lecturer" => StaffTitle.Lecturer,
                "teachingassistant" => StaffTitle.TeachingAssistant,
                //"demonstrator" => StaffTitle.Demonstrator,
                _ => null
            };
        }

        #endregion
    }
}