using System;
using System.Collections.Generic;

namespace Edu_Sync_Backend.Models;

public partial class Assessment
{
    public Guid AssessmentId { get; set; }

    public string? Title { get; set; }

    public string? Question { get; set; }

    public int? MaxScore { get; set; }

    public Guid? CourseId { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<ResultTable> ResultTables { get; set; } = new List<ResultTable>();
}
