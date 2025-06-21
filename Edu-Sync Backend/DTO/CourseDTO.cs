namespace EduSyncWebAPI.DTOs
{
    public class CourseCreateDTO
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Guid? InstructorId { get; set; }
        public string? MediaUrl { get; set; }
    }

    public class CourseReadDTO
    {
        public Guid CourseId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Guid? InstructorId { get; set; }
        public string? MediaUrl { get; set; }
    }

    public class CourseDetailDTO
    {
        public Guid CourseId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Guid? InstructorId { get; set; }
        public string? MediaUrl { get; set; }
        public List<AssessmentSummaryDTO>? Assessments { get; set; }

        public UserDto? Instructor { get; set; }

    }
}
