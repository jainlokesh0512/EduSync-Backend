namespace EduSyncWebAPI.DTOs
{
    public class AssessmentDto
    {
        public Guid AssessmentId { get; set; }
        public string? Title { get; set; }
        public string? Question { get; set; }
        public int? MaxScore { get; set; }
        public Guid? CourseId { get; set; }
    }

    public class AssessmentSummaryDTO
    {
        public Guid AssessmentId { get; set; }
        public string? Title { get; set; }
        public int? MaxScore { get; set; }
    }

    public class AssessmentDetailDto
    {
        public Guid AssessmentId { get; set; }
        public string? Title { get; set; }
        public int? MaxScore { get; set; }
        public string? Questions { get; set; }

        // Use the correct Course DTO here
        public CourseReadDTO? Course { get; set; }
    }
}
