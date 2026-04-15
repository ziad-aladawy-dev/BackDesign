using HUP.Core.Entities.Academics;
using HUP.Core.Entities.Identity;
using HUP.Core.Entities.Permissions;
using HUP.Core.Entities.Financial;
using HUP.Core.Enums;
using HUP.Core.Enums.Financial;
using HUP.Core.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace HUP.Data.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HupDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

            await context.Database.EnsureCreatedAsync();

            // Check if already seeded
            if (await context.Users.AnyAsync())
                return;

            // ========== 1. PERMISSIONS ==========
            var permissions = new List<Permission>();
            foreach (var permName in AppPermissions.GetAll())
            {
                permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = permName,
                    DisplayName = permName.Replace(".", " "),
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                });
            }
            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();

            // ========== 2. ROLES ==========
            var roles = new List<Role>
            {
                new Role { Id = Guid.NewGuid(), Name = "SuperAdmin", DisplayName = "Super Administrator", Description = "Full system access", CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true },
                new Role { Id = Guid.NewGuid(), Name = "Admin", DisplayName = "Administrator", Description = "Administrative access", CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true },
                new Role { Id = Guid.NewGuid(), Name = "FacultyDean", DisplayName = "Faculty Dean", Description = "Faculty level management", CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true },
                new Role { Id = Guid.NewGuid(), Name = "DepartmentHead", DisplayName = "Department Head", Description = "Department level management", CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true },
                new Role { Id = Guid.NewGuid(), Name = "Staff", DisplayName = "Staff Member", Description = "Teaching or administrative staff", CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true },
                new Role { Id = Guid.NewGuid(), Name = "Student", DisplayName = "Student", Description = "Enrolled student", CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();

            // Assign all permissions to SuperAdmin
            var superAdminRole = roles.First(r => r.Name == "SuperAdmin");
            foreach (var perm in permissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = superAdminRole.Id,
                    PermissionId = perm.Id
                });
            }
            await context.SaveChangesAsync();

            // ========== 3. USERS ==========
            var users = new List<User>();
            var superAdminRoleId = roles.First(r => r.Name == "SuperAdmin").Id;
            var adminRoleId = roles.First(r => r.Name == "Admin").Id;
            var deanRoleId = roles.First(r => r.Name == "FacultyDean").Id;
            var deptHeadRoleId = roles.First(r => r.Name == "DepartmentHead").Id;
            var staffRoleId = roles.First(r => r.Name == "Staff").Id;
            var studentRoleId = roles.First(r => r.Name == "Student").Id;

            User CreateUser(string fullName, string email, string nationalId, Guid roleId, string phone = "01000000000")
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = fullName,
                    Email = email,
                    NationalId = nationalId,
                    RoleId = roleId,
                    IsActive = true,
                    PasswordExpiryDate = DateTime.UtcNow.AddYears(1),
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                user.PasswordHash = passwordHasher.HashPassword(user, "P@ssw0rd123");

                return user;
            }

            // Create Users
            var superAdminUser = CreateUser("Ahmed Mansour", "superadmin@hup.edu", "10000000000001", superAdminRoleId, "01000000001");
            var adminUser = CreateUser("Mohamed Ibrahim", "admin@hup.edu", "10000000000002", adminRoleId, "01000000002");
            var deanEngineering = CreateUser("Khaled Youssef", "dean.engineering@hup.edu", "10000000000003", deanRoleId, "01000000003");
            var deanFCI = CreateUser("Tamer Hassan", "dean.fci@hup.edu", "10000000000015", deanRoleId, "01000000015");
            var deanCommerce = CreateUser("Hany Mahmoud", "dean.commerce@hup.edu", "10000000000016", deanRoleId, "01000000016");
            var deanScience = CreateUser("Nadia Ahmed", "dean.science@hup.edu", "10000000000017", deanRoleId, "01000000017");

            var hodCS = CreateUser("Sara Kamel", "hod.cs@hup.edu", "10000000000004", deptHeadRoleId, "01000000004");
            var hodSWE = CreateUser("Ali Hassan", "hod.swe@hup.edu", "10000000000005", deptHeadRoleId, "01000000005");
            var hodMECH = CreateUser("Omar Farouk", "hod.mech@hup.edu", "10000000000018", deptHeadRoleId, "01000000018");
            var hodEE = CreateUser("Mahmoud Reda", "hod.ee@hup.edu", "10000000000019", deptHeadRoleId, "01000000019");
            var hodBA = CreateUser("Nourhan Hesham", "hod.ba@hup.edu", "10000000000020", deptHeadRoleId, "01000000020");
            var hodMath = CreateUser("Laila Mohamed", "hod.math@hup.edu", "10000000000021", deptHeadRoleId, "01000000021");

            var staff1 = CreateUser("Mona Lotfy", "staff1@hup.edu", "10000000000006", staffRoleId, "01000000006");
            var staff2 = CreateUser("Hany Mahmoud", "staff2@hup.edu", "10000000000007", staffRoleId, "01000000007");
            var staff3 = CreateUser("Nadia Ahmed", "staff3@hup.edu", "10000000000008", staffRoleId, "01000000008");
            var staff4 = CreateUser("Omar El-Sayed", "staff4@hup.edu", "10000000000022", staffRoleId, "01000000022");
            var staff5 = CreateUser("Fatma Ibrahim", "staff5@hup.edu", "10000000000023", staffRoleId, "01000000023");
            var staff6 = CreateUser("Youssef Ali", "staff6@hup.edu", "10000000000024", staffRoleId, "01000000024");

            var student1 = CreateUser("Ahmed Ali", "student1@hup.edu", "10000000000009", studentRoleId, "01000000009");
            var student2 = CreateUser("Fatma Hassan", "student2@hup.edu", "10000000000010", studentRoleId, "01000000010");
            var student3 = CreateUser("Youssef Kamel", "student3@hup.edu", "10000000000011", studentRoleId, "01000000011");
            var student4 = CreateUser("Laila Mohamed", "student4@hup.edu", "10000000000012", studentRoleId, "01000000012");
            var student5 = CreateUser("Mahmoud Reda", "student5@hup.edu", "10000000000013", studentRoleId, "01000000013");
            var student6 = CreateUser("Nourhan Hesham", "student6@hup.edu", "10000000000014", studentRoleId, "01000000014");
            var student7 = CreateUser("Omar Farouk", "student7@hup.edu", "10000000000025", studentRoleId, "01000000025");
            var student8 = CreateUser("Sara Kamel", "student8@hup.edu", "10000000000026", studentRoleId, "01000000026");
            var student9 = CreateUser("Khaled Youssef", "student9@hup.edu", "10000000000027", studentRoleId, "01000000027");
            var student10 = CreateUser("Mona Lotfy", "student10@hup.edu", "10000000000028", studentRoleId, "01000000028");

            users.AddRange(new[] {
                superAdminUser, adminUser,
                deanEngineering, deanFCI, deanCommerce, deanScience,
                hodCS, hodSWE, hodMECH, hodEE, hodBA, hodMath,
                staff1, staff2, staff3, staff4, staff5, staff6,
                student1, student2, student3, student4, student5, student6, student7, student8, student9, student10
            });

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // Add PersonalInfo and ContactInfo
            foreach (var user in users)
            {
                user.PersonalInfo = new UserPersonalInfo
                {
                    UserId = user.Id,
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1985, 1, 1),
                    Religion = Religion.Muslim,
                    Nationality = Nationality.Egyptian,
                    BirthPlace = BirthPlace.Cairo
                };

                user.ContactInfo = new UserContact
                {
                    UserId = user.Id,
                    Address = "123 University Street, Cairo",
                    PhoneNumber = "01000000000",
                    AltEmail = user.Email,
                    City = City.Cairo
                };
            }
            await context.SaveChangesAsync();

            // ========== 4. FACULTIES ==========
            var faculties = new List<Faculty>
            {
                new Faculty
                {
                    Id = Guid.NewGuid(),
                    Code = "ENG",
                    Name = FacultyTitle.FacultyOfEngineering,
                    DisplayName = "Faculty of Engineering - Helwan University",
                    DeanId = deanEngineering.Id,
                    ContactInfo = "engineering@hup.edu, Phone: +202 12345678",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                },
                new Faculty
                {
                    Id = Guid.NewGuid(),
                    Code = "FCI",
                    Name = FacultyTitle.FacultyOfComputingAndAI,
                    DisplayName = "Faculty of Computers and Artificial Intelligence",
                    DeanId = deanFCI.Id,
                    ContactInfo = "fci@hup.edu, Phone: +202 87654321",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                },
                new Faculty
                {
                    Id = Guid.NewGuid(),
                    Code = "COM",
                    Name = FacultyTitle.FacultyOfCommerceAndBusinessAdministration,
                    DisplayName = "Faculty of Commerce and Business Administration",
                    DeanId = deanCommerce.Id,
                    ContactInfo = "commerce@hup.edu, Phone: +202 11223344",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                },
                new Faculty
                {
                    Id = Guid.NewGuid(),
                    Code = "SCI",
                    Name = FacultyTitle.FacultyOfScience,
                    DisplayName = "Faculty of Science",
                    DeanId = deanScience.Id,
                    ContactInfo = "science@hup.edu, Phone: +202 55667788",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                }
            };
            await context.Faculties.AddRangeAsync(faculties);
            await context.SaveChangesAsync();

            // ========== 5. DEPARTMENTS ==========
            var engineeringFaculty = faculties.First(f => f.Code == "ENG");
            var computingFaculty = faculties.First(f => f.Code == "FCI");
            var commerceFaculty = faculties.First(f => f.Code == "COM");
            var scienceFaculty = faculties.First(f => f.Code == "SCI");

            var departments = new List<Department>
            {
                new Department
                {
                    Id = Guid.NewGuid(),
                    DepartmentName = "Computer Science Department",
                    BaseDepartmentCode = "CS",
                    CompulsoryHours = 90,
                    ElectiveHours = 30,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                },
                new Department
                {
                    Id = Guid.NewGuid(),
                    DepartmentName = "Software Engineering Department",
                    BaseDepartmentCode = "SWE",
                    CompulsoryHours = 85,
                    ElectiveHours = 35,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                },
                new Department
                {
                    Id = Guid.NewGuid(),
                    DepartmentName = "Mechanical Engineering Department",
                    BaseDepartmentCode = "MECH",
                    CompulsoryHours = 120,
                    ElectiveHours = 40,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                },
                new Department
                {
                    Id = Guid.NewGuid(),
                    DepartmentName = "Electrical Engineering Department",
                    BaseDepartmentCode = "EE",
                    CompulsoryHours = 100,
                    ElectiveHours = 30,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                },
                new Department
                {
                    Id = Guid.NewGuid(),
                    DepartmentName = "Business Administration Department",
                    BaseDepartmentCode = "BA",
                    CompulsoryHours = 80,
                    ElectiveHours = 40,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                },
                new Department
                {
                    Id = Guid.NewGuid(),
                    DepartmentName = "Mathematics Department",
                    BaseDepartmentCode = "MATH",
                    CompulsoryHours = 85,
                    ElectiveHours = 35,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                }
            };
            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();

            var csDept = departments.First(d => d.BaseDepartmentCode == "CS");
            var sweDept = departments.First(d => d.BaseDepartmentCode == "SWE");
            var mechDept = departments.First(d => d.BaseDepartmentCode == "MECH");
            var eeDept = departments.First(d => d.BaseDepartmentCode == "EE");
            var baDept = departments.First(d => d.BaseDepartmentCode == "BA");
            var mathDept = departments.First(d => d.BaseDepartmentCode == "MATH");

            // ========== 6. DEPARTMENT FACULTIES (Junction Table) ==========
            var departmentFaculties = new List<DepartmentFaculty>
            {
                new DepartmentFaculty { DepartmentId = csDept.Id, FacultyId = computingFaculty.Id, DepartmentCode = $"{computingFaculty.Code}-CS", IsPrimary = true, CreatedAt = DateTime.UtcNow },
                new DepartmentFaculty { DepartmentId = sweDept.Id, FacultyId = computingFaculty.Id, DepartmentCode = $"{computingFaculty.Code}-SWE", IsPrimary = true, CreatedAt = DateTime.UtcNow },
                new DepartmentFaculty { DepartmentId = mechDept.Id, FacultyId = engineeringFaculty.Id, DepartmentCode = $"{engineeringFaculty.Code}-MECH", IsPrimary = true, CreatedAt = DateTime.UtcNow },
                new DepartmentFaculty { DepartmentId = eeDept.Id, FacultyId = engineeringFaculty.Id, DepartmentCode = $"{engineeringFaculty.Code}-EE", IsPrimary = true, CreatedAt = DateTime.UtcNow },
                new DepartmentFaculty { DepartmentId = baDept.Id, FacultyId = commerceFaculty.Id, DepartmentCode = $"{commerceFaculty.Code}-BA", IsPrimary = true, CreatedAt = DateTime.UtcNow },
                new DepartmentFaculty { DepartmentId = mathDept.Id, FacultyId = scienceFaculty.Id, DepartmentCode = $"{scienceFaculty.Code}-MATH", IsPrimary = true, CreatedAt = DateTime.UtcNow }
            };
            await context.DepartmentFaculties.AddRangeAsync(departmentFaculties);
            await context.SaveChangesAsync();

            // ========== 7. STAFF ==========
            var staffMembers = new List<Staff>
            {
                new Staff { UserId = staff1.Id, DepartmentId = csDept.Id, FacultyId = computingFaculty.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic },
                new Staff { UserId = staff2.Id, DepartmentId = sweDept.Id, FacultyId = computingFaculty.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic },
                new Staff { UserId = staff3.Id, DepartmentId = mechDept.Id, FacultyId = engineeringFaculty.Id, Title = StaffTitle.AssistantProfessor, Category = StaffCategory.Academic },
                new Staff { UserId = staff4.Id, DepartmentId = eeDept.Id, FacultyId = engineeringFaculty.Id, Title = StaffTitle.TeachingAssistant, Category = StaffCategory.Academic },
                new Staff { UserId = staff5.Id, DepartmentId = baDept.Id, FacultyId = commerceFaculty.Id, Title = StaffTitle.Instructor, Category = StaffCategory.Academic },
                new Staff { UserId = staff6.Id, DepartmentId = mathDept.Id, FacultyId = scienceFaculty.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic },
                // Department Heads
                new Staff { UserId = hodCS.Id, DepartmentId = csDept.Id, FacultyId = computingFaculty.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic },
                new Staff { UserId = hodSWE.Id, DepartmentId = sweDept.Id, FacultyId = computingFaculty.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic },
                new Staff { UserId = hodMECH.Id, DepartmentId = mechDept.Id, FacultyId = engineeringFaculty.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic },
                new Staff { UserId = hodEE.Id, DepartmentId = eeDept.Id, FacultyId = engineeringFaculty.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic },
                new Staff { UserId = hodBA.Id, DepartmentId = baDept.Id, FacultyId = commerceFaculty.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic },
                new Staff { UserId = hodMath.Id, DepartmentId = mathDept.Id, FacultyId = scienceFaculty.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic }
            };
            await context.Staff.AddRangeAsync(staffMembers);
            await context.SaveChangesAsync();

            // Set HeadOfDepartment in Department entities
            csDept.HeadOfDepartmentId = hodCS.Id;
            sweDept.HeadOfDepartmentId = hodSWE.Id;
            mechDept.HeadOfDepartmentId = hodMECH.Id;
            eeDept.HeadOfDepartmentId = hodEE.Id;
            baDept.HeadOfDepartmentId = hodBA.Id;
            mathDept.HeadOfDepartmentId = hodMath.Id;

            context.Departments.UpdateRange(csDept, sweDept, mechDept, eeDept, baDept, mathDept);
            await context.SaveChangesAsync();

            // ========== 8. STUDENTS ==========
            var students = new List<Student>
            {
                new Student { UserId = student1.Id, UniversityCode = "20240001", UniversityEmail = student1.Email, AcademicStatus = AcademicStatus.Active, FacultyID = computingFaculty.Id, DepartmentId = csDept.Id, Level = 1, Cgpa = 3.2m, Group = "A" },
                new Student { UserId = student2.Id, UniversityCode = "20240002", UniversityEmail = student2.Email, AcademicStatus = AcademicStatus.Active, FacultyID = computingFaculty.Id, DepartmentId = csDept.Id, Level = 2, Cgpa = 3.5m, Group = "B" },
                new Student { UserId = student3.Id, UniversityCode = "20240003", UniversityEmail = student3.Email, AcademicStatus = AcademicStatus.Active, FacultyID = computingFaculty.Id, DepartmentId = sweDept.Id, Level = 1, Cgpa = 2.9m, Group = "A" },
                new Student { UserId = student4.Id, UniversityCode = "20240004", UniversityEmail = student4.Email, AcademicStatus = AcademicStatus.Active, FacultyID = computingFaculty.Id, DepartmentId = sweDept.Id, Level = 3, Cgpa = 3.8m, Group = "A" },
                new Student { UserId = student5.Id, UniversityCode = "20240005", UniversityEmail = student5.Email, AcademicStatus = AcademicStatus.Active, FacultyID = computingFaculty.Id, DepartmentId = csDept.Id, Level = 1, Cgpa = 3.0m, Group = "A" },
                new Student { UserId = student6.Id, UniversityCode = "20240006", UniversityEmail = student6.Email, AcademicStatus = AcademicStatus.Active, FacultyID = computingFaculty.Id, DepartmentId = sweDept.Id, Level = 2, Cgpa = 3.1m, Group = "B" },
                new Student { UserId = student7.Id, UniversityCode = "20240007", UniversityEmail = student7.Email, AcademicStatus = AcademicStatus.Active, FacultyID = engineeringFaculty.Id, DepartmentId = mechDept.Id, Level = 1, Cgpa = 3.3m, Group = "A" },
                new Student { UserId = student8.Id, UniversityCode = "20240008", UniversityEmail = student8.Email, AcademicStatus = AcademicStatus.Active, FacultyID = engineeringFaculty.Id, DepartmentId = eeDept.Id, Level = 2, Cgpa = 3.4m, Group = "B" },
                new Student { UserId = student9.Id, UniversityCode = "20240009", UniversityEmail = student9.Email, AcademicStatus = AcademicStatus.Active, FacultyID = commerceFaculty.Id, DepartmentId = baDept.Id, Level = 1, Cgpa = 3.6m, Group = "A" },
                new Student { UserId = student10.Id, UniversityCode = "20240010", UniversityEmail = student10.Email, AcademicStatus = AcademicStatus.Active, FacultyID = scienceFaculty.Id, DepartmentId = mathDept.Id, Level = 1, Cgpa = 3.7m, Group = "A" }
            };
            await context.Students.AddRangeAsync(students);
            await context.SaveChangesAsync();

            // ========== 9. SEMESTERS ==========
            var semesters = new List<Semester>
            {
                new Semester { Id = Guid.NewGuid(), SemesterName = "Fall 2024", StartDate = new DateTime(2024, 9, 15), EndDate = new DateTime(2024, 12, 20), RegistrationDeadline = new DateTime(2024, 9, 10), DropDeadline = new DateTime(2024, 10, 15), IsActive = true, CreatedAt = DateTime.UtcNow, IsDeleted = false },
                new Semester { Id = Guid.NewGuid(), SemesterName = "Spring 2025", StartDate = new DateTime(2025, 2, 10), EndDate = new DateTime(2025, 5, 25), RegistrationDeadline = new DateTime(2025, 2, 5), DropDeadline = new DateTime(2025, 3, 20), IsActive = false, CreatedAt = DateTime.UtcNow, IsDeleted = false },
                new Semester { Id = Guid.NewGuid(), SemesterName = "Summer 2025", StartDate = new DateTime(2025, 6, 10), EndDate = new DateTime(2025, 8, 15), RegistrationDeadline = new DateTime(2025, 6, 5), DropDeadline = new DateTime(2025, 7, 1), IsActive = false, CreatedAt = DateTime.UtcNow, IsDeleted = false }
            };
            await context.Semesters.AddRangeAsync(semesters);
            await context.SaveChangesAsync();

            var fallSemester = semesters.First(s => s.SemesterName == "Fall 2024");

            // ========== 10. COURSES ==========
            var courses = new List<Course>();

            // CS Courses
            var cs101 = new Course { Id = Guid.NewGuid(), CourseCode = "CS101", CourseName = "Introduction to Programming", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var cs102 = new Course { Id = Guid.NewGuid(), CourseCode = "CS102", CourseName = "Object-Oriented Programming", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var cs201 = new Course { Id = Guid.NewGuid(), CourseCode = "CS201", CourseName = "Data Structures", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var cs202 = new Course { Id = Guid.NewGuid(), CourseCode = "CS202", CourseName = "Algorithms", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var cs301 = new Course { Id = Guid.NewGuid(), CourseCode = "CS301", CourseName = "Database Systems", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var cs302 = new Course { Id = Guid.NewGuid(), CourseCode = "CS302", CourseName = "Operating Systems", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var cs401 = new Course { Id = Guid.NewGuid(), CourseCode = "CS401", CourseName = "Computer Networks", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var csElective1 = new Course { Id = Guid.NewGuid(), CourseCode = "CSELECT1", CourseName = "Machine Learning", Credits = 3, CourseType = CourseType.Elective, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var csElective2 = new Course { Id = Guid.NewGuid(), CourseCode = "CSELECT2", CourseName = "Cloud Computing", Credits = 3, CourseType = CourseType.Elective, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };

            // SWE Courses
            var swe101 = new Course { Id = Guid.NewGuid(), CourseCode = "SWE101", CourseName = "Software Engineering Fundamentals", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var swe201 = new Course { Id = Guid.NewGuid(), CourseCode = "SWE201", CourseName = "Requirements Engineering", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var swe202 = new Course { Id = Guid.NewGuid(), CourseCode = "SWE202", CourseName = "Software Design & Architecture", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var swe301 = new Course { Id = Guid.NewGuid(), CourseCode = "SWE301", CourseName = "Software Testing & Quality", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var swe302 = new Course { Id = Guid.NewGuid(), CourseCode = "SWE302", CourseName = "Project Management", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var sweElective1 = new Course { Id = Guid.NewGuid(), CourseCode = "SWELECT1", CourseName = "DevOps", Credits = 3, CourseType = CourseType.Elective, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };

            // MECH Courses
            var mech101 = new Course { Id = Guid.NewGuid(), CourseCode = "MECH101", CourseName = "Engineering Mechanics", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var mech102 = new Course { Id = Guid.NewGuid(), CourseCode = "MECH102", CourseName = "Thermodynamics", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var mech201 = new Course { Id = Guid.NewGuid(), CourseCode = "MECH201", CourseName = "Fluid Mechanics", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var mech202 = new Course { Id = Guid.NewGuid(), CourseCode = "MECH202", CourseName = "Strength of Materials", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var mechElective1 = new Course { Id = Guid.NewGuid(), CourseCode = "MECHELECT1", CourseName = "Robotics", Credits = 3, CourseType = CourseType.Elective, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };

            // EE Courses
            var ee101 = new Course { Id = Guid.NewGuid(), CourseCode = "EE101", CourseName = "Circuit Analysis", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var ee102 = new Course { Id = Guid.NewGuid(), CourseCode = "EE102", CourseName = "Electronics", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var ee201 = new Course { Id = Guid.NewGuid(), CourseCode = "EE201", CourseName = "Digital Logic Design", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var eeElective1 = new Course { Id = Guid.NewGuid(), CourseCode = "EEELECT1", CourseName = "Embedded Systems", Credits = 3, CourseType = CourseType.Elective, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };

            // BA Courses
            var ba101 = new Course { Id = Guid.NewGuid(), CourseCode = "BA101", CourseName = "Principles of Management", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var ba102 = new Course { Id = Guid.NewGuid(), CourseCode = "BA102", CourseName = "Marketing Management", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var ba201 = new Course { Id = Guid.NewGuid(), CourseCode = "BA201", CourseName = "Financial Accounting", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var baElective1 = new Course { Id = Guid.NewGuid(), CourseCode = "BAELECT1", CourseName = "E-Commerce", Credits = 3, CourseType = CourseType.Elective, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };

            // MATH Courses
            var math101 = new Course { Id = Guid.NewGuid(), CourseCode = "MATH101", CourseName = "Calculus I", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var math102 = new Course { Id = Guid.NewGuid(), CourseCode = "MATH102", CourseName = "Calculus II", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var math201 = new Course { Id = Guid.NewGuid(), CourseCode = "MATH201", CourseName = "Linear Algebra", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var math202 = new Course { Id = Guid.NewGuid(), CourseCode = "MATH202", CourseName = "Differential Equations", Credits = 3, CourseType = CourseType.Mandatory, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };
            var mathElective1 = new Course { Id = Guid.NewGuid(), CourseCode = "MATHELECT1", CourseName = "Numerical Analysis", Credits = 3, CourseType = CourseType.Elective, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true };

            courses.AddRange(new[] {
                cs101, cs102, cs201, cs202, cs301, cs302, cs401, csElective1, csElective2,
                swe101, swe201, swe202, swe301, swe302, sweElective1,
                mech101, mech102, mech201, mech202, mechElective1,
                ee101, ee102, ee201, eeElective1,
                ba101, ba102, ba201, baElective1,
                math101, math102, math201, math202, mathElective1
            });

            // Set prerequisites
            cs102.PrerequisiteId = cs101.Id;
            cs201.PrerequisiteId = cs102.Id;
            cs202.PrerequisiteId = cs201.Id;
            cs301.PrerequisiteId = cs202.Id;
            swe201.PrerequisiteId = swe101.Id;
            swe202.PrerequisiteId = swe201.Id;
            mech102.PrerequisiteId = mech101.Id;
            mech201.PrerequisiteId = mech102.Id;
            ee102.PrerequisiteId = ee101.Id;
            math102.PrerequisiteId = math101.Id;
            math201.PrerequisiteId = math102.Id;

            await context.Courses.AddRangeAsync(courses);
            await context.SaveChangesAsync();

            // ========== 11. PROGRAM PLANS ==========
            var programPlans = new List<ProgramPlan>();

            // CS Program Plan
            var csCoursesList = courses.Where(c => c.CourseCode.StartsWith("CS") || c.CourseCode == "MATH101" || c.CourseCode == "MATH102").ToList();
            foreach (var course in csCoursesList)
            {
                programPlans.Add(new ProgramPlan
                {
                    DepartmentId = csDept.Id,
                    CourseId = course.Id,
                    RequirementType = course.CourseCode.StartsWith("MATH") ? RequirementType.University : RequirementType.Department,
                    IsCompulsory = !course.CourseCode.Contains("ELECT"),
                    FinalGrade = 100
                });
            }

            // SWE Program Plan
            var sweCoursesList = courses.Where(c => c.CourseCode.StartsWith("SWE") || c.CourseCode == "CS101" || c.CourseCode == "CS201").ToList();
            foreach (var course in sweCoursesList)
            {
                programPlans.Add(new ProgramPlan
                {
                    DepartmentId = sweDept.Id,
                    CourseId = course.Id,
                    RequirementType = RequirementType.Department,
                    IsCompulsory = !course.CourseCode.Contains("ELECT"),
                    FinalGrade = 100
                });
            }

            // MECH Program Plan
            var mechCoursesList = courses.Where(c => c.CourseCode.StartsWith("MECH") || c.CourseCode.StartsWith("MATH")).ToList();
            foreach (var course in mechCoursesList)
            {
                programPlans.Add(new ProgramPlan
                {
                    DepartmentId = mechDept.Id,
                    CourseId = course.Id,
                    RequirementType = RequirementType.Department,
                    IsCompulsory = !course.CourseCode.Contains("ELECT"),
                    FinalGrade = 100
                });
            }

            // EE Program Plan
            var eeCoursesList = courses.Where(c => c.CourseCode.StartsWith("EE") || c.CourseCode.StartsWith("MATH")).ToList();
            foreach (var course in eeCoursesList)
            {
                programPlans.Add(new ProgramPlan
                {
                    DepartmentId = eeDept.Id,
                    CourseId = course.Id,
                    RequirementType = RequirementType.Department,
                    IsCompulsory = !course.CourseCode.Contains("ELECT"),
                    FinalGrade = 100
                });
            }

            // BA Program Plan
            var baCoursesList = courses.Where(c => c.CourseCode.StartsWith("BA") || c.CourseCode == "MATH101").ToList();
            foreach (var course in baCoursesList)
            {
                programPlans.Add(new ProgramPlan
                {
                    DepartmentId = baDept.Id,
                    CourseId = course.Id,
                    RequirementType = RequirementType.Department,
                    IsCompulsory = !course.CourseCode.Contains("ELECT"),
                    FinalGrade = 100
                });
            }

            // MATH Program Plan
            var mathCoursesList = courses.Where(c => c.CourseCode.StartsWith("MATH")).ToList();
            foreach (var course in mathCoursesList)
            {
                programPlans.Add(new ProgramPlan
                {
                    DepartmentId = mathDept.Id,
                    CourseId = course.Id,
                    RequirementType = RequirementType.Department,
                    IsCompulsory = !course.CourseCode.Contains("ELECT"),
                    FinalGrade = 100
                });
            }

            await context.ProgramPlan.AddRangeAsync(programPlans);
            await context.SaveChangesAsync();

            // ========== 12. COURSE OFFERINGS ==========
            var courseOfferings = new List<CourseOffering>();
            var random = new Random();

            foreach (var course in courses.Where(c => !c.CourseCode.Contains("ELECT")))
            {
                var deptId = course.CourseCode.StartsWith("CS") ? csDept.Id :
                            course.CourseCode.StartsWith("SWE") ? sweDept.Id :
                            course.CourseCode.StartsWith("MECH") ? mechDept.Id :
                            course.CourseCode.StartsWith("EE") ? eeDept.Id :
                            course.CourseCode.StartsWith("BA") ? baDept.Id :
                            course.CourseCode.StartsWith("MATH") ? mathDept.Id : csDept.Id;

                courseOfferings.Add(new CourseOffering
                {
                    Id = Guid.NewGuid(),
                    CourseId = course.Id,
                    SemesterId = fallSemester.Id,
                    DepartmentId = deptId,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                });
            }
            await context.CourseOfferings.AddRangeAsync(courseOfferings);
            await context.SaveChangesAsync();

            // ========== 13. SCHEDULES ==========
            var schedules = new List<Schedule>();
            var days = new[] { Core.Enums.DayOfWeek.Sunday, Core.Enums.DayOfWeek.Monday, Core.Enums.DayOfWeek.Tuesday, Core.Enums.DayOfWeek.Wednesday, Core.Enums.DayOfWeek.Thursday };
            var startTimes = new[] { new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0), new TimeSpan(13, 0, 0), new TimeSpan(15, 0, 0) };
            var halls = new[] { "Hall A", "Hall B", "Lab 101", "Lab 102", "Room 201", "Room 202" };

            var allCourseOfferings = await context.CourseOfferings.ToListAsync();
            int scheduleIndex = 0;

            foreach (var offering in allCourseOfferings)
            {
                var staffForDept = staffMembers.FirstOrDefault(s => s.DepartmentId == offering.DepartmentId);
                var staffId = staffForDept?.UserId ?? staff1.Id;
                var staffUser = users.FirstOrDefault(u => u.Id == staffId);

                var schedule = new Schedule
                {
                    Id = Guid.NewGuid(),
                    CourseOfferingId = offering.Id,
                    StaffId = staffId,
                    Group = $"Group {(scheduleIndex % 3) + 1}",
                    DayOfWeek = days[scheduleIndex % days.Length],
                    StartTime = startTimes[scheduleIndex % startTimes.Length],
                    EndTime = startTimes[scheduleIndex % startTimes.Length].Add(new TimeSpan(1, 50, 0)),
                    Hall = halls[scheduleIndex % halls.Length],
                    StaffName = staffUser?.FullName ?? "Staff",
                    TotalSeats = 50,
                    AvailableSeats = 50 - random.Next(0, 30),
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                };
                schedules.Add(schedule);
                scheduleIndex++;
            }
            await context.Schedules.AddRangeAsync(schedules);
            await context.SaveChangesAsync();

            // ========== 14. EXAMS ==========
            var exams = new List<Exam>();
            var examStartDate = new DateOnly(2024, 12, 10);
            int examIndex = 0;

            foreach (var offering in courseOfferings)
            {
                exams.Add(new Exam
                {
                    Id = Guid.NewGuid(),
                    CourseOfferingId = offering.Id,
                    ExamType = ExamType.Midterm,
                    ExamDate = examStartDate.AddDays(examIndex % 20),
                    ExamTime = new TimeOnly(10, 0, 0),
                    Location = halls[examIndex % halls.Length],
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                });

                exams.Add(new Exam
                {
                    Id = Guid.NewGuid(),
                    CourseOfferingId = offering.Id,
                    ExamType = ExamType.Final,
                    ExamDate = examStartDate.AddDays(25 + (examIndex % 10)),
                    ExamTime = new TimeOnly(13, 0, 0),
                    Location = "Main Auditorium",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsActive = true
                });
                examIndex++;
            }
            await context.Exams.AddRangeAsync(exams);
            await context.SaveChangesAsync();

            // ========== 15. ENROLLMENTS ==========
            var enrollments = new List<Enrollment>();
            var enrollmentDate = new DateTime(2024, 9, 1);
            int enrollmentIndex = 0;

            var scheduleLookup = schedules.ToDictionary(s => s.CourseOfferingId);

            foreach (var student in students)
            {
                var deptOfferings = courseOfferings.Where(o => o.DepartmentId == student.DepartmentId).Take(4).ToList();
                foreach (var offering in deptOfferings)
                {
                    var schedule = scheduleLookup.GetValueOrDefault(offering.Id);
                    if (schedule == null) continue;

                    var classGrade = random.Next(0, 30);
                    var midtermGrade = random.Next(0, 30);
                    var finalGrade = random.Next(0, 40);
                    var totalGrade = classGrade + midtermGrade + finalGrade;

                    var status = totalGrade >= 60 ? EnrollmentStatus.Completed :
                                 totalGrade >= 50 ? EnrollmentStatus.InProgress :
                                 totalGrade >= 40 ? EnrollmentStatus.Incomplete : EnrollmentStatus.Failed;

                    enrollments.Add(new Enrollment
                    {
                        Id = Guid.NewGuid(),
                        StudentId = student.UserId,
                        CourseOfferingId = offering.Id,
                        ScheduleId = schedule.Id,
                        EnrollmentDate = enrollmentDate.AddDays(enrollmentIndex % 5),
                        ClassGrade = classGrade,
                        MidtermGrade = midtermGrade,
                        finalGrade = finalGrade,
                        Status = status,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    });
                    enrollmentIndex++;
                }
            }
            await context.Enrollments.AddRangeAsync(enrollments);
            await context.SaveChangesAsync();

            // ========== 16. FEES ==========
            var fees = new List<Fee>
            {
                new Fee { Id = Guid.NewGuid(), Name = "Tuition Fee - Fall 2024", Description = "Tuition fee per credit hour for Fall 2024 semester", Amount = 550m, IsPerCredit = true, Type = FeeType.Tuition, SemesterId = fallSemester.Id, DepartmentId = csDept.Id, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true },
                new Fee { Id = Guid.NewGuid(), Name = "Bus Service Fee", Description = "Transportation service for the semester", Amount = 1500m, IsPerCredit = false, Type = FeeType.Bus, SemesterId = fallSemester.Id, DepartmentId = csDept.Id, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true },
                new Fee { Id = Guid.NewGuid(), Name = "Laboratory Fee", Description = "Laboratory equipment and materials fee", Amount = 350m, IsPerCredit = false, Type = FeeType.Lab, SemesterId = fallSemester.Id, DepartmentId = csDept.Id, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true },
                new Fee { Id = Guid.NewGuid(), Name = "Student Activities Fee", Description = "Extracurricular and student club activities", Amount = 200m, IsPerCredit = false, Type = FeeType.Activity, SemesterId = fallSemester.Id, DepartmentId = csDept.Id, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true },
                new Fee { Id = Guid.NewGuid(), Name = "Library Fee", Description = "Library access and digital resources", Amount = 100m, IsPerCredit = false, Type = FeeType.Other, SemesterId = fallSemester.Id, DepartmentId = csDept.Id, CreatedAt = DateTime.UtcNow, IsDeleted = false, IsActive = true }
            };
            await context.Fees.AddRangeAsync(fees);
            await context.SaveChangesAsync();

            // ========== 17. STUDENT FEES ==========
            var studentFees = new List<StudentFee>();
            var dueDate = new DateTime(2024, 10, 15);
            int studentFeeIndex = 0;

            foreach (var student in students)
            {
                int totalCredits = student.Level == 1 ? 15 : student.Level == 2 ? 16 : student.Level == 3 ? 17 : 18;

                foreach (var fee in fees)
                {
                    decimal amount = fee.IsPerCredit ? fee.Amount * totalCredits : fee.Amount;
                    decimal paidAmount;
                    FeeStatus status;

                    if (studentFeeIndex % 4 == 0)
                    {
                        paidAmount = amount;
                        status = FeeStatus.Paid;
                    }
                    else if (studentFeeIndex % 4 == 1)
                    {
                        paidAmount = amount * 0.5m;
                        status = FeeStatus.PartiallyPaid;
                    }
                    else if (studentFeeIndex % 4 == 2)
                    {
                        paidAmount = 0;
                        status = FeeStatus.Overdue;
                    }
                    else
                    {
                        paidAmount = 0;
                        status = FeeStatus.Pending;
                    }

                    studentFees.Add(new StudentFee
                    {
                        Id = Guid.NewGuid(),
                        StudentId = student.UserId,
                        FeeId = fee.Id,
                        Amount = amount,
                        PaidAmount = paidAmount,
                        Status = status,
                        DueDate = dueDate,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    });
                    studentFeeIndex++;
                }
            }
            await context.StudentFees.AddRangeAsync(studentFees);
            await context.SaveChangesAsync();

            // ========== 18. PAYMENTS ==========
            var payments = new List<Payment>();
            var paymentDate = new DateTime(2024, 9, 20);
            int paymentIndex = 0;

            foreach (var studentFee in studentFees.Where(sf => sf.PaidAmount > 0))
            {
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    StudentFeeId = studentFee.Id,
                    Amount = studentFee.PaidAmount,
                    PaymentDate = paymentDate.AddDays(paymentIndex % 20),
                    Method = PaymentMethod.Online,
                    ReferenceNumber = $"TXN{10000 + paymentIndex}",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
                paymentIndex++;
            }
            await context.Payments.AddRangeAsync(payments);
            await context.SaveChangesAsync();
        }
    }
}

//using HUP.Core.Entities.Academics;
//using HUP.Core.Entities.Identity;
//using HUP.Core.Entities.Permissions;
//using HUP.Core.Entities.Financial;
//using HUP.Core.Enums;
//using HUP.Core.Enums.Financial;
//using HUP.Core.Constants;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.AspNetCore.Identity;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace HUP.Data.Seed
//{
//    public static class DatabaseSeeder
//    {
//        public static async Task SeedAsync(IServiceProvider serviceProvider)
//        {
//            using var scope = serviceProvider.CreateScope();
//            var context = scope.ServiceProvider.GetRequiredService<HupDbContext>();
//            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

//            await context.Database.EnsureCreatedAsync();

//            await ClearDataAsync(context);

//            // ========== 1. PERMISSIONS ==========
//            var permissions = new List<Permission>();
//            foreach (var permName in AppPermissions.GetAll())
//            {
//                permissions.Add(new Permission
//                {
//                    Id = Guid.NewGuid(),
//                    Name = permName,
//                    DisplayName = permName.Replace(".", " "),
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                });
//            }
//            context.Permissions.AddRange(permissions);
//            await context.SaveChangesAsync();

//            // ========== 2. ROLES ==========
//            var roles = new List<Role>
//            {
//                new Role { Id = Guid.NewGuid(), Name = "SuperAdmin", DisplayName = "Super Administrator", Description = "Full system access", CreatedAt = DateTime.UtcNow, IsDeleted = false },
//                new Role { Id = Guid.NewGuid(), Name = "Admin", DisplayName = "Administrator", Description = "Administrative access", CreatedAt = DateTime.UtcNow, IsDeleted = false },
//                new Role { Id = Guid.NewGuid(), Name = "FacultyDean", DisplayName = "Faculty Dean", Description = "Faculty level management", CreatedAt = DateTime.UtcNow, IsDeleted = false },
//                new Role { Id = Guid.NewGuid(), Name = "DepartmentHead", DisplayName = "Department Head", Description = "Department level management", CreatedAt = DateTime.UtcNow, IsDeleted = false },
//                new Role { Id = Guid.NewGuid(), Name = "Staff", DisplayName = "Staff Member", Description = "Teaching or administrative staff", CreatedAt = DateTime.UtcNow, IsDeleted = false },
//                new Role { Id = Guid.NewGuid(), Name = "Student", DisplayName = "Student", Description = "Enrolled student", CreatedAt = DateTime.UtcNow, IsDeleted = false }
//            };
//            context.Roles.AddRange(roles);
//            await context.SaveChangesAsync();

//            // Assign all permissions to SuperAdmin
//            var superAdminRole = roles.First(r => r.Name == "SuperAdmin");
//            foreach (var perm in permissions)
//            {
//                context.RolePermissions.Add(new RolePermission
//                {
//                    RoleId = superAdminRole.Id,
//                    PermissionId = perm.Id
//                });
//            }
//            await context.SaveChangesAsync();

//            // ========== 3. USERS ==========
//            var users = new List<User>();
//            var superAdminRoleId = roles.First(r => r.Name == "SuperAdmin").Id;
//            var adminRoleId = roles.First(r => r.Name == "Admin").Id;
//            var deanRoleId = roles.First(r => r.Name == "FacultyDean").Id;
//            var deptHeadRoleId = roles.First(r => r.Name == "DepartmentHead").Id;
//            var staffRoleId = roles.First(r => r.Name == "Staff").Id;
//            var studentRoleId = roles.First(r => r.Name == "Student").Id;

//            User CreateUser(string fullName, string email, string nationalId, Guid roleId, string phone = "01000000000")
//            {
//                var user = new User
//                {
//                    Id = Guid.NewGuid(),
//                    FullName = fullName,
//                    Email = email,
//                    NationalId = nationalId,
//                    RoleId = roleId,
//                    IsActive = true,
//                    PasswordExpiryDate = DateTime.UtcNow.AddYears(1),
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                };

//                user.PasswordHash = passwordHasher.HashPassword(user, "P@ssw0rd123");

//                // Create PersonalInfo and ContactInfo WITHOUT setting UserId yet
//                user.PersonalInfo = new UserPersonalInfo
//                {
//                    Gender = Gender.Male,
//                    BirthDate = new DateTime(1985, 1, 1),
//                    Religion = Religion.Muslim,
//                    Nationality = Nationality.Egyptian,
//                    BirthPlace = BirthPlace.Cairo
//                };

//                user.ContactInfo = new UserContact
//                {
//                    Address = "123 University Street, Cairo",
//                    PhoneNumber = phone,
//                    AltEmail = email,
//                    City = City.Cairo
//                };

//                return user;
//            }

//            var superAdminUser = CreateUser("Ahmed Mansour", "superadmin@hup.edu", "10000000000001", superAdminRoleId, "01000000001");
//            var adminUser = CreateUser("Mohamed Ibrahim", "admin@hup.edu", "10000000000002", adminRoleId, "01000000002");
//            var deanEngineering = CreateUser("Khaled Youssef", "dean.engineering@hup.edu", "10000000000003", deanRoleId, "01000000003");
//            var deanFCI = CreateUser("Tamer Hassan", "dean.fci@hup.edu", "10000000000015", deanRoleId, "01000000015");
//            var deanCommerce = CreateUser("Hany Mahmoud", "dean.commerce@hup.edu", "10000000000016", deanRoleId, "01000000016");
//            var deanScience = CreateUser("Nadia Ahmed", "dean.science@hup.edu", "10000000000017", deanRoleId, "01000000017");

//            var hodCS = CreateUser("Sara Kamel", "hod.cs@hup.edu", "10000000000004", deptHeadRoleId, "01000000004");
//            var hodSWE = CreateUser("Ali Hassan", "hod.swe@hup.edu", "10000000000005", deptHeadRoleId, "01000000005");
//            var hodMECH = CreateUser("Omar Farouk", "hod.mech@hup.edu", "10000000000018", deptHeadRoleId, "01000000018");

//            var staff1 = CreateUser("Mona Lotfy", "staff1@hup.edu", "10000000000006", staffRoleId, "01000000006");
//            var staff2 = CreateUser("Hany Mahmoud", "staff2@hup.edu", "10000000000007", staffRoleId, "01000000007");
//            var staff3 = CreateUser("Nadia Ahmed", "staff3@hup.edu", "10000000000008", staffRoleId, "01000000008");

//            var student1 = CreateUser("Omar El-Sayed", "student1@hup.edu", "10000000000009", studentRoleId, "01000000009");
//            var student2 = CreateUser("Fatma Ibrahim", "student2@hup.edu", "10000000000010", studentRoleId, "01000000010");
//            var student3 = CreateUser("Youssef Ali", "student3@hup.edu", "10000000000011", studentRoleId, "01000000011");
//            var student4 = CreateUser("Laila Mohamed", "student4@hup.edu", "10000000000012", studentRoleId, "01000000012");
//            var student5 = CreateUser("Mahmoud Reda", "student5@hup.edu", "10000000000013", studentRoleId, "01000000013");
//            var student6 = CreateUser("Nourhan Hesham", "student6@hup.edu", "10000000000014", studentRoleId, "01000000014");

//            users.AddRange(new[] { superAdminUser, adminUser, deanEngineering, deanFCI, deanCommerce, deanScience, hodCS, hodSWE, hodMECH, staff1, staff2, staff3, student1, student2, student3, student4, student5, student6 });

//            context.Users.AddRange(users);
//            await context.SaveChangesAsync();

//            // Fix UserPersonalInfo and UserContact UserId after save
//            foreach (var user in users)
//            {
//                if (user.PersonalInfo != null)
//                    user.PersonalInfo.UserId = user.Id;
//                if (user.ContactInfo != null)
//                    user.ContactInfo.UserId = user.Id;
//            }
//            await context.SaveChangesAsync();

//            // ========== 4. FACULTIES ==========
//            var faculties = new List<Faculty>
//            {
//                new Faculty
//                {
//                    Id = Guid.NewGuid(),
//                    Name = FacultyTitle.FacultyOfEngineering,
//                    DisplayName = "Faculty of Engineering - Helwan University",
//                    DeanId = deanEngineering.Id,
//                    //DeanName = deanEngineering.FullName,
//                    ContactInfo = "engineering@hup.edu, Phone: +202 12345678",
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                },
//                new Faculty
//                {
//                    Id = Guid.NewGuid(),
//                    Name = FacultyTitle.FacultyOfComputingAndAI,
//                    DisplayName = "Faculty of Computers and Artificial Intelligence",
//                    DeanId = deanFCI.Id,
//                    //DeanName = deanFCI.FullName,
//                    ContactInfo = "fci@hup.edu, Phone: +202 87654321",
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                },
//                new Faculty
//                {
//                    Id = Guid.NewGuid(),
//                    Name = FacultyTitle.FacultyOfCommerceAndBusinessAdministration,
//                    DisplayName = "Faculty of Commerce and Business Administration",
//                    DeanId = deanCommerce.Id,
//                    //DeanName = deanCommerce.FullName,
//                    ContactInfo = "commerce@hup.edu, Phone: +202 11223344",
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                },
//                new Faculty
//                {
//                    Id = Guid.NewGuid(),
//                    Name = FacultyTitle.FacultyOfScience,
//                    DisplayName = "Faculty of Science",
//                    DeanId = deanScience.Id,
//                    //DeanName = deanScience.FullName,
//                    ContactInfo = "science@hup.edu, Phone: +202 55667788",
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                }
//            };
//            context.Faculties.AddRange(faculties);
//            await context.SaveChangesAsync();

//            // ========== 5. DEPARTMENTS ==========
//            var engineeringFaculty = faculties.First(f => f.Name == FacultyTitle.FacultyOfEngineering);
//            var computingFaculty = faculties.First(f => f.Name == FacultyTitle.FacultyOfComputingAndAI);
//            var commerceFaculty = faculties.First(f => f.Name == FacultyTitle.FacultyOfCommerceAndBusinessAdministration);
//            var scienceFaculty = faculties.First(f => f.Name == FacultyTitle.FacultyOfScience);

//            var departments = new List<Department>
//            {
//                new Department
//                {
//                    Id = Guid.NewGuid(),
//                    DepartmentName = "Computer Science Department",
//                    //DepartmentCode = "CS",
//                    //FacultyId = computingFaculty.Id,
//                    //DurationInYears = 4,
//                    CompulsoryHours = 90,
//                    ElectiveHours = 30,
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                },
//                new Department
//                {
//                    Id = Guid.NewGuid(),
//                    DepartmentName = "Software Engineering Department",
//                    //DepartmentCode = "SWE",
//                    //FacultyId = computingFaculty.Id,
//                    //DurationInYears = 4,
//                    CompulsoryHours = 85,
//                    ElectiveHours = 35,
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                },
//                new Department
//                {
//                    Id = Guid.NewGuid(),
//                    DepartmentName = "Mechanical Engineering Department",
//                    //DepartmentCode = "MECH",
//                    //FacultyId = engineeringFaculty.Id,
//                    //DurationInYears = 5,
//                    CompulsoryHours = 120,
//                    ElectiveHours = 40,
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                },
//                new Department
//                {
//                    Id = Guid.NewGuid(),
//                    DepartmentName = "Electrical Engineering Department",
//                    //DepartmentCode = "EE",
//                    //FacultyId = engineeringFaculty.Id,
//                    //DurationInYears = 4,
//                    CompulsoryHours = 100,
//                    ElectiveHours = 30,
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                },
//                new Department
//                {
//                    Id = Guid.NewGuid(),
//                    DepartmentName = "Business Administration Department",
//                    //DepartmentCode = "BA",
//                    //FacultyId = commerceFaculty.Id,
//                    //DurationInYears = 4,
//                    CompulsoryHours = 80,
//                    ElectiveHours = 40,
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                },
//                new Department
//                {
//                    Id = Guid.NewGuid(),
//                    DepartmentName = "Mathematics Department",
//                    //DepartmentCode = "MATH",
//                    //FacultyId = scienceFaculty.Id,
//                    //DurationInYears = 4,
//                    CompulsoryHours = 85,
//                    ElectiveHours = 35,
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                }
//            };
//            context.Departments.AddRange(departments);
//            await context.SaveChangesAsync();

//            // ========== 6. STAFF ==========
//            //var csDept = departments.First(d => d.DepartmentCode == "CS");
//            //var sweDept = departments.First(d => d.DepartmentCode == "SWE");
//            //var mechDept = departments.First(d => d.DepartmentCode == "MECH");
//            //var eeDept = departments.First(d => d.DepartmentCode == "EE");
//            //var baDept = departments.First(d => d.DepartmentCode == "BA");
//            //var mathDept = departments.First(d => d.DepartmentCode == "MATH");

//            var staffMembers = new List<Staff>
//            {
//                new Staff { UserId = staff1.Id, DepartmentId = csDept.Id, FacultyId = computingFaculty.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic },
//                new Staff { UserId = staff2.Id, DepartmentId = sweDept.Id, FacultyId = computingFaculty.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic },
//                new Staff { UserId = staff3.Id, DepartmentId = mechDept.Id, FacultyId = engineeringFaculty.Id, Title = StaffTitle.AssistantProfessor, Category = StaffCategory.Academic },
//                // Department Heads (Staff with DepartmentHeadedId)
//                new Staff { UserId = hodCS.Id, DepartmentId = csDept.Id, FacultyId = computingFaculty.Id, DepartmentHeadedId = csDept.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic },
//                new Staff { UserId = hodSWE.Id, DepartmentId = sweDept.Id, FacultyId = computingFaculty.Id, DepartmentHeadedId = sweDept.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic },
//                new Staff { UserId = hodMECH.Id, DepartmentId = mechDept.Id, FacultyId = engineeringFaculty.Id, DepartmentHeadedId = mechDept.Id, Title = StaffTitle.Professor, Category = StaffCategory.Academic }
//            };
//            context.Staff.AddRange(staffMembers);
//            await context.SaveChangesAsync();

//            // Set HeadOfDepartment in Department entities
//            var csHead = staffMembers.First(s => s.DepartmentHeadedId == csDept.Id);
//            var sweHead = staffMembers.First(s => s.DepartmentHeadedId == sweDept.Id);
//            var mechHead = staffMembers.First(s => s.DepartmentHeadedId == mechDept.Id);

//            csDept.HeadOfDepartment = csHead;
//            sweDept.HeadOfDepartment = sweHead;
//            mechDept.HeadOfDepartment = mechHead;

//            context.Departments.UpdateRange(csDept, sweDept, mechDept);
//            await context.SaveChangesAsync();

//            // ========== 7. STUDENTS ==========
//            var students = new List<Student>
//            {
//                new Student { UserId = student1.Id, UniversityCode = "20240001", UniversityEmail = student1.Email, AcademicStatus = AcademicStatus.Active, DepartmentId = csDept.Id, Level = 1, Cgpa = 3.2m, Group = "A" },
//                new Student { UserId = student2.Id, UniversityCode = "20240002", UniversityEmail = student2.Email, AcademicStatus = AcademicStatus.Active, DepartmentId = csDept.Id, Level = 2, Cgpa = 3.5m, Group = "B" },
//                new Student { UserId = student3.Id, UniversityCode = "20240003", UniversityEmail = student3.Email, AcademicStatus = AcademicStatus.Active, DepartmentId = sweDept.Id, Level = 1, Cgpa = 2.9m, Group = "A" },
//                new Student { UserId = student4.Id, UniversityCode = "20240004", UniversityEmail = student4.Email, AcademicStatus = AcademicStatus.Active, DepartmentId = sweDept.Id, Level = 3, Cgpa = 3.8m, Group = "A" },
//                new Student { UserId = student5.Id, UniversityCode = "20240005", UniversityEmail = student5.Email, AcademicStatus = AcademicStatus.Active, DepartmentId = csDept.Id, Level = 1, Cgpa = 3.0m, Group = "A" },
//                new Student { UserId = student6.Id, UniversityCode = "20240006", UniversityEmail = student6.Email, AcademicStatus = AcademicStatus.Active, DepartmentId = sweDept.Id, Level = 2, Cgpa = 3.1m, Group = "B" }
//            };
//            context.Students.AddRange(students);
//            await context.SaveChangesAsync();

//            // ========== 8. SEMESTERS ==========
//            var semesters = new List<Semester>
//            {
//                new Semester { Id = Guid.NewGuid(), SemesterName = "Fall 2024", StartDate = new DateTime(2024, 9, 15), EndDate = new DateTime(2024, 12, 20), RegistrationDeadline = new DateTime(2024, 9, 10), DropDeadline = new DateTime(2024, 10, 15), IsActive = true, CreatedAt = DateTime.UtcNow, IsDeleted = false },
//                new Semester { Id = Guid.NewGuid(), SemesterName = "Spring 2025", StartDate = new DateTime(2025, 2, 10), EndDate = new DateTime(2025, 5, 25), RegistrationDeadline = new DateTime(2025, 2, 5), DropDeadline = new DateTime(2025, 3, 20), IsActive = false, CreatedAt = DateTime.UtcNow, IsDeleted = false },
//                new Semester { Id = Guid.NewGuid(), SemesterName = "Summer 2025", StartDate = new DateTime(2025, 6, 10), EndDate = new DateTime(2025, 8, 15), RegistrationDeadline = new DateTime(2025, 6, 5), DropDeadline = new DateTime(2025, 7, 1), IsActive = false, CreatedAt = DateTime.UtcNow, IsDeleted = false }
//            };
//            context.Semesters.AddRange(semesters);
//            await context.SaveChangesAsync();

//            // ========== 9. COURSES ==========
//            var courses = new List<Course>();

//            // CS Courses
//            var cs101 = new Course { Id = Guid.NewGuid(), CourseCode = "CS101", CourseName = "Introduction to Programming", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var cs102 = new Course { Id = Guid.NewGuid(), CourseCode = "CS102", CourseName = "Object-Oriented Programming", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var cs201 = new Course { Id = Guid.NewGuid(), CourseCode = "CS201", CourseName = "Data Structures", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var cs202 = new Course { Id = Guid.NewGuid(), CourseCode = "CS202", CourseName = "Algorithms", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var cs301 = new Course { Id = Guid.NewGuid(), CourseCode = "CS301", CourseName = "Database Systems", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var cs302 = new Course { Id = Guid.NewGuid(), CourseCode = "CS302", CourseName = "Operating Systems", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var cs401 = new Course { Id = Guid.NewGuid(), CourseCode = "CS401", CourseName = "Computer Networks", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };

//            // SWE Courses
//            var swe101 = new Course { Id = Guid.NewGuid(), CourseCode = "SWE101", CourseName = "Software Engineering Fundamentals", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var swe201 = new Course { Id = Guid.NewGuid(), CourseCode = "SWE201", CourseName = "Requirements Engineering", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var swe202 = new Course { Id = Guid.NewGuid(), CourseCode = "SWE202", CourseName = "Software Design & Architecture", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var swe301 = new Course { Id = Guid.NewGuid(), CourseCode = "SWE301", CourseName = "Software Testing & Quality", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var swe302 = new Course { Id = Guid.NewGuid(), CourseCode = "SWE302", CourseName = "Project Management", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };

//            // MECH Courses
//            var mech101 = new Course { Id = Guid.NewGuid(), CourseCode = "MECH101", CourseName = "Engineering Mechanics", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var mech102 = new Course { Id = Guid.NewGuid(), CourseCode = "MECH102", CourseName = "Thermodynamics", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var mech201 = new Course { Id = Guid.NewGuid(), CourseCode = "MECH201", CourseName = "Fluid Mechanics", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var mech202 = new Course { Id = Guid.NewGuid(), CourseCode = "MECH202", CourseName = "Strength of Materials", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };

//            // EE Courses
//            var ee101 = new Course { Id = Guid.NewGuid(), CourseCode = "EE101", CourseName = "Circuit Analysis", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var ee102 = new Course { Id = Guid.NewGuid(), CourseCode = "EE102", CourseName = "Electronics", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var ee201 = new Course { Id = Guid.NewGuid(), CourseCode = "EE201", CourseName = "Digital Logic Design", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };

//            // BA Courses
//            var ba101 = new Course { Id = Guid.NewGuid(), CourseCode = "BA101", CourseName = "Principles of Management", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var ba102 = new Course { Id = Guid.NewGuid(), CourseCode = "BA102", CourseName = "Marketing Management", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var ba201 = new Course { Id = Guid.NewGuid(), CourseCode = "BA201", CourseName = "Financial Accounting", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };

//            // MATH Courses
//            var math101 = new Course { Id = Guid.NewGuid(), CourseCode = "MATH101", CourseName = "Calculus I", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var math102 = new Course { Id = Guid.NewGuid(), CourseCode = "MATH102", CourseName = "Calculus II", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var math201 = new Course { Id = Guid.NewGuid(), CourseCode = "MATH201", CourseName = "Linear Algebra", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };
//            var math202 = new Course { Id = Guid.NewGuid(), CourseCode = "MATH202", CourseName = "Differential Equations", Credits = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false };

//            courses.AddRange(new[] { cs101, cs102, cs201, cs202, cs301, cs302, cs401, swe101, swe201, swe202, swe301, swe302, mech101, mech102, mech201, mech202, ee101, ee102, ee201, ba101, ba102, ba201, math101, math102, math201, math202 });

//            // Set prerequisites
//            cs102.PrerequisiteId = cs101.Id;
//            cs201.PrerequisiteId = cs102.Id;
//            cs202.PrerequisiteId = cs201.Id;
//            cs301.PrerequisiteId = cs202.Id;
//            swe201.PrerequisiteId = swe101.Id;
//            swe202.PrerequisiteId = swe201.Id;
//            mech102.PrerequisiteId = mech101.Id;
//            mech201.PrerequisiteId = mech102.Id;
//            ee102.PrerequisiteId = ee101.Id;
//            math102.PrerequisiteId = math101.Id;
//            math201.PrerequisiteId = math102.Id;

//            context.Courses.AddRange(courses);
//            await context.SaveChangesAsync();

//            // ========== 10. PROGRAM PLANS ==========
//            var programPlans = new List<ProgramPlan>();

//            // CS Program Plan
//            var csCoursesList = courses.Where(c => c.CourseCode.StartsWith("CS") || c.CourseCode.StartsWith("MATH")).ToList();
//            foreach (var course in csCoursesList)
//            {
//                programPlans.Add(new ProgramPlan
//                {
//                    DepartmentId = csDept.Id,
//                    CourseId = course.Id,
//                    RequirementType = course.CourseCode.StartsWith("MATH") ? RequirementType.University : RequirementType.Department,
//                    IsCompulsory = true,
//                    FinalGrade = 100
//                });
//            }

//            // SWE Program Plan
//            var sweCoursesList = courses.Where(c => c.CourseCode.StartsWith("SWE") || c.CourseCode == "CS101" || c.CourseCode == "CS201").ToList();
//            foreach (var course in sweCoursesList)
//            {
//                programPlans.Add(new ProgramPlan
//                {
//                    DepartmentId = sweDept.Id,
//                    CourseId = course.Id,
//                    RequirementType = RequirementType.Department,
//                    IsCompulsory = true,
//                    FinalGrade = 100
//                });
//            }

//            // MECH Program Plan
//            var mechCoursesList = courses.Where(c => c.CourseCode.StartsWith("MECH") || c.CourseCode.StartsWith("MATH")).ToList();
//            foreach (var course in mechCoursesList)
//            {
//                programPlans.Add(new ProgramPlan
//                {
//                    DepartmentId = mechDept.Id,
//                    CourseId = course.Id,
//                    RequirementType = RequirementType.Department,
//                    IsCompulsory = true,
//                    FinalGrade = 100
//                });
//            }

//            context.ProgramPlan.AddRange(programPlans);
//            await context.SaveChangesAsync();

//            // ========== 11. COURSE OFFERINGS ==========
//            var fallSemester = semesters.First(s => s.SemesterName == "Fall 2024");
//            var courseOfferings = new List<CourseOffering>();

//            foreach (var course in courses.Where(c => c.CourseCode == "CS101" || c.CourseCode == "CS102" || c.CourseCode == "CS201"))
//            {
//                courseOfferings.Add(new CourseOffering
//                {
//                    Id = Guid.NewGuid(),
//                    CourseId = course.Id,
//                    SemesterId = fallSemester.Id,
//                    DepartmentId = csDept.Id,
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                });
//            }

//            foreach (var course in courses.Where(c => c.CourseCode == "SWE101" || c.CourseCode == "SWE201"))
//            {
//                courseOfferings.Add(new CourseOffering
//                {
//                    Id = Guid.NewGuid(),
//                    CourseId = course.Id,
//                    SemesterId = fallSemester.Id,
//                    DepartmentId = sweDept.Id,
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                });
//            }

//            foreach (var course in courses.Where(c => c.CourseCode == "MECH101" || c.CourseCode == "MECH102"))
//            {
//                courseOfferings.Add(new CourseOffering
//                {
//                    Id = Guid.NewGuid(),
//                    CourseId = course.Id,
//                    SemesterId = fallSemester.Id,
//                    DepartmentId = mechDept.Id,
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                });
//            }

//            foreach (var course in courses.Where(c => c.CourseCode == "EE101"))
//            {
//                courseOfferings.Add(new CourseOffering
//                {
//                    Id = Guid.NewGuid(),
//                    CourseId = course.Id,
//                    SemesterId = fallSemester.Id,
//                    DepartmentId = eeDept.Id,
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                });
//            }

//            foreach (var course in courses.Where(c => c.CourseCode == "BA101"))
//            {
//                courseOfferings.Add(new CourseOffering
//                {
//                    Id = Guid.NewGuid(),
//                    CourseId = course.Id,
//                    SemesterId = fallSemester.Id,
//                    DepartmentId = baDept.Id,
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                });
//            }

