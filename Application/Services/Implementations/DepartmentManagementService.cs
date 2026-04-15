using ClosedXML.Excel;
using HUP.Application.DTOs.AcademicDtos.DepartmentDtos;
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
    public class DepartmentManagementService : IDepartmentManagementService
    {
        private readonly HupDbContext _context;
        private readonly ILogger<DepartmentManagementService> _logger;

        public DepartmentManagementService(
            HupDbContext context,
            ILogger<DepartmentManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Query Methods

        public async Task<PaginatedResult<DepartmentListDto>> GetDepartmentsAsync(DepartmentFilterDto? filter = null)
        {
            try
            {
                filter ??= new DepartmentFilterDto();
                var query = BuildDepartmentQuery(filter);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(d => new DepartmentListDto
                    {
                        Id = d.Id,
                        DepartmentName = d.DepartmentName,
                        BaseDepartmentCode = d.BaseDepartmentCode,
                        Faculties = d.DepartmentFaculties.Select(df => new DepartmentFacultyInfoDto
                        {
                            FacultyId = df.FacultyId,
                            FacultyName = df.Faculty.Name.ToString(),
                            //FacultyName = df.Faculty.NameEn,
                            DepartmentCode = df.DepartmentCode,
                            IsPrimary = df.IsPrimary
                        }).ToList(),
                        HeadOfDepartmentName = d.HeadOfDepartment != null ? d.HeadOfDepartment.User.FullName : null,
                        HeadOfDepartmentId = d.HeadOfDepartmentId,
                        CompulsoryHours = d.CompulsoryHours,
                        ElectiveHours = d.ElectiveHours,
                        StudentCount = _context.Students.Count(s => s.DepartmentId == d.Id && !s.User.IsDeleted),
                        InstructorCount = _context.Staff.Count(i => i.DepartmentId == d.Id && !i.User.IsDeleted),
                        IsActive = d.IsActive,
                        CreatedAt = d.CreatedAt
                    })
                    .ToListAsync();

                return new PaginatedResult<DepartmentListDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDepartmentsAsync");
                throw;
            }
        }

        public async Task<DepartmentDetailsDto> GetDepartmentByIdAsync(Guid departmentId)
        {
            try
            {
                var department = await _context.Departments
                    .Include(d => d.HeadOfDepartment)
                        .ThenInclude(h => h.User)
                    .Include(d => d.DepartmentFaculties)
                        .ThenInclude(df => df.Faculty)
                    .Include(d => d.StaffMembers)
                        .ThenInclude(i => i.User)
                    .Include(d => d.Programs)
                        .ThenInclude(p => p.Course)
                            .ThenInclude(c => c.Prerequisite)
                    .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

                if (department == null)
                    return null;

                var courseEnrollments = await _context.Enrollments
                    .Where(e => e.CourseOffering.Course.Programs.Any(p => p.DepartmentId == departmentId))
                    .GroupBy(e => e.CourseOffering.CourseId)
                    .Select(g => new { CourseId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CourseId, x => x.Count);

                var dto = new DepartmentDetailsDto
                {
                    Id = department.Id,
                    DepartmentName = department.DepartmentName,
                    BaseDepartmentCode = department.BaseDepartmentCode,
                    Faculties = department.DepartmentFaculties.Select(df => new DepartmentFacultyInfoDto
                    {
                        FacultyId = df.FacultyId,
                        FacultyName = df.Faculty.Name.ToString(),
                        //FacultyName = df.Faculty.NameEn,
                        DepartmentCode = df.DepartmentCode,
                        IsPrimary = df.IsPrimary
                    }).ToList(),
                    HeadOfDepartmentName = department.HeadOfDepartment?.User?.FullName,
                    HeadOfDepartmentId = department.HeadOfDepartmentId,
                    CompulsoryHours = department.CompulsoryHours,
                    ElectiveHours = department.ElectiveHours,
                    StudentCount = await _context.Students.CountAsync(s => s.DepartmentId == departmentId && !s.User.IsDeleted),
                    InstructorCount = department.StaffMembers.Count(i => !i.User.IsDeleted),
                    IsActive = department.IsActive,
                    CreatedAt = department.CreatedAt,
                    UpdatedAt = department.UpdatedAt,
                    FacultyContactInfo = department.DepartmentFaculties.FirstOrDefault()?.Faculty?.ContactInfo,

                    Courses = department.Programs.Select(p => new DepartmentCourseDto
                    {
                        CourseId = p.Course.Id,
                        CourseCode = p.Course.CourseCode,
                        CourseName = p.Course.CourseName,
                        Credits = p.Course.Credits,
                        IsCompulsory = p.IsCompulsory,
                        PrerequisiteName = p.Course.Prerequisite?.CourseName,
                        StudentEnrollmentCount = courseEnrollments.GetValueOrDefault(p.Course.Id, 0)
                    }).ToList(),

                    Instructors = department.StaffMembers.Where(i => !i.User.IsDeleted).Select(i => new DepartmentInstructorDto
                    {
                        InstructorId = i.UserId,
                        FullName = i.User.FullName,
                        AcademicTitle = i.Title.ToString(),
                        Email = i.User.Email,
                        IsHeadOfDepartment = i.UserId == department.HeadOfDepartmentId,
                        IsActive = i.User.IsActive
                    }).ToList()
                };

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetDepartmentByIdAsync for ID {departmentId}");
                throw;
            }
        }

        public async Task<IEnumerable<LookupDto>> GetDepartmentsLookupAsync(Guid? facultyId = null)
        {
            var query = _context.DepartmentFaculties
                .Include(df => df.Department)
                .Include(df => df.Faculty)
                .Where(df => df.Department.IsActive && !df.Department.IsDeleted);

            if (facultyId.HasValue)
            {
                query = query.Where(df => df.FacultyId == facultyId.Value);
            }

            return await query
                .OrderBy(df => df.Faculty.Name)
                //.OrderBy(df => df.Faculty.NameEn)
                .ThenBy(df => df.Department.DepartmentName)
                .Select(df => new LookupDto
                {
                    Id = df.DepartmentId,
                    Name = $"{df.Department.DepartmentName} ({df.Faculty.Name})"
                    //Name = $"{df.Department.DepartmentName} ({df.Faculty.NameEn})"
                })
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<LookupDto>> GetDepartmentsByFacultyLookupAsync(Guid facultyId)
        {
            return await _context.DepartmentFaculties
                .Include(df => df.Department)
                .Where(df => df.FacultyId == facultyId && df.Department.IsActive && !df.Department.IsDeleted)
                .OrderBy(df => df.Department.DepartmentName)
                .Select(df => new LookupDto
                {
                    Id = df.DepartmentId,
                    Name = df.Department.DepartmentName
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetDepartmentCodesAsync()
        {
            try
            {
                return await _context.DepartmentFaculties
                    .Where(df => !df.Department.IsDeleted)
                    .Select(df => df.DepartmentCode)
                    .Distinct()
                    .Where(code => code != null && code != "")
                    .OrderBy(code => code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDepartmentCodesAsync");
                throw;
            }
        }

        #endregion

        #region Create Methods

        public async Task<DepartmentActionResponse> CreateDepartmentAsync(CreateDepartmentDto dto)
        {
            try
            {
                var faculties = await _context.Faculties
                    .Where(f => dto.FacultyIds.Contains(f.Id) && !f.IsDeleted)
                    .ToListAsync();

                if (faculties.Count != dto.FacultyIds.Count)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "One or more faculties not found"
                    };
                }

                foreach (var facultyId in dto.FacultyIds)
                {
                    var faculty = faculties.First(f => f.Id == facultyId);
                    var fullCode = $"{faculty.Code}-{dto.BaseDepartmentCode}";

                    var exists = await _context.DepartmentFaculties
                        .AnyAsync(df => df.FacultyId == facultyId &&
                                       df.DepartmentCode == fullCode &&
                                       !df.Department.IsDeleted);

                    if (exists)
                    {
                        return new DepartmentActionResponse
                        {
                            Success = false,
                            Message = $"Department with code '{fullCode}' already exists in faculty {faculty.Name}"
                            //Message = $"Department with code '{fullCode}' already exists in faculty {faculty.NameEn}"
                        };
                    }
                }

                if (dto.HeadOfDepartmentId.HasValue)
                {
                    var headExists = await _context.Staff
                        .AnyAsync(i => i.UserId == dto.HeadOfDepartmentId.Value && !i.User.IsDeleted);

                    if (!headExists)
                    {
                        return new DepartmentActionResponse
                        {
                            Success = false,
                            Message = "Selected head of department not found"
                        };
                    }

                    var isHeadElsewhere = await _context.Departments
                        .AnyAsync(d => d.HeadOfDepartmentId == dto.HeadOfDepartmentId.Value && !d.IsDeleted);

                    if (isHeadElsewhere)
                    {
                        return new DepartmentActionResponse
                        {
                            Success = false,
                            Message = "This instructor is already head of another department"
                        };
                    }
                }

                var department = new Department
                {
                    Id = Guid.NewGuid(),
                    DepartmentName = dto.DepartmentName,
                    BaseDepartmentCode = dto.BaseDepartmentCode,
                    HeadOfDepartmentId = dto.HeadOfDepartmentId,
                    CompulsoryHours = dto.CompulsoryHours,
                    ElectiveHours = dto.ElectiveHours,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Departments.AddAsync(department);

                bool isFirst = true;
                foreach (var facultyId in dto.FacultyIds)
                {
                    var faculty = faculties.First(f => f.Id == facultyId);
                    var departmentFaculty = new DepartmentFaculty
                    {
                        DepartmentId = department.Id,
                        FacultyId = facultyId,
                        DepartmentCode = $"{faculty.Code}-{dto.BaseDepartmentCode}",
                        IsPrimary = isFirst, 
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.DepartmentFaculties.AddAsync(departmentFaculty);
                    isFirst = false;
                }

                await _context.SaveChangesAsync();

                return new DepartmentActionResponse
                {
                    Success = true,
                    Message = "Department created successfully",
                    Data = new { DepartmentId = department.Id }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                return new DepartmentActionResponse
                {
                    Success = false,
                    Message = $"Error creating department: {ex.Message}"
                };
            }
        }

        #endregion

        #region Update Methods

        public async Task<DepartmentActionResponse> UpdateDepartmentAsync(Guid departmentId, UpdateDepartmentDto dto)
        {
            try
            {
                var department = await _context.Departments
                    .Include(d => d.DepartmentFaculties)
                        .ThenInclude(df => df.Faculty)
                    .Include(d => d.HeadOfDepartment)
                    .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

                if (department == null)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "Department not found"
                    };
                }

                if (!string.IsNullOrEmpty(dto.DepartmentName) && dto.DepartmentName != department.DepartmentName)
                {
                    department.DepartmentName = dto.DepartmentName;
                }

                if (!string.IsNullOrEmpty(dto.BaseDepartmentCode) && dto.BaseDepartmentCode != department.BaseDepartmentCode)
                {
                    foreach (var df in department.DepartmentFaculties)
                    {
                        var newFullCode = $"{df.Faculty.Code}-{dto.BaseDepartmentCode}";
                        var exists = await _context.DepartmentFaculties
                            .AnyAsync(x => x.FacultyId == df.FacultyId &&
                                          x.DepartmentCode == newFullCode &&
                                          x.DepartmentId != departmentId &&
                                          !x.Department.IsDeleted);

                        if (exists)
                        {
                            return new DepartmentActionResponse
                            {
                                Success = false,
                                Message = $"Department with code '{newFullCode}' already exists in faculty {df.Faculty.Name}"
                                //Message = $"Department with code '{newFullCode}' already exists in faculty {df.Faculty.NameEn}"
                            };
                        }
                    }

                    foreach (var df in department.DepartmentFaculties)
                    {
                        df.DepartmentCode = $"{df.Faculty.Code}-{dto.BaseDepartmentCode}";
                    }

                    department.BaseDepartmentCode = dto.BaseDepartmentCode;
                }

                if (dto.FacultyIds != null && dto.FacultyIds.Any())
                {
                    var faculties = await _context.Faculties
                        .Where(f => dto.FacultyIds.Contains(f.Id) && !f.IsDeleted)
                        .ToListAsync();

                    if (faculties.Count != dto.FacultyIds.Count)
                    {
                        return new DepartmentActionResponse
                        {
                            Success = false,
                            Message = "One or more faculties not found"
                        };
                    }

                    foreach (var facultyId in dto.FacultyIds)
                    {
                        var faculty = faculties.First(f => f.Id == facultyId);
                        var fullCode = $"{faculty.Code}-{department.BaseDepartmentCode}";

                        var exists = await _context.DepartmentFaculties
                            .AnyAsync(df => df.FacultyId == facultyId &&
                                           df.DepartmentCode == fullCode &&
                                           df.DepartmentId != departmentId &&
                                           !df.Department.IsDeleted);

                        if (exists)
                        {
                            return new DepartmentActionResponse
                            {
                                Success = false,
                                Message = $"Department with code '{fullCode}' already exists in faculty {faculty.Name}"
                                //Message = $"Department with code '{fullCode}' already exists in faculty {faculty.NameEn}"
                            };
                        }
                    }

                    _context.DepartmentFaculties.RemoveRange(department.DepartmentFaculties);

                    bool isFirst = true;
                    foreach (var facultyId in dto.FacultyIds)
                    {
                        var faculty = faculties.First(f => f.Id == facultyId);
                        var departmentFaculty = new DepartmentFaculty
                        {
                            DepartmentId = department.Id,
                            FacultyId = facultyId,
                            DepartmentCode = $"{faculty.Code}-{department.BaseDepartmentCode}",
                            IsPrimary = isFirst,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.DepartmentFaculties.AddAsync(departmentFaculty);
                        isFirst = false;
                    }
                }

                if (dto.HeadOfDepartmentId != department.HeadOfDepartmentId)
                {
                    if (dto.HeadOfDepartmentId.HasValue)
                    {
                        var newHead = await _context.Staff
                            .FirstOrDefaultAsync(i => i.UserId == dto.HeadOfDepartmentId.Value && !i.User.IsDeleted);

                        if (newHead == null)
                        {
                            return new DepartmentActionResponse
                            {
                                Success = false,
                                Message = "Selected head of department not found"
                            };
                        }

                        var isHeadElsewhere = await _context.Departments
                            .AnyAsync(d => d.HeadOfDepartmentId == dto.HeadOfDepartmentId.Value &&
                                          d.Id != departmentId && !d.IsDeleted);

                        if (isHeadElsewhere)
                        {
                            return new DepartmentActionResponse
                            {
                                Success = false,
                                Message = "This instructor is already head of another department"
                            };
                        }

                        if (newHead.DepartmentId != departmentId)
                        {
                            return new DepartmentActionResponse
                            {
                                Success = false,
                                Message = "Instructor must belong to this department to be head"
                            };
                        }

                        department.HeadOfDepartmentId = dto.HeadOfDepartmentId.Value;
                    }
                    else
                    {
                        department.HeadOfDepartmentId = null;
                    }
                }

                if (dto.CompulsoryHours.HasValue)
                    department.CompulsoryHours = dto.CompulsoryHours.Value;

                if (dto.ElectiveHours.HasValue)
                    department.ElectiveHours = dto.ElectiveHours.Value;

                if (dto.IsActive.HasValue)
                    department.IsActive = dto.IsActive.Value;

                department.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new DepartmentActionResponse
                {
                    Success = true,
                    Message = "Department updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating department {departmentId}");
                return new DepartmentActionResponse
                {
                    Success = false,
                    Message = $"Error updating department: {ex.Message}"
                };
            }
        }

        #endregion

        #region Head of Department Management

        public async Task<DepartmentActionResponse> AssignHeadOfDepartmentAsync(Guid departmentId, Guid instructorId)
        {
            try
            {
                var department = await _context.Departments
                    .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

                if (department == null)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "Department not found"
                    };
                }

                var instructor = await _context.Staff
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.UserId == instructorId && !i.User.IsDeleted);

                if (instructor == null)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "Instructor not found"
                    };
                }

                var isHeadElsewhere = await _context.Departments
                    .AnyAsync(d => d.HeadOfDepartmentId == instructorId && d.Id != departmentId && !d.IsDeleted);

                if (isHeadElsewhere)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "This instructor is already head of another department"
                    };
                }

                if (instructor.DepartmentId != departmentId)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "Instructor must belong to this department to be head"
                    };
                }

                department.HeadOfDepartmentId = instructorId;
                department.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new DepartmentActionResponse
                {
                    Success = true,
                    Message = "Head of department assigned successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning head to department {departmentId}");
                return new DepartmentActionResponse
                {
                    Success = false,
                    Message = $"Error assigning head: {ex.Message}"
                };
            }
        }

        public async Task<DepartmentActionResponse> RemoveHeadOfDepartmentAsync(Guid departmentId)
        {
            try
            {
                var department = await _context.Departments
                    .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

                if (department == null)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "Department not found"
                    };
                }

                if (!department.HeadOfDepartmentId.HasValue)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "Department has no head assigned"
                    };
                }

                department.HeadOfDepartmentId = null;
                department.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new DepartmentActionResponse
                {
                    Success = true,
                    Message = "Head of department removed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing head from department {departmentId}");
                return new DepartmentActionResponse
                {
                    Success = false,
                    Message = $"Error removing head: {ex.Message}"
                };
            }
        }

        #endregion

        #region Course Management

        public async Task<DepartmentActionResponse> AddCourseToProgramAsync(Guid departmentId, ManageDepartmentCourseDto dto)
        {
            try
            {
                var department = await _context.Departments
                    .FirstOrDefaultAsync(d => d.Id == departmentId && !d.IsDeleted);

                if (department == null)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "Department not found"
                    };
                }

                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == dto.CourseId && !c.IsDeleted);

                if (course == null)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "Course not found"
                    };
                }

                var alreadyAdded = await _context.ProgramPlan
                    .AnyAsync(p => p.DepartmentId == departmentId && p.CourseId == dto.CourseId);

                if (alreadyAdded)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "Course is already in department program"
                    };
                }

                var programPlan = new ProgramPlan
                {
                    DepartmentId = departmentId,
                    CourseId = dto.CourseId,
                    RequirementType = RequirementType.Department,
                    IsCompulsory = dto.IsCompulsory
                };

                await _context.ProgramPlan.AddAsync(programPlan);
                await _context.SaveChangesAsync();

                return new DepartmentActionResponse
                {
                    Success = true,
                    Message = "Course added to program successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding course to department {departmentId}");
                return new DepartmentActionResponse
                {
                    Success = false,
                    Message = $"Error adding course: {ex.Message}"
                };
            }
        }

        public async Task<DepartmentActionResponse> RemoveCourseFromProgramAsync(Guid departmentId, Guid courseId)
        {
            try
            {
                var hasEnrollments = await _context.Enrollments
                    .AnyAsync(e => e.CourseOffering.CourseId == courseId &&
                                  e.CourseOffering.Course.Programs.Any(p => p.DepartmentId == departmentId) &&
                                  e.Status != EnrollmentStatus.Completed &&
                                  e.Status != EnrollmentStatus.Failed &&
                                  e.Status != EnrollmentStatus.Dropped);

                if (hasEnrollments)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "Cannot remove course with active student enrollments"
                    };
                }

                var programPlan = await _context.ProgramPlan
                    .FirstOrDefaultAsync(p => p.DepartmentId == departmentId && p.CourseId == courseId);

                if (programPlan == null)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "Course not found in department program"
                    };
                }

                _context.ProgramPlan.Remove(programPlan);
                await _context.SaveChangesAsync();

                return new DepartmentActionResponse
                {
                    Success = true,
                    Message = "Course removed from program successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing course from department {departmentId}");
                return new DepartmentActionResponse
                {
                    Success = false,
                    Message = $"Error removing course: {ex.Message}"
                };
            }
        }

        public async Task<IEnumerable<DepartmentCourseDto>> GetDepartmentCoursesAsync(Guid departmentId)
        {
            var courses = await _context.ProgramPlan
                .Include(p => p.Course)
                    .ThenInclude(c => c.Prerequisite)
                .Where(p => p.DepartmentId == departmentId)
                .Select(p => new DepartmentCourseDto
                {
                    CourseId = p.Course.Id,
                    CourseCode = p.Course.CourseCode,
                    CourseName = p.Course.CourseName,
                    Credits = p.Course.Credits,
                    IsCompulsory = p.IsCompulsory,
                    PrerequisiteName = p.Course.Prerequisite.CourseName,
                    StudentEnrollmentCount = _context.Enrollments.Count(e =>
                        e.CourseOffering.CourseId == p.CourseId &&
                        e.CourseOffering.Course.Programs.Any(pp => pp.DepartmentId == departmentId))
                })
                .ToListAsync();

            return courses;
        }

        #endregion

        #region Transfer Operations

        public async Task<DepartmentActionResponse> TransferItemsAsync(TransferDepartmentItemsDto dto)
        {
            try
            {
                var sourceDept = await _context.Departments
                    .FirstOrDefaultAsync(d => d.Id == dto.SourceDepartmentId && !d.IsDeleted);

                var targetDept = await _context.Departments
                    .FirstOrDefaultAsync(d => d.Id == dto.TargetDepartmentId && !d.IsDeleted);

                if (sourceDept == null || targetDept == null)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "One or both departments not found"
                    };
                }

                if (sourceDept.Id == targetDept.Id)
                {
                    return new DepartmentActionResponse
                    {
                        Success = false,
                        Message = "Source and target departments cannot be the same"
                    };
                }

                var transferLog = new List<string>();

                if (dto.TransferStudents)
                {
                    var students = await _context.Students
                        .Where(s => s.DepartmentId == dto.SourceDepartmentId && !s.User.IsDeleted)
                        .ToListAsync();

                    foreach (var student in students)
                    {
                        student.DepartmentId = dto.TargetDepartmentId;
                    }

                    transferLog.Add($"{students.Count} students transferred");
                }

                if (dto.TransferInstructors)
                {
                    var instructors = await _context.Staff
                        .Where(i => i.DepartmentId == dto.SourceDepartmentId && !i.User.IsDeleted)
                        .ToListAsync();

                    var heads = instructors.Where(i => i.UserId == sourceDept.HeadOfDepartmentId).ToList();
                    if (heads.Any())
                    {
                        sourceDept.HeadOfDepartmentId = null;
                    }

                    foreach (var instructor in instructors)
                    {
                        instructor.DepartmentId = dto.TargetDepartmentId;
                    }

                    transferLog.Add($"{instructors.Count} instructors transferred");
                }

                await _context.SaveChangesAsync();

                return new DepartmentActionResponse
                {
                    Success = true,
                    Message = $"Transfer completed: {string.Join(", ", transferLog)}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring department items");
                return new DepartmentActionResponse
                {
                    Success = false,
                    Message = $"Error transferring items: {ex.Message}"
                };
            }
        }

        #endregion

        #region Status Change Methods

        public async Task<DepartmentActionResponse> ActivateDepartmentAsync(Guid departmentId)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
                return new DepartmentActionResponse { Success = false, Message = "Department not found" };

            if (department.IsActive)
                return new DepartmentActionResponse { Success = false, Message = "Department is already active" };

            department.IsActive = true;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new DepartmentActionResponse { Success = true, Message = "Department activated successfully" };
        }

        public async Task<DepartmentActionResponse> DeactivateDepartmentAsync(Guid departmentId)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
                return new DepartmentActionResponse { Success = false, Message = "Department not found" };

            if (!department.IsActive)
                return new DepartmentActionResponse { Success = false, Message = "Department is already inactive" };

            department.IsActive = false;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new DepartmentActionResponse { Success = true, Message = "Department deactivated successfully" };
        }

        public async Task<DepartmentActionResponse> SoftDeleteDepartmentAsync(Guid departmentId)
        {
            var hasStudents = await _context.Students
                .AnyAsync(s => s.DepartmentId == departmentId && !s.User.IsDeleted);

            if (hasStudents)
            {
                return new DepartmentActionResponse
                {
                    Success = false,
                    Message = "Cannot delete department with existing students. Transfer students first."
                };
            }

            var hasInstructors = await _context.Staff
                .AnyAsync(i => i.DepartmentId == departmentId && !i.User.IsDeleted);

            if (hasInstructors)
            {
                return new DepartmentActionResponse
                {
                    Success = false,
                    Message = "Cannot delete department with existing instructors. Transfer instructors first."
                };
            }

            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
                return new DepartmentActionResponse { Success = false, Message = "Department not found" };

            if (department.IsDeleted)
                return new DepartmentActionResponse { Success = false, Message = "Department is already deleted" };

            department.IsDeleted = true;
            department.IsActive = false;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new DepartmentActionResponse { Success = true, Message = "Department soft deleted successfully" };
        }

        public async Task<DepartmentActionResponse> RestoreDepartmentAsync(Guid departmentId)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
                return new DepartmentActionResponse { Success = false, Message = "Department not found" };

            if (!department.IsDeleted)
                return new DepartmentActionResponse { Success = false, Message = "Department is not deleted" };

            department.IsDeleted = false;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new DepartmentActionResponse { Success = true, Message = "Department restored successfully" };
        }

        #endregion

        #region Search

        public async Task<IEnumerable<DepartmentListDto>> SearchDepartmentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<DepartmentListDto>();

            var departments = await _context.Departments
                .Include(d => d.DepartmentFaculties)
                    .ThenInclude(df => df.Faculty)
                .Include(d => d.HeadOfDepartment)
                    .ThenInclude(h => h.User)
                .Where(d => !d.IsDeleted && (
                    d.DepartmentName.Contains(searchTerm) ||
                    d.BaseDepartmentCode.Contains(searchTerm) ||
                    d.DepartmentFaculties.Any(df => df.Faculty.Name.ToString().Contains(searchTerm))))
                //d.DepartmentFaculties.Any(df => df.Faculty.NameEn.Contains(searchTerm))))
                .Take(20)
                .Select(d => new DepartmentListDto
                {
                    Id = d.Id,
                    DepartmentName = d.DepartmentName,
                    BaseDepartmentCode = d.BaseDepartmentCode,
                    Faculties = d.DepartmentFaculties.Select(df => new DepartmentFacultyInfoDto
                    {
                        FacultyId = df.FacultyId,
                        FacultyName = df.Faculty.Name.ToString(),
                        //FacultyName = df.Faculty.NameEn,
                        DepartmentCode = df.DepartmentCode,
                        IsPrimary = df.IsPrimary
                    }).ToList(),
                    HeadOfDepartmentName = d.HeadOfDepartment.User.FullName,
                    CompulsoryHours = d.CompulsoryHours,
                    ElectiveHours = d.ElectiveHours,
                    StudentCount = _context.Students.Count(s => s.DepartmentId == d.Id && !s.User.IsDeleted),
                    InstructorCount = _context.Staff.Count(i => i.DepartmentId == d.Id && !i.User.IsDeleted),
                    IsActive = d.IsActive,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return departments;
        }

        #endregion

        #region Statistics

        public async Task<DepartmentStatisticsDto> GetDepartmentStatisticsAsync()
        {
            var departments = await _context.Departments
                .Include(d => d.DepartmentFaculties)
                    .ThenInclude(df => df.Faculty)
                .Where(d => !d.IsDeleted)
                .ToListAsync();

            var stats = new DepartmentStatisticsDto
            {
                TotalDepartments = departments.Count,
                ActiveDepartments = departments.Count(d => d.IsActive),
                InactiveDepartments = departments.Count(d => !d.IsActive),
                DepartmentsWithHead = departments.Count(d => d.HeadOfDepartmentId.HasValue),
                DepartmentsWithoutHead = departments.Count(d => !d.HeadOfDepartmentId.HasValue),
                GeneratedAt = DateTime.UtcNow
            };

            var studentsByDept = await _context.Students
                .Where(s => !s.User.IsDeleted)
                .GroupBy(s => s.DepartmentId)
                .Select(g => new { DeptId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DeptId, x => x.Count);

            stats.TotalStudents = studentsByDept.Values.Sum();
            stats.AverageStudentsPerDepartment = departments.Count > 0
                ? Math.Round((double)stats.TotalStudents / departments.Count, 2)
                : 0;

            var instructorsByDept = await _context.Staff
                .Where(i => !i.User.IsDeleted)
                .GroupBy(i => i.DepartmentId)
                .Select(g => new { DeptId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DeptId, x => x.Count);

            stats.TotalInstructors = instructorsByDept.Values.Sum();
            stats.AverageInstructorsPerDepartment = departments.Count > 0
                ? Math.Round((double)stats.TotalInstructors / departments.Count, 2)
                : 0;

            stats.TotalCourses = await _context.ProgramPlan
                .CountAsync();

            stats.TopDepartmentsByStudents = departments
                .Where(d => studentsByDept.GetValueOrDefault(d.Id) > 0)
                .OrderByDescending(d => studentsByDept.GetValueOrDefault(d.Id))
                .Take(5)
                .ToDictionary(
                    d => d.DepartmentName,
                    d => studentsByDept.GetValueOrDefault(d.Id)
                );

            stats.TopDepartmentsByInstructors = departments
                .Where(d => instructorsByDept.GetValueOrDefault(d.Id) > 0)
                .OrderByDescending(d => instructorsByDept.GetValueOrDefault(d.Id))
                .Take(5)
                .ToDictionary(
                    d => d.DepartmentName,
                    d => instructorsByDept.GetValueOrDefault(d.Id)
                );

            stats.DepartmentsByFaculty = departments
                .SelectMany(d => d.DepartmentFaculties)
                .GroupBy(df => df.Faculty.Name.ToString())
                //.GroupBy(df => df.Faculty.NameEn)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );

            return stats;
        }

        #endregion

        #region Export Methods

        public async Task<byte[]> ExportDepartmentsToExcelAsync(DepartmentFilterDto? filter = null)
        {
            var departments = await GetDepartmentsAsync(filter ?? new DepartmentFilterDto { PageSize = 1000 });

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Departments");

                var headers = new[] {
                    "ID", "Department Name", "Base Code", "Faculties", "Head of Department",
                    "Duration (Years)", "Compulsory Hours", "Elective Hours", "Total Hours",
                    "Students", "Instructors", "Status", "Created Date"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                int row = 2;
                foreach (var dept in departments.Items)
                {
                    var faculties = string.Join(", ", dept.Faculties.Select(f => $"{f.FacultyName} ({f.DepartmentCode})"));

                    worksheet.Cell(row, 1).Value = dept.Id.ToString();
                    worksheet.Cell(row, 2).Value = dept.DepartmentName;
                    worksheet.Cell(row, 3).Value = dept.BaseDepartmentCode;
                    worksheet.Cell(row, 4).Value = faculties;
                    worksheet.Cell(row, 5).Value = dept.HeadOfDepartmentName ?? "Not Assigned";
                    worksheet.Cell(row, 6).Value = dept.DurationInYears;
                    worksheet.Cell(row, 7).Value = dept.CompulsoryHours;
                    worksheet.Cell(row, 8).Value = dept.ElectiveHours;
                    worksheet.Cell(row, 9).Value = dept.TotalHours;
                    worksheet.Cell(row, 10).Value = dept.StudentCount;
                    worksheet.Cell(row, 11).Value = dept.InstructorCount;
                    worksheet.Cell(row, 12).Value = dept.IsActive ? "Active" : "Inactive";
                    worksheet.Cell(row, 13).Value = dept.CreatedAt.ToString("yyyy-MM-dd");
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

        public async Task<byte[]> ExportDepartmentsToCsvAsync(DepartmentFilterDto? filter = null)
        {
            var departments = await GetDepartmentsAsync(filter ?? new DepartmentFilterDto { PageSize = 1000 });

            var csv = new StringBuilder();
            csv.AppendLine("ID,Department Name,Base Code,Faculties,Head of Department,Duration,Compulsory Hours,Elective Hours,Total Hours,Students,Instructors,Status,Created Date");

            foreach (var dept in departments.Items)
            {
                var faculties = string.Join("; ", dept.Faculties.Select(f => $"{f.FacultyName} ({f.DepartmentCode})"));
                csv.AppendLine($"\"{dept.Id}\",\"{dept.DepartmentName}\",\"{dept.BaseDepartmentCode}\",\"{faculties}\",\"{dept.HeadOfDepartmentName}\",{dept.DurationInYears},{dept.CompulsoryHours},{dept.ElectiveHours},{dept.TotalHours},{dept.StudentCount},{dept.InstructorCount},\"{(dept.IsActive ? "Active" : "Inactive")}\",\"{dept.CreatedAt:yyyy-MM-dd}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<string> GenerateDepartmentsReportAsync(DepartmentFilterDto? filter = null)
        {
            var departments = await GetDepartmentsAsync(filter ?? new DepartmentFilterDto());
            var stats = await GetDepartmentStatisticsAsync();

            var report = new StringBuilder();

            report.AppendLine("=== Department Management Report ===");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine("=== Summary ===");
            report.AppendLine($"Total Departments: {stats.TotalDepartments}");
            report.AppendLine($"Active Departments: {stats.ActiveDepartments}");
            report.AppendLine($"Inactive Departments: {stats.InactiveDepartments}");
            report.AppendLine($"Departments with Head: {stats.DepartmentsWithHead}");
            report.AppendLine($"Departments without Head: {stats.DepartmentsWithoutHead}");
            report.AppendLine($"Total Students: {stats.TotalStudents}");
            report.AppendLine($"Total Instructors: {stats.TotalInstructors}");
            report.AppendLine($"Total Courses: {stats.TotalCourses}");
            report.AppendLine($"Average Students/Department: {stats.AverageStudentsPerDepartment}");
            report.AppendLine($"Average Instructors/Department: {stats.AverageInstructorsPerDepartment}");
            report.AppendLine();
            report.AppendLine("=== Departments by Faculty ===");
            foreach (var kvp in stats.DepartmentsByFaculty)
                report.AppendLine($"{kvp.Key}: {kvp.Value} department(s)");
            report.AppendLine();
            report.AppendLine("=== Top Departments by Students ===");
            foreach (var kvp in stats.TopDepartmentsByStudents)
                report.AppendLine($"{kvp.Key}: {kvp.Value} students");
            report.AppendLine();
            report.AppendLine("=== Recent Departments (Last 10) ===");
            foreach (var dept in departments.Items.Take(10))
            {
                var faculties = string.Join(", ", dept.Faculties.Select(f => f.FacultyName));
                report.AppendLine($"{dept.DepartmentName} ({dept.BaseDepartmentCode}) - Faculties: {faculties} - Created: {dept.CreatedAt:yyyy-MM-dd}");
            }

            return report.ToString();
        }

        #endregion

        #region Private Helper Methods

        private IQueryable<Department> BuildDepartmentQuery(DepartmentFilterDto filter)
        {
            var query = _context.Departments
                .Include(d => d.DepartmentFaculties)
                    .ThenInclude(df => df.Faculty)
                .Include(d => d.HeadOfDepartment)
                    .ThenInclude(h => h.User)
                .Where(d => !d.IsDeleted)
                .AsQueryable();

            if (filter == null)
                return query;

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(d =>
                    d.DepartmentName.ToLower().Contains(searchTerm) ||
                    d.BaseDepartmentCode.ToLower().Contains(searchTerm) ||
                    d.DepartmentFaculties.Any(df => df.Faculty.Name.ToString().ToLower().Contains(searchTerm)));
                //d.DepartmentFaculties.Any(df => df.Faculty.NameEn.ToLower().Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(filter.DepartmentName))
            {
                query = query.Where(d => d.DepartmentName == filter.DepartmentName);
            }

            if (filter.FacultyIds != null && filter.FacultyIds.Any())
            {
                query = query.Where(d => d.DepartmentFaculties.Any(df => filter.FacultyIds.Contains(df.FacultyId)));
            }

            if (!string.IsNullOrWhiteSpace(filter.DepartmentCode))
            {
                query = query.Where(d => d.DepartmentFaculties.Any(df => df.DepartmentCode == filter.DepartmentCode));
            }

            if (filter.HasHead.HasValue)
            {
                if (filter.HasHead.Value)
                    query = query.Where(d => d.HeadOfDepartmentId != null);
                else
                    query = query.Where(d => d.HeadOfDepartmentId == null);
            }

            if (filter.IsActive.HasValue)
                query = query.Where(d => d.IsActive == filter.IsActive.Value);

            if (filter.MinStudents.HasValue)
            {
                query = query.Where(d => _context.Students.Count(s => s.DepartmentId == d.Id && !s.User.IsDeleted) >= filter.MinStudents);
            }

            if (filter.MinInstructors.HasValue)
            {
                query = query.Where(d => _context.Staff.Count(i => i.DepartmentId == d.Id && !i.User.IsDeleted) >= filter.MinInstructors);
            }

            if (filter.MinTotalHours.HasValue)
            {
                query = query.Where(d => (d.CompulsoryHours + d.ElectiveHours) >= filter.MinTotalHours);
            }

            if (filter.CreatedFrom.HasValue)
                query = query.Where(d => d.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(d => d.CreatedAt <= filter.CreatedTo.Value);

            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(d => d.DepartmentName)
                    : query.OrderBy(d => d.DepartmentName),
                "code" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(d => d.BaseDepartmentCode)
                    : query.OrderBy(d => d.BaseDepartmentCode),
                "faculty" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(d => d.DepartmentFaculties.FirstOrDefault().Faculty.Name)
                    : query.OrderBy(d => d.DepartmentFaculties.FirstOrDefault().Faculty.Name),
                //? query.OrderByDescending(d => d.DepartmentFaculties.FirstOrDefault().Faculty.NameEn)
                //: query.OrderBy(d => d.DepartmentFaculties.FirstOrDefault().Faculty.NameEn),
                "head" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(d => d.HeadOfDepartment.User.FullName)
                    : query.OrderBy(d => d.HeadOfDepartment.User.FullName),
                "students" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(d => _context.Students.Count(s => s.DepartmentId == d.Id))
                    : query.OrderBy(d => _context.Students.Count(s => s.DepartmentId == d.Id)),
                "instructors" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(d => _context.Staff.Count(i => i.DepartmentId == d.Id))
                    : query.OrderBy(d => _context.Staff.Count(i => i.DepartmentId == d.Id)),
                "hours" => filter.SortOrder == "desc"
                    ? query.OrderByDescending(d => d.CompulsoryHours + d.ElectiveHours)
                    : query.OrderBy(d => d.CompulsoryHours + d.ElectiveHours),
                _ => filter.SortOrder == "desc"
                    ? query.OrderByDescending(d => d.DepartmentName)
                    : query.OrderBy(d => d.DepartmentName)
            };

            return query;
        }

        #endregion
    }
}