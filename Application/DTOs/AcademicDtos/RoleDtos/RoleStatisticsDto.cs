namespace HUP.Application.DTOs.AcademicDtos.RoleDtos
{
    public class RoleStatisticsDto
    {
        public int TotalRoles { get; set; }
        public int ActiveRoles { get; set; }
        public int InactiveRoles { get; set; }
        public int TotalPermissions { get; set; }
        public int TotalUsersAssigned { get; set; }
        public Dictionary<string, int> RolesByUsersCount { get; set; } = new();
        public List<RolePermissionSummaryDto> PermissionsDistribution { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }
}