//            foreach (var course in courses.Where(c => c.CourseCode == "MATH101" || c.CourseCode == "MATH102"))
//            {
//                courseOfferings.Add(new CourseOffering
//                {
//                    Id = Guid.NewGuid(),
//                    CourseId = course.Id,
//                    SemesterId = fallSemester.Id,
//                    DepartmentId = mathDept.Id,
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                });
//            }

//            context.CourseOfferings.AddRange(courseOfferings);
//            await context.SaveChangesAsync();

//            // ========== 12. SCHEDULES ==========
//            var schedules = new List<Schedule>();
//            var days = new[] { Core.Enums.DayOfWeek.Sunday, Core.Enums.DayOfWeek.Monday, Core.Enums.DayOfWeek.Tuesday, Core.Enums.DayOfWeek.Wednesday, Core.Enums.DayOfWeek.Thursday };
//            var startTimes = new[] { new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0), new TimeSpan(13, 0, 0), new TimeSpan(15, 0, 0) };
//            var halls = new[] { "Hall A", "Hall B", "Lab 101", "Lab 102", "Room 201", "Room 202" };

//            var allCourseOfferings = context.CourseOfferings.ToList(); 

//            int scheduleIndex = 0;
//            foreach (var offering in allCourseOfferings)
//            {
//                var departmentStaff = staffMembers.Where(s => s.DepartmentId == offering.DepartmentId).ToList();
//                var staffForSchedule = departmentStaff.Any() ? departmentStaff.First() : staffMembers.First();
//                var staffUser = users.FirstOrDefault(u => u.Id == staffForSchedule.UserId);

