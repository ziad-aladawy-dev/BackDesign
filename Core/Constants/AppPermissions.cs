namespace HUP.Core.Constants
{
    public static class AppPermissions
    {
        // Student Profile
        public const string VIEW_PROFILE = "VIEW_PROFILE";
        public const string UPDATE_PROFILE = "UPDATE_PROFILE";
        public const string VIEW_ACADEMIC_STATUS = "VIEW_ACADEMIC_STATUS";
        public const string CREATE_STUDENT = "CREATE_STUDENT";
        public const string UPDATE_STUDENT_STATUS = "UPDATE_STUDENT_STATUS";

        // Enrollment
        public const string CREATE_ENROLLMENT = "CREATE_ENROLLMENT";
        public const string DELETE_ENROLLMENT = "DELETE_ENROLLMENT"; // Drop
        public const string VIEW_ENROLLMENT = "VIEW_ENROLLMENT"; // My Registered Courses
        public const string UPDATE_ENROLLMENT = "UPDATE_ENROLLMENT";
        public const string VIEW_GRADES = "VIEW_GRADES";
        public const string VIEW_TRANSCRIPT = "VIEW_TRANSCRIPT";

        // Course Offerings
        public const string VIEW_COURSE_OFFERING = "VIEW_COURSE_OFFERING";
        public const string CREATE_COURSE_OFFERING = "CREATE_COURSE_OFFERING";
        public const string UPDATE_COURSE_OFFERING = "UPDATE_COURSE_OFFERING";
        public const string DELETE_COURSE_OFFERING = "DELETE_COURSE_OFFERING";

        // Exams
        public const string VIEW_EXAM_SCHEDULE = "VIEW_EXAM_SCHEDULE";

        // Schedule / Timetable
        public const string VIEW_TIMETABLE = "VIEW_TIMETABLE";
        public const string CREATE_SCHEDULE = "CREATE_SCHEDULE"; // Admin
        public const string DELETE_SCHEDULE = "DELETE_SCHEDULE"; // Admin

        // Financial
        public const string VIEW_FEES = "VIEW_FEES";
        public const string VIEW_PAYMENT_HISTORY = "VIEW_PAYMENT_HISTORY"; // Future

        // Users (Admin)
        public const string CREATE_USER = "CREATE_USER";
        public const string VIEW_USERS = "VIEW_USERS";
        public const string UPDATE_USER = "UPDATE_USER";
        public const string DELETE_USER = "DELETE_USER";

        // Program Plan
        public const string VIEW_PROGRAM_PLAN = "VIEW_PROGRAM_PLAN";

        // Departments & Faculties
        public const string VIEW_DEPARTMENTS = "VIEW_DEPARTMENTS";
        public const string VIEW_FACULTIES = "VIEW_FACULTIES";

        public static List<string> GetAll()
        {
            return typeof(AppPermissions).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => (string)x.GetRawConstantValue())
                .ToList();
        }
    }
}
