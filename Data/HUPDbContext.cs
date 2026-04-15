using HUP.Core.Entities.Academics;
using HUP.Core.Entities.Financial;
using HUP.Core.Entities.Identity;
using HUP.Core.Entities.Permissions;
using HUP.Core.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using SharpCompress.Common;
using System.Reflection;


namespace HUP.Data
{
    public class HupDbContext : DbContext
    {
        public HupDbContext(DbContextOptions<HupDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Staff> Staff { get; set; }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DepartmentFaculty> DepartmentFaculties { get; set; }

        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseOffering> CourseOfferings { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<ProgramPlan> ProgramPlan { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Fee> Fees { get; set; }
        public DbSet<StudentFee> StudentFees { get; set; }
        public DbSet<Payment> Payments { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // RolePermission (Many-to-Many)
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // User ↔ Role (Many-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserRole)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // User ↔ UserContact (One-to-One)
            // User ↔ UserPersonalInfo (One-to-One)
            modelBuilder.Entity<User>()
                .OwnsOne(u => u.PersonalInfo);

            modelBuilder.Entity<User>()
                .OwnsOne(u => u.ContactInfo);

            // User ↔ Student (One-to-One)
            modelBuilder.Entity<Student>()
                .HasKey(s => s.UserId);

            modelBuilder.Entity<Student>()
                .Property(s => s.UserId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Student>()
                .HasOne( s => s.User)
                .WithOne()
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Faculty)
                .WithMany(f => f.Students)
                .HasForeignKey(s => s.FacultyID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Department)
                .WithMany()
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // User ↔ Staff (One-to-One)
            modelBuilder.Entity<Staff>()
                .HasKey(s => s.UserId);

            modelBuilder.Entity<Staff>()
                .Property(i => i.UserId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Staff>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<Staff>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Staff>()
                .HasOne(i => i.Department)
                .WithMany(d => d.StaffMembers)
                .HasForeignKey(i => i.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Staff ↔ Faculty (Many-to-One)
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Faculty)
                .WithMany()
                .HasForeignKey(s => s.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Faculty>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.Property(f => f.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                //entity.Property(f => f.NameAr)
                //    .IsRequired()
                //    .HasMaxLength(200);

                //entity.Property(f => f.NameEn)
                //    .IsRequired()
                //    .HasMaxLength(200);

                entity.Property(f => f.ContactInfo)
                    .HasMaxLength(500);

                entity.HasOne(f => f.Dean)
                    .WithMany()
                    .HasForeignKey(f => f.DeanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // User ↔ Faculty (One-to-One - Dean)
            //modelBuilder.Entity<Faculty>()
            //    .HasOne(f => f.Dean)
            //    .WithOne()
            //    .HasForeignKey<Faculty>(f => f.DeanId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // Role ↔ User (CreatedBy)
            modelBuilder.Entity<Role>()
                .HasOne(r => r.CreatedByUser)
                .WithMany(u => u.CreatedRoles)
                .HasForeignKey(r => r.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Faculty ↔ Department (One-to-Many)
            //modelBuilder.Entity<Department>()
            //    .HasOne(d => d.Faculty)
            //    .WithMany()
            //    .HasForeignKey(d => d.FacultyId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // Department ↔ HeadOfDepartment
            modelBuilder.Entity<Department>()
                .HasOne(d => d.HeadOfDepartment)
                .WithMany()
                .HasForeignKey(d => d.HeadOfDepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Department ↔ Staff (One-to-Many)
            //modelBuilder.Entity<Staff>()
            //    .HasOne(s => s.Department)
            //    .WithMany()
            //    .HasForeignKey(s => s.DepartmentId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // Department ↔ Staff (One-to-One)
            //modelBuilder.Entity<Staff>()
            //    .HasOne(s => s.DepartmentHeaded)
            //    .WithOne()
            //    .HasForeignKey<Staff>(s => s.DepartmentHeadedId)
            //    .OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<Staff>()
            //    .HasOne(s => s.Department)
            //    .WithMany()
            //    .HasForeignKey(s => s.DepartmentId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Department>()
            //    .HasOne(d => d.HeadOfDepartment)
            //    .WithOne()
            //    .HasForeignKey<Department>(d => d.HeadOfDepartmentId)
            //    .OnDelete(DeleteBehavior.Restrict);
            
            
            modelBuilder.Entity<ProgramPlan>()
                .HasKey(pp => new { pp.DepartmentId, pp.CourseId });

            // Department ↔ ProgramPlan (One-to-Many)
            modelBuilder.Entity<ProgramPlan>()
                .HasOne(p => p.Department)
                .WithMany(d => d.Programs)
                .HasForeignKey(p => p.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course ↔ ProgramPlan (One-to-Many)
            modelBuilder.Entity<ProgramPlan>()
                .HasOne(p => p.Course)
                .WithMany(c => c.Programs)
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course ↔ Course (Self-referential Prerequisite)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Prerequisite)
                .WithMany()
                .HasForeignKey(c => c.PrerequisiteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course ↔ CourseOffering (One-to-Many)
            modelBuilder.Entity<CourseOffering>()
                .HasOne(co => co.Course)
                .WithMany(c => c.CourseOfferings)
                .HasForeignKey(co => co.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Semester ↔ CourseOffering (One-to-Many)
            modelBuilder.Entity<CourseOffering>()
                .HasOne(co => co.Semester)
                .WithMany()
                .HasForeignKey(co => co.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseOffering ↔ Department (Many-to-One)
            modelBuilder.Entity<CourseOffering>()
                .HasOne(co => co.Department)
                .WithMany(d => d.CourseOfferings)
                .HasForeignKey(co => co.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student ↔ Department (Many-to-One)
            //modelBuilder.Entity<Student>()
            //    .HasOne(s => s.Department)
            //    .WithMany()
            //    .HasForeignKey(s => s.DepartmentId)
            //    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasKey(e => e.Id);

            // Student ↔ Enrollment (One-to-Many)
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseOffering ↔ Enrollment (One-to-Many)
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.CourseOffering)
                .WithMany(co => co.Enrollments)
                .HasForeignKey(e => e.CourseOfferingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Schedule ↔ Enrollment (One-to-Many)
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Schedule)
                .WithMany()
                .HasForeignKey(e => e.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseOffering ↔ Exam (One-to-Many)
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.CourseOffering)
                .WithMany(co => co.Exams)
                .HasForeignKey(e => e.CourseOfferingId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseOffering ↔ Schedule (One-to-Many)
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.CourseOffering)
                .WithMany(co => co.Schedules)
                .HasForeignKey(s => s.CourseOfferingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Staff ↔ Schedule (One-to-Many)
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Staff)
                .WithMany()
                .HasForeignKey(s => s.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            // Financial Relationships
            modelBuilder.Entity<StudentFee>()
                .HasOne(sf => sf.Student)
                .WithMany()
                .HasForeignKey(sf => sf.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Fee>()
                .HasOne(f => f.Department)
                .WithMany()
                .HasForeignKey(f => f.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Fee>()
                .HasOne(f => f.Semester)
                .WithMany()
                .HasForeignKey(f => f.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentFee>()
                .HasOne(sf => sf.Fee)
                .WithMany()
                .HasForeignKey(sf => sf.FeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.StudentFee)
                .WithMany()
                .HasForeignKey(p => p.StudentFeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DepartmentFaculty>(entity =>
            {
                entity.HasKey(df => new { df.DepartmentId, df.FacultyId });

                entity.HasOne(df => df.Department)
                    .WithMany(d => d.DepartmentFaculties)
                    .HasForeignKey(df => df.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(df => df.Faculty)
                    .WithMany(f => f.DepartmentFaculties)
                    .HasForeignKey(df => df.FacultyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(df => df.DepartmentCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(df => df.IsPrimary)
                    .HasDefaultValue(false);
            });

            // Apply Global Query Filter for BaseEntity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = SetGlobalQueryMethod.MakeGenericMethod(entityType.ClrType);
                    method.Invoke(this, new object[] { modelBuilder });
                }
            }
        }

        static readonly MethodInfo SetGlobalQueryMethod = typeof(HupDbContext).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == nameof(SetGlobalQuery));

        private void SetGlobalQuery<T>(ModelBuilder builder) where T : BaseEntity
        {
            builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