//                var schedule = new Schedule
//                {
//                    Id = Guid.NewGuid(),
//                    CourseOfferingId = offering.Id,
//                    StaffId = staffForSchedule.UserId,
//                    Group = $"Group {(scheduleIndex % 3) + 1}",
//                    DayOfWeek = days[scheduleIndex % days.Length],
//                    StartTime = startTimes[scheduleIndex % startTimes.Length],
//                    EndTime = startTimes[scheduleIndex % startTimes.Length].Add(new TimeSpan(1, 50, 0)),
//                    Hall = halls[scheduleIndex % halls.Length],
//                    StaffName = staffUser?.FullName ?? "Staff",
//                    TotalSeats = 50,
//                    AvailableSeats = 50 - (scheduleIndex % 25), // على الأقل 25 مقعد متاح
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                };
//                schedules.Add(schedule);
//                scheduleIndex++;
//            }
//            context.Schedules.AddRange(schedules);
//            await context.SaveChangesAsync();

//            // ========== 13. EXAMS ==========
//            var exams = new List<Exam>();
//            var examStartDate = new DateOnly(2024, 12, 10);
//            int examIndex = 0;

//            foreach (var offering in courseOfferings)
//            {
//                exams.Add(new Exam
//                {
//                    Id = Guid.NewGuid(),
//                    CourseOfferingId = offering.Id,
//                    ExamType = ExamType.Midterm,
//                    ExamDate = examStartDate.AddDays(examIndex % 20),
//                    ExamTime = new TimeOnly(10, 0, 0),
//                    Location = halls[examIndex % halls.Length],
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                });

