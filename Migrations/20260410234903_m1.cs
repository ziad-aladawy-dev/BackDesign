using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HUP.Migrations
{
    /// <inheritdoc />
    public partial class m1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseOfferings_Courses_CourseId1",
                table: "CourseOfferings");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseOfferings_Departments_DepartmentId1",
                table: "CourseOfferings");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Faculties_FacultyId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Faculties_FacultyId1",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Staff_HeadOfDepartmentId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_CourseOfferings_CourseOfferingId1",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Students_StudentUserId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_CourseOfferings_CourseOfferingId1",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramPlan_Courses_CourseId1",
                table: "ProgramPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramPlan_Departments_DepartmentId1",
                table: "ProgramPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId1",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleId1",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_CourseOfferings_CourseOfferingId1",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Departments_DepartmentHeadedId",
                table: "Staff");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Departments_DepartmentId1",
                table: "Staff");

            migrationBuilder.DropTable(
                name: "RoleUser");

            migrationBuilder.DropIndex(
                name: "IX_Staff_DepartmentHeadedId",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Staff_DepartmentId1",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_CourseOfferingId1",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_PermissionId1",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_RoleId1",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_ProgramPlan_CourseId1",
                table: "ProgramPlan");

            migrationBuilder.DropIndex(
                name: "IX_ProgramPlan_DepartmentId1",
                table: "ProgramPlan");

            migrationBuilder.DropIndex(
                name: "IX_Faculties_DeanId",
                table: "Faculties");

            migrationBuilder.DropIndex(
                name: "IX_Exams_CourseOfferingId1",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_CourseOfferingId1",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentUserId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_FacultyId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_FacultyId1",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_HeadOfDepartmentId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_CourseOfferings_CourseId1",
                table: "CourseOfferings");

            migrationBuilder.DropIndex(
                name: "IX_CourseOfferings_DepartmentId1",
                table: "CourseOfferings");

            migrationBuilder.DropColumn(
                name: "DepartmentHeadedId",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "DepartmentId1",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "PermissionId1",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "RoleId1",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "ProgramPlan");

            migrationBuilder.DropColumn(
                name: "DepartmentId1",
                table: "ProgramPlan");

            migrationBuilder.DropColumn(
                name: "DeanName",
                table: "Faculties");

            migrationBuilder.DropColumn(
                name: "DurationInYears",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "FacultyId",
                table: "Departments");

            migrationBuilder.RenameColumn(
                name: "CourseOfferingId1",
                table: "Schedules",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CourseOfferingId1",
                table: "Exams",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "StudentUserId",
                table: "Enrollments",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CourseOfferingId1",
                table: "Enrollments",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "FacultyId1",
                table: "Departments",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "DepartmentCode",
                table: "Departments",
                newName: "BaseDepartmentCode");

            migrationBuilder.RenameColumn(
                name: "DepartmentId1",
                table: "CourseOfferings",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CourseId1",
                table: "CourseOfferings",
                newName: "DeletedBy");

            migrationBuilder.AlterColumn<int>(
                name: "PersonalInfo_Religion",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PersonalInfo_Nationality",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PersonalInfo_Gender",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PersonalInfo_BirthPlace",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PersonalInfo_BirthDate",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FacultyID",
                table: "Students",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "StudentFees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StudentFees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "StudentFees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "StudentFees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "StudentFees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Semesters",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Semesters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Semesters",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Semesters",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Schedules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Schedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Schedules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Schedules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Roles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Permissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Permissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Permissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Permissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Fees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Fees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Fees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Fees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Fees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeanId",
                table: "Faculties",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "ContactInfo",
                table: "Faculties",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Faculties",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Faculties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Faculties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Faculties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Faculties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Faculties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Exams",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Exams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Exams",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Exams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Enrollments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Enrollments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Departments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CourseType",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Courses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CourseOfferings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CourseOfferings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CourseOfferings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DepartmentFaculties",
                columns: table => new
                {
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentFaculties", x => new { x.DepartmentId, x.FacultyId });
                    table.ForeignKey(
                        name: "FK_DepartmentFaculties_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentFaculties_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_FacultyID",
                table: "Students",
                column: "FacultyID");

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_DeanId",
                table: "Faculties",
                column: "DeanId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadOfDepartmentId",
                table: "Departments",
                column: "HeadOfDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentFaculties_FacultyId",
                table: "DepartmentFaculties",
                column: "FacultyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Staff_HeadOfDepartmentId",
                table: "Departments",
                column: "HeadOfDepartmentId",
                principalTable: "Staff",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Faculties_FacultyID",
                table: "Students",
                column: "FacultyID",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Staff_HeadOfDepartmentId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Faculties_FacultyID",
                table: "Students");

            migrationBuilder.DropTable(
                name: "DepartmentFaculties");

            migrationBuilder.DropIndex(
                name: "IX_Students_FacultyID",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Faculties_DeanId",
                table: "Faculties");

            migrationBuilder.DropIndex(
                name: "IX_Departments_HeadOfDepartmentId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FacultyID",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Fees");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Fees");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Fees");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Fees");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Fees");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Faculties");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Faculties");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Faculties");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Faculties");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Faculties");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Faculties");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CourseType",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CourseOfferings");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CourseOfferings");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CourseOfferings");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Schedules",
                newName: "CourseOfferingId1");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Exams",
                newName: "CourseOfferingId1");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Enrollments",
                newName: "StudentUserId");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Enrollments",
                newName: "CourseOfferingId1");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Departments",
                newName: "FacultyId1");

            migrationBuilder.RenameColumn(
                name: "BaseDepartmentCode",
                table: "Departments",
                newName: "DepartmentCode");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "CourseOfferings",
                newName: "DepartmentId1");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "CourseOfferings",
                newName: "CourseId1");

            migrationBuilder.AlterColumn<int>(
                name: "PersonalInfo_Religion",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PersonalInfo_Nationality",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PersonalInfo_Gender",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PersonalInfo_BirthPlace",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PersonalInfo_BirthDate",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentHeadedId",
                table: "Staff",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId1",
                table: "Staff",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PermissionId1",
                table: "RolePermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId1",
                table: "RolePermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId1",
                table: "ProgramPlan",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId1",
                table: "ProgramPlan",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeanId",
                table: "Faculties",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactInfo",
                table: "Faculties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeanName",
                table: "Faculties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DurationInYears",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "FacultyId",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "RoleUser",
                columns: table => new
                {
                    CreatedRolesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.CreatedRolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_RoleUser_Roles_CreatedRolesId",
                        column: x => x.CreatedRolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Staff_DepartmentHeadedId",
                table: "Staff",
                column: "DepartmentHeadedId",
                unique: true,
                filter: "[DepartmentHeadedId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_DepartmentId1",
                table: "Staff",
                column: "DepartmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_CourseOfferingId1",
                table: "Schedules",
                column: "CourseOfferingId1");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId1",
                table: "RolePermissions",
                column: "PermissionId1");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId1",
                table: "RolePermissions",
                column: "RoleId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramPlan_CourseId1",
                table: "ProgramPlan",
                column: "CourseId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramPlan_DepartmentId1",
                table: "ProgramPlan",
                column: "DepartmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_DeanId",
                table: "Faculties",
                column: "DeanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exams_CourseOfferingId1",
                table: "Exams",
                column: "CourseOfferingId1");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseOfferingId1",
                table: "Enrollments",
                column: "CourseOfferingId1");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentUserId",
                table: "Enrollments",
                column: "StudentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_FacultyId",
                table: "Departments",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_FacultyId1",
                table: "Departments",
                column: "FacultyId1");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadOfDepartmentId",
                table: "Departments",
                column: "HeadOfDepartmentId",
                unique: true,
                filter: "[HeadOfDepartmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferings_CourseId1",
                table: "CourseOfferings",
                column: "CourseId1");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferings_DepartmentId1",
                table: "CourseOfferings",
                column: "DepartmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UsersId",
                table: "RoleUser",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseOfferings_Courses_CourseId1",
                table: "CourseOfferings",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseOfferings_Departments_DepartmentId1",
                table: "CourseOfferings",
                column: "DepartmentId1",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Faculties_FacultyId",
                table: "Departments",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Faculties_FacultyId1",
                table: "Departments",
                column: "FacultyId1",
                principalTable: "Faculties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Staff_HeadOfDepartmentId",
                table: "Departments",
                column: "HeadOfDepartmentId",
                principalTable: "Staff",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_CourseOfferings_CourseOfferingId1",
                table: "Enrollments",
                column: "CourseOfferingId1",
                principalTable: "CourseOfferings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Students_StudentUserId",
                table: "Enrollments",
                column: "StudentUserId",
                principalTable: "Students",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_CourseOfferings_CourseOfferingId1",
                table: "Exams",
                column: "CourseOfferingId1",
                principalTable: "CourseOfferings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramPlan_Courses_CourseId1",
                table: "ProgramPlan",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramPlan_Departments_DepartmentId1",
                table: "ProgramPlan",
                column: "DepartmentId1",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId1",
                table: "RolePermissions",
                column: "PermissionId1",
                principalTable: "Permissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleId1",
                table: "RolePermissions",
                column: "RoleId1",
                principalTable: "Roles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_CourseOfferings_CourseOfferingId1",
                table: "Schedules",
                column: "CourseOfferingId1",
                principalTable: "CourseOfferings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Departments_DepartmentHeadedId",
                table: "Staff",
                column: "DepartmentHeadedId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Departments_DepartmentId1",
                table: "Staff",
                column: "DepartmentId1",
                principalTable: "Departments",
                principalColumn: "Id");
        }
    }
}
