using HUP.Core.Entities.Shared;
using HUP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using HUP.Core.Enums;

namespace HUP.Core.Entities.Academics
{
    public class Staff
    {
        public Guid UserId { get; set; }

        public Guid? DepartmentId { get; set; }
        public Guid? FacultyId { get; set; }
        //public Guid? DepartmentHeadedId { get; set; }
        public StaffTitle Title { get; set; }
        public StaffCategory Category { get; set; }

        public User User { get; set; }
        public Department Department { get; set; }
        public Faculty Faculty { get; set; }
        //public Department? DepartmentHeaded { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
    }
}