//                exams.Add(new Exam
//                {
//                    Id = Guid.NewGuid(),
//                    CourseOfferingId = offering.Id,
//                    ExamType = ExamType.Final,
//                    ExamDate = examStartDate.AddDays(25 + (examIndex % 10)),
//                    ExamTime = new TimeOnly(13, 0, 0),
//                    Location = "Main Auditorium",
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                });
//                examIndex++;
//            }
//            context.Exams.AddRange(exams);
//            await context.SaveChangesAsync();

//            // ========== 14. ENROLLMENTS ==========
//            var enrollments = new List<Enrollment>();
//            var enrollmentDate = new DateTime(2024, 9, 1);
//            int enrollmentIndex = 0;

//            var scheduleLookup = schedules.ToDictionary(s => s.CourseOfferingId);

//            foreach (var student in students)
//            {
//                var deptOfferings = courseOfferings.Where(o => o.DepartmentId == student.DepartmentId).ToList();
//                foreach (var offering in deptOfferings.Take(3))
//                {
//                    var schedule = scheduleLookup.GetValueOrDefault(offering.Id);
//                    if (schedule == null) continue;

//                    var classGrade = (decimal)((enrollmentIndex * 13) % 30) / 1m;
//                    var midtermGrade = (decimal)((enrollmentIndex * 17) % 30) / 1m;
//                    var finalGrade = (decimal)((enrollmentIndex * 23) % 40) / 1m;
//                    var totalGrade = classGrade + midtermGrade + finalGrade;

