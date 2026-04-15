using HUP.Application.DTOs.LookupDtos;
using HUP.Application.Services.Interfaces;
using HUP.Core.Enums;
using HUP.Data;
using Microsoft.EntityFrameworkCore;

namespace HUP.Application.Services.Implementations
{
    public class LookupService : ILookupService
    {
        private readonly HupDbContext _context;

        public LookupService(HupDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FacultyLookupDto>> GetAllFacultiesAsync()
        {
            return await _context.Faculties
                .Where(f => f.IsActive && !f.IsDeleted)
                .Select(f => new FacultyLookupDto
                {
                    Id = f.Id,
                    Name = GetLocalizedName(f.Name, "en"),
                    DisplayName = f.DisplayName ?? GetLocalizedName(f.Name, "en"),
                    //Name = f.NameEn,
                    //DisplayName = f.NameEn,
                    Departments = f.DepartmentFaculties
                        .Where(df => df.Department.IsActive && !df.Department.IsDeleted)
                        .Select(df => new DepartmentLookupDto
                        {
                            Id = df.Department.Id,
                            Name = df.Department.DepartmentName,
                            FacultyId = f.Id,
                            DepartmentCode = df.DepartmentCode
                        }).ToList()
                })
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DepartmentLookupDto>> GetAllDepartmentsAsync()
        {
            return await _context.DepartmentFaculties
                .Where(df => df.Department.IsActive && !df.Department.IsDeleted &&
                            df.Faculty.IsActive && !df.Faculty.IsDeleted)
                .Select(df => new DepartmentLookupDto
                {
                    Id = df.Department.Id,
                    Name = df.Department.DepartmentName,
                    FacultyId = df.FacultyId,
                    //FacultyName = df.Faculty.NameEn,
                    FacultyName = GetLocalizedName(df.Faculty.Name, "en"),
                    DepartmentCode = df.DepartmentCode,
                    IsPrimary = df.IsPrimary
                })
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<DepartmentLookupDto>> GetDepartmentsByFacultyAsync(Guid facultyId)
        {
            return await _context.DepartmentFaculties
                .Where(df => df.FacultyId == facultyId &&
                            df.Faculty.IsActive && !df.Faculty.IsDeleted &&
                            df.Department.IsActive && !df.Department.IsDeleted)
                .Select(df => new DepartmentLookupDto
                {
                    Id = df.Department.Id,
                    Name = df.Department.DepartmentName,
                    FacultyId = df.FacultyId,
                    FacultyName = GetLocalizedName(df.Faculty.Name, "en"),
                    //FacultyName = df.Faculty.NameEn,
                    DepartmentCode = df.DepartmentCode,
                    IsPrimary = df.IsPrimary
                })
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoleLookupDto>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Where(r => r.IsActive && !r.IsDeleted)
                .Select(r => new RoleLookupDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    DisplayName = r.DisplayName
                })
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public Task<IEnumerable<string>> GetAllUserTypesAsync()
        {
            var types = new[] { "Student", "Instructor", "Admin" };
            return Task.FromResult(types.AsEnumerable());
        }

        public Task<IEnumerable<AcademicStatus>> GetAllAcademicStatusesAsync()
        {
            var statuses = Enum.GetValues(typeof(AcademicStatus))
                .Cast<AcademicStatus>();
            return Task.FromResult(statuses);
        }

        #region Private Helper Methods

        private string GetLocalizedName(FacultyTitle title, string language)
        {
            if (language.ToLower() == "ar")
            {
                return title switch
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
                };
            }

            // Default to English (enum name)
            return title.ToString();
        }

        #endregion
    }
}