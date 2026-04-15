using ClosedXML.Excel;
using HUP.Application.DTOs.AcademicDtos.FacultyDtos;
using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.LookupDtos;
using HUP.Application.Services.Interfaces;
using HUP.Core.Entities.Academics;
using HUP.Core.Enums;
using HUP.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace HUP.Application.Services.Implementations
{
    public class FacultyManagementService : IFacultyManagementService
    {
        private readonly HupDbContext _context;
        private readonly ILogger<FacultyManagementService> _logger;

        public FacultyManagementService(
            HupDbContext context,
            ILogger<FacultyManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Query Methods

        public async Task<PaginatedResult<FacultyListDto>> GetFacultiesAsync(FacultyFilterDto? filter = null)
        {
            try
            {
                filter ??= new FacultyFilterDto();
                var query = BuildFacultyQuery(filter);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(f => new FacultyListDto
                    {
                        Id = f.Id,
                        Code = f.Code,
                        NameAr = GetLocalizedName(f.Name, "ar"),
                        NameEn = GetLocalizedName(f.Name, "en"),
                        //NameAr = f.NameAr,
                        //NameEn = f.NameEn,
                        DeanName = f.Dean != null ? f.Dean.FullName : null,
                        DeanId = f.DeanId,
                        ContactInfo = f.ContactInfo,
                        DepartmentCount = f.DepartmentFaculties.Count(df => !df.Department.IsDeleted),
                        StudentCount = f.Students != null
                            ? f.Students.Count(s => s.User != null && !s.User.IsDeleted)
                            : 0,
                        IsActive = f.IsActive,
                        CreatedAt = f.CreatedAt
                    })
                    .ToListAsync();

                return new PaginatedResult<FacultyListDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFacultiesAsync");
                throw;
            }
        }

        public async Task<FacultyDetailsDto?> GetFacultyByIdAsync(Guid facultyId)
        {
            try
            {
                var faculty = await _context.Faculties
                    .Include(f => f.Dean)
                    .Include(f => f.DepartmentFaculties)
                        .ThenInclude(df => df.Department)
                            .ThenInclude(d => d.HeadOfDepartment)
                                .ThenInclude(h => h.User)
                    .Include(f => f.Students!)
                        .ThenInclude(s => s.User)
                    .FirstOrDefaultAsync(f => f.Id == facultyId && !f.IsDeleted);

                if (faculty == null)
                    return null;

                var instructorCount = await _context.Staff
                    .CountAsync(i => i.Department != null &&
                                     i.Department.DepartmentFaculties.Any(df => df.FacultyId == facultyId) &&
                                     i.User != null &&
                                     !i.User.IsDeleted);

                var courseCount = await _context.CourseOfferings
                    .CountAsync(c => c.Department != null &&
                                     c.Department.DepartmentFaculties.Any(df => df.FacultyId == facultyId));

                var studentCount = await _context.Students
                    .CountAsync(s => s.FacultyID == facultyId && !s.User.IsDeleted);

                var dto = new FacultyDetailsDto
                {
                    Id = faculty.Id,
                    Code = faculty.Code,
                    NameAr = GetLocalizedName(faculty.Name, "ar"),
                    NameEn = GetLocalizedName(faculty.Name, "en"),
                    //NameAr = faculty.NameAr,
                    //NameEn = faculty.NameEn,
                    DeanName = faculty.Dean?.FullName,
                    DeanId = faculty.DeanId,
                    ContactInfo = faculty.ContactInfo,
                    DepartmentCount = faculty.DepartmentFaculties.Count(df => !df.Department.IsDeleted),
                    StudentCount = faculty.Students?.Count(s => s.User != null && !s.User.IsDeleted) ?? 0,
                    InstructorCount = instructorCount,
                    CourseCount = courseCount,
                    IsActive = faculty.IsActive,
                    CreatedAt = faculty.CreatedAt,
                    UpdatedAt = faculty.UpdatedAt,
                    Departments = faculty.DepartmentFaculties
                        .Where(df => !df.Department.IsDeleted)
                        .Select(df => new FacultyDepartmentDto
                        {
                            Id = df.Department.Id,
                            Name = df.Department.DepartmentName,
                            Code = df.DepartmentCode,
                            HeadOfDepartment = df.Department.HeadOfDepartment != null
                                ? df.Department.HeadOfDepartment.User?.FullName
                                : null,
                            StudentCount = _context.Students.Count(s => s.DepartmentId == df.DepartmentId && !s.User!.IsDeleted),
                            InstructorCount = _context.Staff.Count(i => i.DepartmentId == df.DepartmentId && !i.User!.IsDeleted),
                            IsActive = df.Department.IsActive
                        }).ToList()
                };

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetFacultyByIdAsync for ID {facultyId}");
                throw;
            }
        }

        public async Task<IEnumerable<LookupDto>> GetFacultiesLookupAsync()
        {
            return await _context.Faculties
                .Where(f => f.IsActive && !f.IsDeleted)
                .Select(f => new LookupDto
                {
                    Id = f.Id,
                    Name = GetLocalizedName(f.Name, "en"),
                    DisplayName = GetLocalizedName(f.Name, "ar")
                    //Name = f.NameEn,
                    //DisplayName = f.NameAr
                })
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        #endregion

        #region Create Methods

        public async Task<FacultyActionResponse> CreateFacultyAsync(CreateFacultyDto dto)
        {
            try
            {
                FacultyTitle? facultyTitle = null;

                if (!string.IsNullOrWhiteSpace(dto.Name))
                {
                    if (Enum.TryParse<FacultyTitle>(dto.Name, true, out var parsedTitle))
                    {
                        facultyTitle = parsedTitle;
                    }
                }

                if (facultyTitle == null)
                {
                    return new FacultyActionResponse
                    {
                        Success = false,
                        Message = $"Invalid faculty name. Please provide a valid faculty name from the system list."
                    };
                }

                var existsByCode = await _context.Faculties
                    .AnyAsync(f => f.Code == dto.Code && !f.IsDeleted);

                if (existsByCode)
                {
                    return new FacultyActionResponse
                    {
                        Success = false,
                        Message = $"Faculty with code '{dto.Code}' already exists"
                    };
                }

                var existsByName = await _context.Faculties
                    .AnyAsync(f => f.Name == facultyTitle && !f.IsDeleted);
                //.AnyAsync(f => (GetLocalizedName(f.Name, "ar") == dto.NameAr || GetLocalizedName(f.Name, "en") == dto.NameEn) && !f.IsDeleted);

                if (existsByName)
                {
                    return new FacultyActionResponse
                    {
                        Success = false,
                        Message = "Faculty with this name already exists"
                    };
                }

                if (dto.DeanId.HasValue)
                {
                    var deanExists = await _context.Staff
                        .AnyAsync(i => i.UserId == dto.DeanId.Value && !i.User!.IsDeleted);

                    if (!deanExists)
                    {
                        return new FacultyActionResponse
                        {
                            Success = false,
                            Message = "Selected dean not found"
                        };
                    }

                    var isDeanElsewhere = await _context.Faculties
                        .AnyAsync(f => f.DeanId == dto.DeanId.Value && !f.IsDeleted);

                    if (isDeanElsewhere)
                    {
                        return new FacultyActionResponse
                        {
                            Success = false,
                            Message = "This instructor is already a dean of another faculty"
                        };
                    }
                }

                var faculty = new Faculty
                {
                    Id = Guid.NewGuid(),
                    Code = dto.Code,
                    Name = facultyTitle.Value,
                    DisplayName = dto.DisplayName ?? GetLocalizedName(facultyTitle.Value, "en"),
                    //NameAr = dto.NameAr,
                    //NameEn = dto.NameEn,
                    DeanId = dto.DeanId,
                    ContactInfo = dto.ContactInfo,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    DepartmentFaculties = new List<DepartmentFaculty>()
                };

                await _context.Faculties.AddAsync(faculty);
                await _context.SaveChangesAsync();

                //await LogFacultyActivityAsync(faculty.Id, "CREATE_FACULTY",
                //    $"Faculty '{faculty.NameAr}' created");

                return new FacultyActionResponse
                {
                    Success = true,
                    Message = "Faculty created successfully",
                    Data = new
                    {
                        FacultyId = faculty.Id,
                        Code = faculty.Code,
                        //Name = faculty.NameAr
                        Name = faculty.Name.ToString()

                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating faculty");
                return new FacultyActionResponse
                {
                    Success = false,
                    Message = $"Error creating faculty: {ex.Message}"
                };
            }
        }

        #endregion

        #region Update Methods

        public async Task<FacultyActionResponse> UpdateFacultyAsync(Guid facultyId, UpdateFacultyDto dto)
        {
            try
            {
                var faculty = await _context.Faculties
                    .FirstOrDefaultAsync(f => f.Id == facultyId && !f.IsDeleted);

                if (faculty == null)
                {
                    return new FacultyActionResponse
                    {
                        Success = false,
                        Message = "Faculty not found"
                    };
                }

                if (!string.IsNullOrWhiteSpace(dto.Code) && dto.Code != faculty.Code)
                {
                    var exists = await _context.Faculties
                        .AnyAsync(f => f.Code == dto.Code && f.Id != facultyId && !f.IsDeleted);

                    if (exists)
                    {
                        return new FacultyActionResponse
                        {
                            Success = false,
                            Message = $"Faculty with code '{dto.Code}' already exists"
                        };
                    }
                    faculty.Code = dto.Code;
                }

                if (!string.IsNullOrWhiteSpace(dto.Name))
                {
                    if (Enum.TryParse<FacultyTitle>(dto.Name, true, out var newTitle))
                    {
                        var exists = await _context.Faculties
                            .AnyAsync(f => f.Name == newTitle && f.Id != facultyId && !f.IsDeleted);

                        if (exists)
                        {
                            return new FacultyActionResponse
                            {
                                Success = false,
                                Message = $"Faculty with name '{dto.Name}' already exists"
                            };
                        }
                        faculty.Name = newTitle;
                    }
                }

                if (!string.IsNullOrWhiteSpace(dto.DisplayName))
                {
                    faculty.DisplayName = dto.DisplayName;
                }


                //if (!string.IsNullOrWhiteSpace(dto.NameAr) && dto.NameAr != faculty.NameAr)
                //{
                //    var exists = await _context.Faculties
                //        .AnyAsync(f => f.NameAr == dto.NameAr && f.Id != facultyId && !f.IsDeleted);

                //    if (exists)
                //    {
                //        return new FacultyActionResponse
                //        {
                //            Success = false,
                //            Message = $"Faculty with Arabic name '{dto.NameAr}' already exists"
                //        };
                //    }
                //    faculty.NameAr = dto.NameAr;
                //}

                //if (!string.IsNullOrWhiteSpace(dto.NameEn) && dto.NameEn != faculty.NameEn)
                //{
                //    var exists = await _context.Faculties
                //        .AnyAsync(f => f.NameEn == dto.NameEn && f.Id != facultyId && !f.IsDeleted);

                //    if (exists)
                //    {
                //        return new FacultyActionResponse
                //        {
                //            Success = false,
                //            Message = $"Faculty with English name '{dto.NameEn}' already exists"
                //        };
                //    }
                //    faculty.NameEn = dto.NameEn;
                //}

                if (dto.DeanId.HasValue && dto.DeanId.Value != faculty.DeanId)
                {
                    var newDean = await _context.Staff
                        .Include(i => i.User)
                        .FirstOrDefaultAsync(i => i.UserId == dto.DeanId.Value && !i.User!.IsDeleted);

                    if (newDean == null)
                    {
                        return new FacultyActionResponse
                        {
                            Success = false,
                            Message = "Selected dean not found"
                        };
                    }

                    var isDeanElsewhere = await _context.Faculties
                        .AnyAsync(f => f.DeanId == dto.DeanId.Value && f.Id != facultyId && !f.IsDeleted);

                    if (isDeanElsewhere)
                    {
                        return new FacultyActionResponse
                        {
                            Success = false,
                            Message = "This instructor is already a dean of another faculty"
                        };
                    }

                    faculty.DeanId = dto.DeanId.Value;
                }
                else if (dto.DeanId == null)
                {
                    faculty.DeanId = null;
                }

                if (dto.ContactInfo != null)
                    faculty.ContactInfo = dto.ContactInfo;

                if (dto.IsActive.HasValue)
                    faculty.IsActive = dto.IsActive.Value;

                faculty.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new FacultyActionResponse
                {
                    Success = true,
                    Message = "Faculty updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating faculty {facultyId}");
                return new FacultyActionResponse
                {
                    Success = false,
                    Message = $"Error updating faculty: {ex.Message}"
                };
            }
        }

        #endregion

        #region Dean Management

        public async Task<FacultyActionResponse> AssignDeanAsync(Guid facultyId, Guid instructorId)
        {
            try
            {
                var faculty = await _context.Faculties
                    .FirstOrDefaultAsync(f => f.Id == facultyId && !f.IsDeleted);

                if (faculty == null)
                {
                    return new FacultyActionResponse
                    {
                        Success = false,
                        Message = "Faculty not found"
                    };
                }

                var instructor = await _context.Staff
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.UserId == instructorId && !i.User!.IsDeleted);

                if (instructor == null)
                {
                    return new FacultyActionResponse
                    {
                        Success = false,
                        Message = "Instructor not found"
                    };
                }

                var isDeanElsewhere = await _context.Faculties
                    .AnyAsync(f => f.DeanId == instructorId && f.Id != facultyId && !f.IsDeleted);

                if (isDeanElsewhere)
                {
                    return new FacultyActionResponse
                    {
                        Success = false,
                        Message = "This instructor is already a dean of another faculty"
                    };
                }

                faculty.DeanId = instructorId;
                faculty.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new FacultyActionResponse
                {
                    Success = true,
                    Message = "Dean assigned successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning dean to faculty {facultyId}");
                return new FacultyActionResponse
                {
                    Success = false,
                    Message = $"Error assigning dean: {ex.Message}"
                };
            }
        }

        public async Task<FacultyActionResponse> RemoveDeanAsync(Guid facultyId)
        {
            try
            {
                var faculty = await _context.Faculties
                    .FirstOrDefaultAsync(f => f.Id == facultyId && !f.IsDeleted);

                if (faculty == null)
                {
                    return new FacultyActionResponse
                    {
                        Success = false,
                        Message = "Faculty not found"
                    };
                }

                if (!faculty.DeanId.HasValue)
                {
                    return new FacultyActionResponse
                    {
                        Success = false,
                        Message = "Faculty has no dean assigned"
                    };
                }

                faculty.DeanId = null;
                faculty.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new FacultyActionResponse
                {
                    Success = true,
                    Message = "Dean removed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing dean from faculty {facultyId}");
                return new FacultyActionResponse
                {
                    Success = false,
                    Message = $"Error removing dean: {ex.Message}"
                };
            }
        }

        #endregion

        #region Status Change Methods

        public async Task<FacultyActionResponse> ActivateFacultyAsync(Guid facultyId)
        {
            var faculty = await _context.Faculties.FindAsync(facultyId);
            if (faculty == null)
                return new FacultyActionResponse { Success = false, Message = "Faculty not found" };

            if (faculty.IsActive)
                return new FacultyActionResponse { Success = false, Message = "Faculty is already active" };

            faculty.IsActive = true;
            faculty.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new FacultyActionResponse { Success = true, Message = "Faculty activated successfully" };
        }

        public async Task<FacultyActionResponse> DeactivateFacultyAsync(Guid facultyId)
        {
            var faculty = await _context.Faculties.FindAsync(facultyId);
            if (faculty == null)
                return new FacultyActionResponse { Success = false, Message = "Faculty not found" };

            if (!faculty.IsActive)
                return new FacultyActionResponse { Success = false, Message = "Faculty is already inactive" };

            faculty.IsActive = false;
            faculty.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new FacultyActionResponse { Success = true, Message = "Faculty deactivated successfully" };
        }

        public async Task<FacultyActionResponse> SoftDeleteFacultyAsync(Guid facultyId)
        {
            var hasDepartments = await _context.DepartmentFaculties
                .AnyAsync(df => df.FacultyId == facultyId && !df.Department.IsDeleted);

            if (hasDepartments)
            {
                return new FacultyActionResponse
                {
                    Success = false,
                    Message = "Cannot delete faculty with existing departments"
                };
            }

            var faculty = await _context.Faculties.FindAsync(facultyId);
            if (faculty == null)
                return new FacultyActionResponse { Success = false, Message = "Faculty not found" };

            if (faculty.IsDeleted)
                return new FacultyActionResponse { Success = false, Message = "Faculty is already deleted" };

            faculty.IsDeleted = true;
            faculty.IsActive = false;
            faculty.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new FacultyActionResponse { Success = true, Message = "Faculty deleted successfully" };
        }

        public async Task<FacultyActionResponse> RestoreFacultyAsync(Guid facultyId)
        {
            var faculty = await _context.Faculties.FindAsync(facultyId);
            if (faculty == null)
                return new FacultyActionResponse { Success = false, Message = "Faculty not found" };

            if (!faculty.IsDeleted)
                return new FacultyActionResponse { Success = false, Message = "Faculty is not deleted" };

            faculty.IsDeleted = false;
            faculty.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new FacultyActionResponse { Success = true, Message = "Faculty restored successfully" };
        }

        #endregion

        #region Search

        public async Task<IEnumerable<FacultyListDto>> SearchFacultiesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<FacultyListDto>();

            var faculties = await _context.Faculties
                .Include(f => f.Dean)
                .Where(f => !f.IsDeleted && (
                    f.Code.Contains(searchTerm) ||
                    f.Name.ToString().Contains(searchTerm) ||
                    f.DisplayName.Contains(searchTerm) ||
                    //f.NameAr.Contains(searchTerm) ||
                    //f.NameEn.Contains(searchTerm) ||
                    (f.Dean != null && f.Dean.FullName.Contains(searchTerm))))
                .Take(20)
                .Select(f => new FacultyListDto
                {
                    Id = f.Id,
                    Code = f.Code,
                    NameAr = GetLocalizedName(f.Name, "ar"),
                    NameEn = GetLocalizedName(f.Name, "en"),
                    //NameAr = f.NameAr,
                    //NameEn = f.NameEn,
                    DeanName = f.Dean != null ? f.Dean.FullName : null,
                    ContactInfo = f.ContactInfo,
                    DepartmentCount = f.DepartmentFaculties.Count(df => !df.Department.IsDeleted),
                    StudentCount = f.Students != null
                        ? f.Students.Count(s => s.User != null && !s.User.IsDeleted)
                        : 0,
                    IsActive = f.IsActive,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            return faculties;
        }

        #endregion

        #region Statistics

        public async Task<FacultyStatisticsDto> GetFacultyStatisticsAsync()
        {
            var faculties = await _context.Faculties
                .Include(f => f.DepartmentFaculties)
                    .ThenInclude(df => df.Department)
                .Include(f => f.Students)
                    .ThenInclude(s => s.User)
                .Where(f => !f.IsDeleted)
                .ToListAsync();

            var totalStudents = faculties
                .Sum(f => f.Students?.Count(s => !s.User!.IsDeleted) ?? 0);

            var stats = new FacultyStatisticsDto
            {
                TotalFaculties = faculties.Count,
                ActiveFaculties = faculties.Count(f => f.IsActive),
                InactiveFaculties = faculties.Count(f => !f.IsActive),
                FacultiesWithDean = faculties.Count(f => f.DeanId.HasValue),
                FacultiesWithoutDean = faculties.Count(f => !f.DeanId.HasValue),
                TotalDepartments = faculties.Sum(f => f.DepartmentFaculties.Count(df => !df.Department.IsDeleted)),
                TotalStudents = totalStudents,
                GeneratedAt = DateTime.UtcNow
            };

            stats.TotalInstructors = await _context.Staff
                .CountAsync(i => !i.User!.IsDeleted);

            stats.FacultiesByType = faculties
                .ToDictionary(f => f.Name.ToString(), f => 1);
            //.ToDictionary(f => f.NameAr, f => 1);

            stats.TopFacultiesByStudents = faculties
                .Where(f => f.Students?.Count(s => !s.User!.IsDeleted) > 0)
                .OrderByDescending(f => f.Students?.Count(s => !s.User!.IsDeleted))
                .Take(5)
                .ToDictionary(
                    f => f.Name.ToString(),
                    //f => f.NameAr,
                    f => f.Students?.Count(s => !s.User!.IsDeleted) ?? 0
                );

            stats.TopFacultiesByDepartments = faculties
                .Where(f => f.DepartmentFaculties.Count(df => !df.Department.IsDeleted) > 0)
                .OrderByDescending(f => f.DepartmentFaculties.Count(df => !df.Department.IsDeleted))
                .Take(5)
                .ToDictionary(
                    f => f.Name.ToString(),
                    //f => f.NameAr,
                    f => f.DepartmentFaculties.Count(df => !df.Department.IsDeleted)
                );

            return stats;
        }

        #endregion

        #region Export Methods

        public async Task<byte[]> ExportFacultiesToExcelAsync(FacultyFilterDto? filter = null)
        {
            var faculties = await GetFacultiesAsync(filter ?? new FacultyFilterDto { PageSize = 1000 });

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Faculties");

                var headers = new[] {
                    "ID", "Code", "Arabic Name", "English Name", "Dean", "Contact Info", "Departments",
                    "Students", "Status", "Created Date"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                int row = 2;
                foreach (var faculty in faculties.Items)
                {
                    worksheet.Cell(row, 1).Value = faculty.Id.ToString();
                    worksheet.Cell(row, 2).Value = faculty.Code;
                    worksheet.Cell(row, 3).Value = faculty.NameAr;
                    worksheet.Cell(row, 4).Value = faculty.NameEn;
                    worksheet.Cell(row, 5).Value = faculty.DeanName ?? "Not Assigned";
                    worksheet.Cell(row, 6).Value = faculty.ContactInfo;
                    worksheet.Cell(row, 7).Value = faculty.DepartmentCount;
                    worksheet.Cell(row, 8).Value = faculty.StudentCount;
                    worksheet.Cell(row, 9).Value = faculty.IsActive ? "Active" : "Inactive";
                    worksheet.Cell(row, 10).Value = faculty.CreatedAt.ToString("yyyy-MM-dd");
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

        public async Task<byte[]> ExportFacultiesToCsvAsync(FacultyFilterDto? filter = null)
        {
            var faculties = await GetFacultiesAsync(filter ?? new FacultyFilterDto { PageSize = 1000 });

            var csv = new StringBuilder();
            csv.AppendLine("ID,Code,Arabic Name,English Name,Dean,Contact Info,Departments,Students,Status,Created Date");

            foreach (var faculty in faculties.Items)
            {
                csv.AppendLine($"\"{faculty.Id}\",\"{faculty.Code}\",\"{faculty.NameAr}\",\"{faculty.NameEn}\",\"{faculty.DeanName}\",\"{faculty.ContactInfo}\",{faculty.DepartmentCount},{faculty.StudentCount},\"{(faculty.IsActive ? "Active" : "Inactive")}\",\"{faculty.CreatedAt:yyyy-MM-dd}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        #endregion

        #region Private Helper Methods

        private IQueryable<Faculty> BuildFacultyQuery(FacultyFilterDto filter)
        {
            var query = _context.Faculties
                .Include(f => f.Dean)
                .Include(f => f.DepartmentFaculties)
                    .ThenInclude(df => df.Department)
                .Include(f => f.Students)
                .Where(f => !f.IsDeleted)
                .AsQueryable();

            if (filter == null)
                return query;

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(f =>
                    f.Code.ToLower().Contains(searchTerm) ||
                    f.Name.ToString().ToLower().Contains(searchTerm) ||
                    f.DisplayName.ToLower().Contains(searchTerm) ||
                    //f.NameAr.ToLower().Contains(searchTerm) ||
                    //f.NameEn.ToLower().Contains(searchTerm) ||
                    (f.Dean != null && f.Dean.FullName.ToLower().Contains(searchTerm)));
            }

            if (filter.IsActive.HasValue)
                query = query.Where(f => f.IsActive == filter.IsActive.Value);

            if (filter.HasDean.HasValue)
            {
                if (filter.HasDean.Value)
                    query = query.Where(f => f.DeanId != null);
                else
                    query = query.Where(f => f.DeanId == null);
            }

            if (filter.CreatedFrom.HasValue)
                query = query.Where(f => f.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(f => f.CreatedAt <= filter.CreatedTo.Value);

            query = filter.SortBy?.ToLower() switch
            {
                "code" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(f => f.Code)
                    : query.OrderBy(f => f.Code),
                "name" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(f => f.Name)
                    : query.OrderBy(f => f.Name),
                //"namear" => filter.SortOrder == "desc"
                //    ? query.OrderByDescending(f => f.NameAr)
                //    : query.OrderBy(f => f.NameAr),
                //"nameen" => filter.SortOrder == "desc"
                //    ? query.OrderByDescending(f => f.NameEn)
                //    : query.OrderBy(f => f.NameEn),
                "dean" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(f => f.Dean != null ? f.Dean.FullName : "")
                    : query.OrderBy(f => f.Dean != null ? f.Dean.FullName : ""),
                "departments" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(f => f.DepartmentFaculties.Count(df => !df.Department.IsDeleted))
                    : query.OrderBy(f => f.DepartmentFaculties.Count(df => !df.Department.IsDeleted)),
                "students" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(f => f.Students != null ? f.Students.Count(s => !s.User!.IsDeleted) : 0)
                    : query.OrderBy(f => f.Students != null ? f.Students.Count(s => !s.User!.IsDeleted) : 0),
                "createdat" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(f => f.CreatedAt)
                    : query.OrderBy(f => f.CreatedAt),
                _ => filter.SortOrder == "desc"
                    ? query.OrderByDescending(f => f.CreatedAt)
                    : query.OrderBy(f => f.CreatedAt)
            };

            return query;
        }

        private string GetLocalizedName(FacultyTitle title, string language)
        {
            return language.ToLower() switch
            {
                "ar" => title switch
                {
                    FacultyTitle.FacultyOfArts => "كلية الآداب",
                    FacultyTitle.FacultyOfHomeEconomics => "كلية الاقتصاد المنزلي",
                    FacultyTitle.FacultyOfEducation => "كلية التربية",
                    FacultyTitle.FacultyOfNursing => "كلية التمريض",
                    FacultyTitle.FacultyOfComputingAndAI => "كلية الحاسبات والذكاء الاصطناعي",
                    FacultyTitle.FacultyOfSocialWork => "كلية الخدمة الاجتماعية",
                    FacultyTitle.FacultyOfPharmacy => "كلية الصيدلة",
                    FacultyTitle.FacultyOfMedicine => "كلية الطب",
                    FacultyTitle.FacultyOfScience => "كلية العلوم",
                    FacultyTitle.FacultyOfAppliedArts => "كلية الفنون التطبيقية",
                    FacultyTitle.FacultyOfFineArts => "كلية الفنون الجميلة",
                    FacultyTitle.FacultyOfSportsScienceBoys => "كلية علوم الرياضة بنين",
                    FacultyTitle.FacultyOfSportsScienceGirls => "كلية علوم الرياضة بنات",
                    FacultyTitle.FacultyOfEngineeringMataria => "كلية الهندسة (مطريه)",
                    FacultyTitle.FacultyOfEngineering => "كلية الهندسة (حلوان)",
                    FacultyTitle.FacultyOfCommerceAndBusinessAdministration => "كلية التجارة وإدارة الأعمال",
                    FacultyTitle.FacultyOfArtEducation => "كلية التربية الفنية",
                    FacultyTitle.TechnicalInstituteOfNursing => "معهد التمريض",
                    FacultyTitle.FacultyOfTechnologyAndEducation => "كلية التكنولوجيا والتعليم",
                    FacultyTitle.FacultyOfLaw => "كلية الحقوق",
                    FacultyTitle.FacultyOfMusicEducation => "كلية التربية الموسيقية",
                    FacultyTitle.FacultyOfTourismAndHotels => "كلية السياحة والفنادق",
                    FacultyTitle.FacultyOfNutritionScience => "كلية علوم التغذية",
                    _ => title.ToString()
                },
                _ => title.ToString()
            };
        }

        private FacultyTitle? GetFacultyTitleFromDisplayName(string displayName)
        {
            var mapping = new Dictionary<string, FacultyTitle>
            {
                ["كلية الآداب"] = FacultyTitle.FacultyOfArts,
                ["كلية الاقتصاد المنزلي"] = FacultyTitle.FacultyOfHomeEconomics,
                ["كلية التربية"] = FacultyTitle.FacultyOfEducation,
                ["كلية التمريض"] = FacultyTitle.FacultyOfNursing,
                ["كلية الحاسبات والذكاء الاصطناعي"] = FacultyTitle.FacultyOfComputingAndAI,
                ["كلية الخدمة الاجتماعية"] = FacultyTitle.FacultyOfSocialWork,
                ["كلية الصيدلة"] = FacultyTitle.FacultyOfPharmacy,
                ["كلية الطب"] = FacultyTitle.FacultyOfMedicine,
                ["كلية العلوم"] = FacultyTitle.FacultyOfScience,
                ["كلية الفنون التطبيقية"] = FacultyTitle.FacultyOfAppliedArts,
                ["كلية الفنون الجميلة"] = FacultyTitle.FacultyOfFineArts,
                ["كلية علوم الرياضة بنين"] = FacultyTitle.FacultyOfSportsScienceBoys,
                ["كلية علوم الرياضة بنات"] = FacultyTitle.FacultyOfSportsScienceGirls,
                ["كلية الهندسة (مطريه)"] = FacultyTitle.FacultyOfEngineeringMataria,
                ["كلية الهندسة (حلوان)"] = FacultyTitle.FacultyOfEngineering,
                ["كلية التجارة وإدارة الأعمال"] = FacultyTitle.FacultyOfCommerceAndBusinessAdministration,
                ["كلية التربية الفنية"] = FacultyTitle.FacultyOfArtEducation,
                ["معهد التمريض"] = FacultyTitle.TechnicalInstituteOfNursing,
                ["كلية التكنولوجيا والتعليم"] = FacultyTitle.FacultyOfTechnologyAndEducation,
                ["كلية الحقوق"] = FacultyTitle.FacultyOfLaw,
                ["كلية التربية الموسيقية"] = FacultyTitle.FacultyOfMusicEducation,
                ["كلية السياحة والفنادق"] = FacultyTitle.FacultyOfTourismAndHotels,
                ["كلية علوم التغذية"] = FacultyTitle.FacultyOfNutritionScience
            };

            return mapping.GetValueOrDefault(displayName);
        }

        #endregion
    }
}