//                    var status = totalGrade >= 60 ? EnrollmentStatus.Completed :
//                                 totalGrade >= 50 ? EnrollmentStatus.InProgress :
//                                 totalGrade >= 40 ? EnrollmentStatus.Incomplete : EnrollmentStatus.Failed;

//                    enrollments.Add(new Enrollment
//                    {
//                        Id = Guid.NewGuid(),
//                        StudentId = student.UserId,
//                        CourseOfferingId = offering.Id,
//                        ScheduleId = schedule.Id,
//                        EnrollmentDate = enrollmentDate.AddDays(enrollmentIndex % 5),
//                        ClassGrade = classGrade,
//                        MidtermGrade = midtermGrade,
//                        finalGrade = finalGrade,
//                        Status = status,
//                        CreatedAt = DateTime.UtcNow,
//                        IsDeleted = false
//                    });
//                    enrollmentIndex++;
//                }
//            }
//            context.Enrollments.AddRange(enrollments);
//            await context.SaveChangesAsync();

//            // ========== 15. FEES ==========
//            var fees = new List<Fee>
//            {
//                new Fee { Id = Guid.NewGuid(), Name = "Tuition Fee - Fall 2024", Description = "Tuition fee per credit hour for Fall 2024 semester", Amount = 550m, IsPerCredit = true, Type = FeeType.Tuition, SemesterId = fallSemester.Id, DepartmentId = csDept.Id, CreatedAt = DateTime.UtcNow, IsDeleted = false },
//                new Fee { Id = Guid.NewGuid(), Name = "Bus Service Fee", Description = "Transportation service for the semester", Amount = 1500m, IsPerCredit = false, Type = FeeType.Bus, SemesterId = fallSemester.Id, DepartmentId = csDept.Id, CreatedAt = DateTime.UtcNow, IsDeleted = false },
//                new Fee { Id = Guid.NewGuid(), Name = "Laboratory Fee", Description = "Laboratory equipment and materials fee", Amount = 350m, IsPerCredit = false, Type = FeeType.Lab, SemesterId = fallSemester.Id, DepartmentId = csDept.Id, CreatedAt = DateTime.UtcNow, IsDeleted = false },
//                new Fee { Id = Guid.NewGuid(), Name = "Student Activities Fee", Description = "Extracurricular and student club activities", Amount = 200m, IsPerCredit = false, Type = FeeType.Activity, SemesterId = fallSemester.Id, DepartmentId = csDept.Id, CreatedAt = DateTime.UtcNow, IsDeleted = false },
//                new Fee { Id = Guid.NewGuid(), Name = "Library Fee", Description = "Library access and digital resources", Amount = 100m, IsPerCredit = false, Type = FeeType.Other, SemesterId = fallSemester.Id, DepartmentId = csDept.Id, CreatedAt = DateTime.UtcNow, IsDeleted = false }
//            };
//            context.Fees.AddRange(fees);
//            await context.SaveChangesAsync();

