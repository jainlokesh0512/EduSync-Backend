namespace EduSyncWebAPI.DTOs
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    public class UserSummaryDTO
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }


    public class UserDetailDto
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public List<CourseReadDTO>? Courses { get; set; }
    }

}
