using ClosedXML.Excel;
using HUP.Application.DTOs.AcademicDtos.CourseDtos;
using HUP.Application.DTOs.AcademicDtos.Shared;
using HUP.Application.DTOs.LookupDtos;
using HUP.Application.Services.Interfaces;
using HUP.Core.Entities.Academics;
using HUP.Core.Enums;
using HUP.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HUP.Application.Services.Implementations
{
    public class CourseManagementService : ICourseManagementService
    {
        private readonly HupDbContext _context;
        private readonly ILogger<CourseManagementService> _logger;

        public CourseManagementService(
            HupDbContext context,
            ILogger<CourseManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Query Methods

        public async Task<PaginatedResult<CourseListDto>> GetCoursesAsync(CourseFilterDto? filter = null)
        {
            try
            {
                filter ??= new CourseFilterDto();
                var query = BuildCourseQuery(filter);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(c => new CourseListDto
                    {
                        Id = c.Id,
                        CourseCode = c.CourseCode,
                        CourseName = c.CourseName,
                        //CourseNameAr = c.CourseNameAr,
                        Credits = c.Credits,
                        PrerequisiteName = c.Prerequisite != null ? c.Prerequisite.CourseName : null,
                        PrerequisiteId = c.PrerequisiteId,
                        CourseType = c.CourseType.ToString(),
                        TotalOfferings = _context.CourseOfferings.Count(co => co.CourseId == c.Id),
                        TotalEnrollments = _context.Enrollments.Count(e => e.CourseOffering.CourseId == c.Id),
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return new PaginatedResult<CourseListDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCoursesAsync");
                throw;
            }
        }

        public async Task<CourseDetailsDto> GetCourseByIdAsync(Guid courseId)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Prerequisite)
                    .Include(c => c.Programs)
                        .ThenInclude(p => p.Department)
                            .ThenInclude(d => d.DepartmentFaculties)
                                .ThenInclude(df => df.Faculty)
                    .Include(c => c.CourseOfferings)
                        .ThenInclude(co => co.Semester)
                    .Include(c => c.CourseOfferings)
                        .ThenInclude(co => co.Department)
                    .Include(c => c.CourseOfferings)
                        .ThenInclude(co => co.Enrollments)
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

                if (course == null)
                    return null;

                var prerequisiteChain = await GetPrerequisiteChainInternalAsync(courseId);

                var recentOfferings = course.CourseOfferings
                    .Where(co => co.IsActive)
                    .OrderByDescending(co => co.Semester != null ? co.Semester.StartDate : DateTime.MinValue)
                    .Take(5)
                    .Select(co => new CourseOfferingSummaryDto
                    {
                        OfferingId = co.Id,
                        SemesterName = co.Semester?.SemesterName ?? "Unknown",
                        DepartmentName = co.Department?.DepartmentName ?? "Unknown",
                        MaxStudents = co.Schedules.Sum(s => s.TotalSeats),
                        //MaxStudents = co.MaxStudents,
                        EnrolledStudents = co.Enrollments?.Count ?? 0,
                        Status = co.IsActive ? "Active" : "Inactive"
                    })
                    .ToList();

                var departments = course.Programs
                    .Where(p => p.Department != null && !p.Department.IsDeleted)
                    .SelectMany(p => p.Department.DepartmentFaculties
                        .Where(df => df.Faculty != null && !df.Faculty.IsDeleted)
                        .Select(df => new CourseDepartmentDto
                        {
                            DepartmentId = p.DepartmentId,
                            DepartmentName = p.Department.DepartmentName,
                            FacultyId = df.FacultyId,
                            //FacultyName = df.Faculty.NameEn,
                            FacultyName = df.Faculty.Name.ToString(),
                            FacultyCode = df.Faculty.Code,
                            DepartmentCode = df.DepartmentCode,
                            IsCompulsory = p.IsCompulsory,
                            IsPrimary = df.IsPrimary
                        }))
                    .GroupBy(d => d.DepartmentId)
                    .Select(g => g.First())
                    .ToList();

                var dto = new CourseDetailsDto
                {
                    Id = course.Id,
                    CourseCode = course.CourseCode,
                    CourseName = course.CourseName,
                    //CourseNameAr = course.CourseNameAr,
                    //Description = course.Description,
                    Credits = course.Credits,
                    PrerequisiteName = course.Prerequisite?.CourseName,
                    PrerequisiteId = course.PrerequisiteId,
                    CourseType = course.CourseType.ToString(),
                    TotalOfferings = course.CourseOfferings?.Count ?? 0,
                    TotalEnrollments = _context.Enrollments.Count(e => e.CourseOffering.CourseId == course.Id),
                    IsActive = course.IsActive,
                    CreatedAt = course.CreatedAt,
                    UpdatedAt = course.UpdatedAt,
                    Departments = departments,
                    PrerequisiteChain = prerequisiteChain.ToList(),
                    RecentOfferings = recentOfferings
                };

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetCourseByIdAsync for ID {courseId}");
                throw;
            }
        }

        public async Task<IEnumerable<LookupDto>> GetCoursesByDepartmentLookupAsync(Guid departmentId)
        {
            return await _context.ProgramPlan
                .Where(p => p.DepartmentId == departmentId && p.Course.IsActive && !p.Course.IsDeleted)
                .OrderBy(p => p.Course.CourseName)
                .ThenBy(p => p.Course.CourseCode)
                .Select(p => new LookupDto
                {
                    Id = p.CourseId,
                    Name = $"{p.Course.CourseCode} - {p.Course.CourseName}"
                })
                .ToListAsync();
        }

        #endregion

        #region Create Methods

        public async Task<CourseActionResponse> CreateCourseAsync(CreateCourseDto dto)
        {
            try
            {
                var exists = await _context.Courses
                    .AnyAsync(c => (c.CourseCode == dto.CourseCode || c.CourseName == dto.CourseName) && !c.IsDeleted);

                if (exists)
                {
                    return new CourseActionResponse
                    {
                        Success = false,
                        Message = $"Course with code '{dto.CourseCode}' or name '{dto.CourseName}' already exists"
                    };
                }

                if (dto.PrerequisiteId.HasValue)
                {
                    var prerequisite = await _context.Courses
                        .FirstOrDefaultAsync(c => c.Id == dto.PrerequisiteId.Value && !c.IsDeleted);

                    if (prerequisite == null)
                    {
                        return new CourseActionResponse
                        {
                            Success = false,
                            Message = "Prerequisite course not found"
                        };
                    }
                }

                var departments = await _context.Departments
                    .Where(d => dto.DepartmentIds.Contains(d.Id) && !d.IsDeleted)
                    .ToListAsync();

                if (departments.Count != dto.DepartmentIds.Count)
                {
                    return new CourseActionResponse
                    {
                        Success = false,
                        Message = "One or more departments not found"
                    };
                }

                var course = new Course
                {
                    Id = Guid.NewGuid(),
                    CourseCode = dto.CourseCode,
                    CourseName = dto.CourseName,
                    //CourseNameAr = dto.CourseNameAr,
                    //Description = dto.Description,
                    Credits = dto.Credits,
                    PrerequisiteId = dto.PrerequisiteId,
                    CourseType = dto.CourseType,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Courses.AddAsync(course);

                foreach (var deptId in dto.DepartmentIds)
                {
                    var programPlan = new ProgramPlan
                    {
                        DepartmentId = deptId,
                        CourseId = course.Id,
                        RequirementType = RequirementType.Department,
                        IsCompulsory = dto.IsCompulsory
                    };
                    await _context.ProgramPlan.AddAsync(programPlan);
                }

                await _context.SaveChangesAsync();

                return new CourseActionResponse
                {
                    Success = true,
                    Message = "Course created successfully",
                    Data = new { CourseId = course.Id }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                return new CourseActionResponse
                {
                    Success = false,
                    Message = $"Error creating course: {ex.Message}"
                };
            }
        }

        #endregion

        #region Update Methods

        public async Task<CourseActionResponse> UpdateCourseAsync(Guid courseId, UpdateCourseDto dto)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Prerequisite)
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

                if (course == null)
                {
                    return new CourseActionResponse
                    {
                        Success = false,
                        Message = "Course not found"
                    };
                }

                if (!string.IsNullOrEmpty(dto.CourseCode) && dto.CourseCode != course.CourseCode)
                {
                    var exists = await _context.Courses
                        .AnyAsync(c => c.CourseCode == dto.CourseCode && c.Id != courseId && !c.IsDeleted);

                    if (exists)
                    {
                        return new CourseActionResponse
                        {
                            Success = false,
                            Message = $"Course with code '{dto.CourseCode}' already exists"
                        };
                    }

                    course.CourseCode = dto.CourseCode;
                }

                if (!string.IsNullOrEmpty(dto.CourseName) && dto.CourseName != course.CourseName)
                {
                    var exists = await _context.Courses
                        .AnyAsync(c => c.CourseName == dto.CourseName && c.Id != courseId && !c.IsDeleted);

                    if (exists)
                    {
                        return new CourseActionResponse
                        {
                            Success = false,
                            Message = $"Course with name '{dto.CourseName}' already exists"
                        };
                    }

                    course.CourseName = dto.CourseName;
                }

                //if (!string.IsNullOrEmpty(dto.CourseNameAr))
                //    course.CourseNameAr = dto.CourseNameAr;

                //if (!string.IsNullOrEmpty(dto.Description))
                //    course.Description = dto.Description;

                if (dto.Credits.HasValue)
                    course.Credits = dto.Credits.Value;

                if (dto.CourseType.HasValue)
                    course.CourseType = dto.CourseType.Value;

                if (dto.PrerequisiteId != course.PrerequisiteId)
                {
                    if (dto.PrerequisiteId.HasValue)
                    {
                        var prerequisite = await _context.Courses
                            .FirstOrDefaultAsync(c => c.Id == dto.PrerequisiteId.Value && !c.IsDeleted);

                        if (prerequisite == null)
                        {
                            return new CourseActionResponse
                            {
                                Success = false,
                                Message = "Prerequisite course not found"
                            };
                        }

                        if (await HasCircularDependencyAsync(courseId, dto.PrerequisiteId.Value))
                        {
                            return new CourseActionResponse
                            {
                                Success = false,
                                Message = "Circular dependency detected"
                            };
                        }

                        course.PrerequisiteId = dto.PrerequisiteId.Value;
                    }
                    else
                    {
                        course.PrerequisiteId = null;
                    }
                }

                if (dto.DepartmentIds != null && dto.DepartmentIds.Any())
                {
                    var departments = await _context.Departments
                        .Where(d => dto.DepartmentIds.Contains(d.Id) && !d.IsDeleted)
                        .ToListAsync();

                    if (departments.Count != dto.DepartmentIds.Count)
                    {
                        return new CourseActionResponse
                        {
                            Success = false,
                            Message = "One or more departments not found"
                        };
                    }

                    var oldPrograms = await _context.ProgramPlan
                        .Where(p => p.CourseId == courseId)
                        .ToListAsync();
                    _context.ProgramPlan.RemoveRange(oldPrograms);

                    foreach (var deptId in dto.DepartmentIds)
                    {
                        var programPlan = new ProgramPlan
                        {
                            DepartmentId = deptId,
                            CourseId = courseId,
                            RequirementType = RequirementType.Department,
                            IsCompulsory = dto.IsCompulsory ?? true
                        };
                        await _context.ProgramPlan.AddAsync(programPlan);
                    }
                }

                if (dto.IsActive.HasValue)
                    course.IsActive = dto.IsActive.Value;

                course.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new CourseActionResponse
                {
                    Success = true,
                    Message = "Course updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating course {courseId}");
                return new CourseActionResponse
                {
                    Success = false,
                    Message = $"Error updating course: {ex.Message}"
                };
            }
        }

        #endregion

        #region Prerequisite Management

        public async Task<CourseActionResponse> AddPrerequisiteAsync(Guid courseId, Guid prerequisiteId)
        {
            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

                if (course == null)
                {
                    return new CourseActionResponse
                    {
                        Success = false,
                        Message = "Course not found"
                    };
                }

                var prerequisite = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == prerequisiteId && !c.IsDeleted);

                if (prerequisite == null)
                {
                    return new CourseActionResponse
                    {
                        Success = false,
                        Message = "Prerequisite course not found"
                    };
                }

                if (await HasCircularDependencyAsync(courseId, prerequisiteId))
                {
                    return new CourseActionResponse
                    {
                        Success = false,
                        Message = "Circular dependency detected"
                    };
                }

                course.PrerequisiteId = prerequisiteId;
                course.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new CourseActionResponse
                {
                    Success = true,
                    Message = "Prerequisite added successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding prerequisite to course {courseId}");
                return new CourseActionResponse
                {
                    Success = false,
                    Message = $"Error adding prerequisite: {ex.Message}"
                };
            }
        }

        public async Task<CourseActionResponse> RemovePrerequisiteAsync(Guid courseId)
        {
            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

                if (course == null)
                {
                    return new CourseActionResponse
                    {
                        Success = false,
                        Message = "Course not found"
                    };
                }

                if (!course.PrerequisiteId.HasValue)
                {
                    return new CourseActionResponse
                    {
                        Success = false,
                        Message = "Course has no prerequisite"
                    };
                }

                course.PrerequisiteId = null;
                course.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new CourseActionResponse
                {
                    Success = true,
                    Message = "Prerequisite removed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing prerequisite from course {courseId}");
                return new CourseActionResponse
                {
                    Success = false,
                    Message = $"Error removing prerequisite: {ex.Message}"
                };
            }
        }

        public async Task<IEnumerable<CoursePrerequisiteDto>> GetPrerequisiteChainAsync(Guid courseId)
        {
            return await GetPrerequisiteChainInternalAsync(courseId);
        }

        #endregion

        #region Department Management

        public async Task<CourseActionResponse> AddDepartmentsAsync(Guid courseId, ManageCourseDepartmentsDto dto)
        {
            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

                if (course == null)
                {
                    return new CourseActionResponse
                    {
                        Success = false,
                        Message = "Course not found"
                    };
                }

                var departments = await _context.Departments
                    .Where(d => dto.DepartmentIds.Contains(d.Id) && !d.IsDeleted)
                    .ToListAsync();

                if (departments.Count != dto.DepartmentIds.Count)
                {
                    return new CourseActionResponse
                    {
                        Success = false,
                        Message = "One or more departments not found"
                    };
                }

                foreach (var deptId in dto.DepartmentIds)
                {
                    var alreadyExists = await _context.ProgramPlan
                        .AnyAsync(p => p.DepartmentId == deptId && p.CourseId == courseId);

                    if (!alreadyExists)
                    {
                        var programPlan = new ProgramPlan
                        {
                            DepartmentId = deptId,
                            CourseId = courseId,
                            RequirementType = RequirementType.Department,
                            IsCompulsory = dto.IsCompulsory
                        };
                        await _context.ProgramPlan.AddAsync(programPlan);
                    }
                }

                await _context.SaveChangesAsync();

                return new CourseActionResponse
                {
                    Success = true,
                    Message = "Departments added successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding departments to course {courseId}");
                return new CourseActionResponse
                {
                    Success = false,
                    Message = $"Error adding departments: {ex.Message}"
                };
            }
        }

        public async Task<CourseActionResponse> RemoveDepartmentAsync(Guid courseId, Guid departmentId)
        {
            try
            {
                var hasActiveOfferings = await _context.CourseOfferings
                    .AnyAsync(co => co.CourseId == courseId &&
                                   co.DepartmentId == departmentId &&
                                   co.IsActive);

                if (hasActiveOfferings)
                {
                    return new CourseActionResponse
                    {
                        Success = false,
                        Message = "Cannot remove department with active course offerings"
                    };
                }

                var programPlan = await _context.ProgramPlan
                    .FirstOrDefaultAsync(p => p.DepartmentId == departmentId && p.CourseId == courseId);

                if (programPlan == null)
                {
                    return new CourseActionResponse
                    {
                        Success = false,
                        Message = "Department not associated with this course"
                    };
                }

                _context.ProgramPlan.Remove(programPlan);
                await _context.SaveChangesAsync();

                return new CourseActionResponse
                {
                    Success = true,
                    Message = "Department removed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing department from course {courseId}");
                return new CourseActionResponse
                {
                    Success = false,
                    Message = $"Error removing department: {ex.Message}"
                };
            }
        }

        public async Task<IEnumerable<CourseDepartmentDto>> GetCourseDepartmentsAsync(Guid courseId)
        {
            return await _context.ProgramPlan
                .Where(p => p.CourseId == courseId)
                .Include(p => p.Department)
                    .ThenInclude(d => d.DepartmentFaculties)
                        .ThenInclude(df => df.Faculty)
                .Select(p => new CourseDepartmentDto
                {
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department.DepartmentName,
                    FacultyId = p.Department.DepartmentFaculties.FirstOrDefault().FacultyId,
                    FacultyName = p.Department.DepartmentFaculties.FirstOrDefault().Faculty.Name.ToString(),
                    //FacultyName = p.Department.DepartmentFaculties.FirstOrDefault().Faculty.NameEn,
                    FacultyCode = p.Department.DepartmentFaculties.FirstOrDefault().Faculty.Code,
                    DepartmentCode = p.Department.DepartmentFaculties.FirstOrDefault().DepartmentCode,
                    IsCompulsory = p.IsCompulsory
                })
                .ToListAsync();
        }

        #endregion

        #region Status Change Methods

        public async Task<CourseActionResponse> ActivateCourseAsync(Guid courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return new CourseActionResponse { Success = false, Message = "Course not found" };

            if (course.IsActive)
                return new CourseActionResponse { Success = false, Message = "Course is already active" };

            course.IsActive = true;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CourseActionResponse { Success = true, Message = "Course activated successfully" };
        }

        public async Task<CourseActionResponse> DeactivateCourseAsync(Guid courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return new CourseActionResponse { Success = false, Message = "Course not found" };

            if (!course.IsActive)
                return new CourseActionResponse { Success = false, Message = "Course is already inactive" };

            course.IsActive = false;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CourseActionResponse { Success = true, Message = "Course deactivated successfully" };
        }

        public async Task<CourseActionResponse> SoftDeleteCourseAsync(Guid courseId)
        {
            var hasActiveOfferings = await _context.CourseOfferings
                .AnyAsync(co => co.CourseId == courseId && co.IsActive);

            if (hasActiveOfferings)
            {
                return new CourseActionResponse
                {
                    Success = false,
                    Message = "Cannot delete course with active offerings"
                };
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return new CourseActionResponse { Success = false, Message = "Course not found" };

            if (course.IsDeleted)
                return new CourseActionResponse { Success = false, Message = "Course is already deleted" };

            course.IsDeleted = true;
            course.IsActive = false;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CourseActionResponse { Success = true, Message = "Course soft deleted successfully" };
        }

        public async Task<CourseActionResponse> RestoreCourseAsync(Guid courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return new CourseActionResponse { Success = false, Message = "Course not found" };

            if (!course.IsDeleted)
                return new CourseActionResponse { Success = false, Message = "Course is not deleted" };

            course.IsDeleted = false;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CourseActionResponse { Success = true, Message = "Course restored successfully" };
        }

        #endregion

        #region Search

        public async Task<IEnumerable<CourseListDto>> SearchCoursesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<CourseListDto>();

            var courses = await _context.Courses
                .Include(c => c.Prerequisite)
                .Where(c => !c.IsDeleted && (
                    c.CourseCode.Contains(searchTerm) ||
                    c.CourseName.Contains(searchTerm)))
                .Take(20)
                .Select(c => new CourseListDto
                {
                    Id = c.Id,
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    Credits = c.Credits,
                    PrerequisiteName = c.Prerequisite != null ? c.Prerequisite.CourseName : null,
                    CourseType = c.CourseType.ToString(),
                    TotalOfferings = _context.CourseOfferings.Count(co => co.CourseId == c.Id),
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return courses;
        }

        #endregion

        #region Statistics

        public async Task<CourseStatisticsDto> GetCourseStatisticsAsync()
        {
            var courses = await _context.Courses
                .Include(c => c.Prerequisite)
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            var stats = new CourseStatisticsDto
            {
                TotalCourses = courses.Count,
                ActiveCourses = courses.Count(c => c.IsActive),
                InactiveCourses = courses.Count(c => !c.IsActive),
                CoursesWithPrerequisites = courses.Count(c => c.PrerequisiteId.HasValue),
                CoursesWithoutPrerequisites = courses.Count(c => !c.PrerequisiteId.HasValue),
                GeneratedAt = DateTime.UtcNow
            };

            stats.CoursesByType = courses
                .GroupBy(c => c.CourseType)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Count()
                );

            stats.TotalEnrollments = await _context.Enrollments
                .CountAsync();

            stats.AverageCredits = courses.Count > 0
                ? Math.Round(courses.Average(c => c.Credits), 2)
                : 0;

            var popularCourses = await _context.Enrollments
                .GroupBy(e => new {
                    e.CourseOffering.CourseId,
                    e.CourseOffering.Course.CourseCode,
                    e.CourseOffering.Course.CourseName
                })
                .Select(g => new
                {
                    g.Key.CourseId,
                    g.Key.CourseCode,
                    g.Key.CourseName,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            stats.PopularCourses = popularCourses.Select(x => new PopularCourseDto
            {
                CourseCode = x.CourseCode,
                CourseName = x.CourseName,
                EnrollmentCount = x.Count
            }).ToList();

            return stats;
        }

        #endregion

        #region Export Methods

        public async Task<byte[]> ExportCoursesToExcelAsync(CourseFilterDto? filter = null)
        {
            var courses = await GetCoursesAsync(filter ?? new CourseFilterDto { PageSize = 1000 });

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Courses");

                var headers = new[] {
                    "ID", "Course Code", "Course Name (EN)", "Course Name (AR)",
                    "Credits", "Prerequisite", "Type",
                    "Total Offerings", "Total Enrollments", "Status", "Created Date"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                int row = 2;
                foreach (var course in courses.Items)
                {
                    worksheet.Cell(row, 1).Value = course.Id.ToString();
                    worksheet.Cell(row, 2).Value = course.CourseCode;
                    worksheet.Cell(row, 3).Value = course.CourseName;
                    worksheet.Cell(row, 4).Value = course.CourseNameAr;
                    worksheet.Cell(row, 5).Value = course.Credits;
                    worksheet.Cell(row, 6).Value = course.PrerequisiteName ?? "None";
                    worksheet.Cell(row, 7).Value = course.CourseType;
                    worksheet.Cell(row, 8).Value = course.TotalOfferings;
                    worksheet.Cell(row, 9).Value = course.TotalEnrollments;
                    worksheet.Cell(row, 10).Value = course.IsActive ? "Active" : "Inactive";
                    worksheet.Cell(row, 11).Value = course.CreatedAt.ToString("yyyy-MM-dd");
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

        public async Task<byte[]> ExportCoursesToCsvAsync(CourseFilterDto? filter = null)
        {
            var courses = await GetCoursesAsync(filter ?? new CourseFilterDto { PageSize = 1000 });

            var csv = new StringBuilder();
            csv.AppendLine("ID,Course Code,Course Name (EN),Course Name (AR),Credits,Prerequisite,Type,Total Offerings,Total Enrollments,Status,Created Date");

            foreach (var course in courses.Items)
            {
                csv.AppendLine($"\"{course.Id}\",\"{course.CourseCode}\",\"{course.CourseName}\",\"{course.CourseNameAr}\",{course.Credits},\"{course.PrerequisiteName}\",\"{course.CourseType}\",{course.TotalOfferings},{course.TotalEnrollments},\"{(course.IsActive ? "Active" : "Inactive")}\",\"{course.CreatedAt:yyyy-MM-dd}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<string> GenerateCoursesReportAsync(CourseFilterDto? filter = null)
        {
            var courses = await GetCoursesAsync(filter ?? new CourseFilterDto());
            var stats = await GetCourseStatisticsAsync();

            var report = new StringBuilder();

            report.AppendLine("=== Course Management Report ===");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine("=== Summary ===");
            report.AppendLine($"Total Courses: {stats.TotalCourses}");
            report.AppendLine($"Active Courses: {stats.ActiveCourses}");
            report.AppendLine($"Inactive Courses: {stats.InactiveCourses}");
            report.AppendLine($"Courses with Prerequisites: {stats.CoursesWithPrerequisites}");
            report.AppendLine($"Courses without Prerequisites: {stats.CoursesWithoutPrerequisites}");
            report.AppendLine($"Total Enrollments: {stats.TotalEnrollments}");
            report.AppendLine($"Average Credits: {stats.AverageCredits}");
            report.AppendLine();
            report.AppendLine("=== Courses by Type ===");
            foreach (var kvp in stats.CoursesByType)
                report.AppendLine($"{kvp.Key}: {kvp.Value}");
            report.AppendLine();
            report.AppendLine("=== Popular Courses (Top 5) ===");
            foreach (var course in stats.PopularCourses)
                report.AppendLine($"{course.CourseCode} - {course.CourseName}: {course.EnrollmentCount} enrollments");
            report.AppendLine();
            report.AppendLine("=== Recent Courses (Last 10) ===");
            foreach (var course in courses.Items.Take(10))
            {
                report.AppendLine($"{course.CourseCode} - {course.CourseName} - Created: {course.CreatedAt:yyyy-MM-dd}");
            }

            return report.ToString();
        }

        #endregion

        #region Private Helper Methods

        private IQueryable<Course> BuildCourseQuery(CourseFilterDto filter)
        {
            var query = _context.Courses
                .Include(c => c.Prerequisite)
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (filter == null)
                return query;

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.CourseCode.ToLower().Contains(searchTerm) ||
                    c.CourseName.ToLower().Contains(searchTerm));
            }

            if (filter.DepartmentIds != null && filter.DepartmentIds.Any())
            {
                query = query.Where(c => c.Programs.Any(p => filter.DepartmentIds.Contains(p.DepartmentId)));
            }

            if (filter.MinCredits.HasValue)
                query = query.Where(c => c.Credits >= filter.MinCredits.Value);

            if (filter.MaxCredits.HasValue)
                query = query.Where(c => c.Credits <= filter.MaxCredits.Value);

            if (filter.HasPrerequisite.HasValue)
            {
                if (filter.HasPrerequisite.Value)
                    query = query.Where(c => c.PrerequisiteId != null);
                else
                    query = query.Where(c => c.PrerequisiteId == null);
            }

            if (filter.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filter.IsActive.Value);

            if (filter.CourseType.HasValue)
                query = query.Where(c => c.CourseType == filter.CourseType.Value);

            if (filter.CreatedFrom.HasValue)
                query = query.Where(c => c.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(c => c.CreatedAt <= filter.CreatedTo.Value);

            query = filter.SortBy?.ToLower() switch
            {
                "code" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(c => c.CourseCode)
                    : query.OrderBy(c => c.CourseCode),
                "name" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(c => c.CourseName)
                    : query.OrderBy(c => c.CourseName),
                "credits" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(c => c.Credits)
                    : query.OrderBy(c => c.Credits),
                "type" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(c => c.CourseType)
                    : query.OrderBy(c => c.CourseType),
                _ => filter.SortOrder == "desc"
                    ? query.OrderByDescending(c => c.CourseCode)
                    : query.OrderBy(c => c.CourseCode)
            };

            return query;
        }

        private async Task<IEnumerable<CoursePrerequisiteDto>> GetPrerequisiteChainInternalAsync(Guid courseId)
        {
            var chain = new List<CoursePrerequisiteDto>();
            var currentCourse = await _context.Courses
                .Include(c => c.Prerequisite)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            while (currentCourse?.Prerequisite != null)
            {
                var prereq = currentCourse.Prerequisite;
                chain.Add(new CoursePrerequisiteDto
                {
                    CourseId = prereq.Id,
                    CourseCode = prereq.CourseCode,
                    CourseName = prereq.CourseName,
                    Credits = prereq.Credits
                });
                currentCourse = prereq;
            }

            return chain;
        }

        private async Task<bool> HasCircularDependencyAsync(Guid courseId, Guid prerequisiteId)
        {
            var visited = new HashSet<Guid>();
            var currentId = prerequisiteId;

            while (currentId != Guid.Empty)
            {
                if (currentId == courseId)
                    return true;

                if (visited.Contains(currentId))
                    break;

                visited.Add(currentId);

                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == currentId);

                if (course == null || !course.PrerequisiteId.HasValue)
                    break;

                currentId = course.PrerequisiteId.Value;
            }

            return false;
        }

        #endregion
    }
}