//            // ========== 16. STUDENT FEES ==========
//            var studentFees = new List<StudentFee>();
//            var dueDate = new DateTime(2024, 10, 15);
//            int studentFeeIndex = 0;

//            foreach (var student in students)
//            {
//                int totalCredits = student.Level == 1 ? 15 : student.Level == 2 ? 16 : student.Level == 3 ? 17 : 18;

//                foreach (var fee in fees)
//                {
//                    decimal amount = fee.IsPerCredit ? fee.Amount * totalCredits : fee.Amount;
//                    decimal paidAmount;
//                    FeeStatus status;

//                    if (studentFeeIndex % 4 == 0)
//                    {
//                        paidAmount = amount;
//                        status = FeeStatus.Paid;
//                    }
//                    else if (studentFeeIndex % 4 == 1)
//                    {
//                        paidAmount = amount * 0.5m;
//                        status = FeeStatus.PartiallyPaid;
//                    }
//                    else if (studentFeeIndex % 4 == 2)
//                    {
//                        paidAmount = 0;
//                        status = FeeStatus.Overdue;
//                    }
//                    else
//                    {
//                        paidAmount = 0;
//                        status = FeeStatus.Pending;
//                    }

//                    studentFees.Add(new StudentFee
//                    {
//                        Id = Guid.NewGuid(),
//                        StudentId = student.UserId,
//                        FeeId = fee.Id,
//                        Amount = amount,
//                        PaidAmount = paidAmount,
//                        Status = status,
//                        DueDate = dueDate,
//                        CreatedAt = DateTime.UtcNow,
//                        IsDeleted = false
//                    });
//                    studentFeeIndex++;
//                }
//            }
//            context.StudentFees.AddRange(studentFees);
//            await context.SaveChangesAsync();

//            // ========== 17. PAYMENTS ==========
//            var payments = new List<Payment>();
//            var paymentDate = new DateTime(2024, 9, 20);
//            int paymentIndex = 0;

//            foreach (var studentFee in studentFees.Where(sf => sf.PaidAmount > 0))
//            {
//                payments.Add(new Payment
//                {
//                    Id = Guid.NewGuid(),
//                    StudentFeeId = studentFee.Id,
//                    Amount = studentFee.PaidAmount,
//                    PaymentDate = paymentDate.AddDays(paymentIndex % 20),
//                    Method = PaymentMethod.Online,
//                    ReferenceNumber = $"TXN{10000 + paymentIndex}",
//                    CreatedAt = DateTime.UtcNow,
//                    IsDeleted = false
//                });
//                paymentIndex++;
//            }
//            context.Payments.AddRange(payments);
//            await context.SaveChangesAsync();

//            await context.SaveChangesAsync();
//        }

//        private static async Task ClearDataAsync(HupDbContext context)
//        {
//            await context.Database.ExecuteSqlRawAsync("UPDATE Departments SET HeadOfDepartmentId = NULL");

//            // Clear in correct order to avoid foreign key constraints
//            context.Payments.RemoveRange(context.Payments);
//            context.StudentFees.RemoveRange(context.StudentFees);
//            context.Fees.RemoveRange(context.Fees);
//            context.Enrollments.RemoveRange(context.Enrollments);
//            context.Exams.RemoveRange(context.Exams);
//            context.Schedules.RemoveRange(context.Schedules);
//            context.CourseOfferings.RemoveRange(context.CourseOfferings);
//            context.ProgramPlan.RemoveRange(context.ProgramPlan);
//            context.Courses.RemoveRange(context.Courses);
//            context.Semesters.RemoveRange(context.Semesters);
//            context.Students.RemoveRange(context.Students);
//            context.Staff.RemoveRange(context.Staff);
//            context.Departments.RemoveRange(context.Departments);
//            context.Faculties.RemoveRange(context.Faculties);
//            context.Users.RemoveRange(context.Users);
//            context.RolePermissions.RemoveRange(context.RolePermissions);
//            context.Roles.RemoveRange(context.Roles);
//            context.Permissions.RemoveRange(context.Permissions);
//            await context.SaveChangesAsync();
//        }
//    }
//}