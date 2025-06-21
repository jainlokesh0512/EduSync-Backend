using System;
using System.Collections.Generic;

namespace Edu_Sync_Backend.Models;

public partial class UserModel
{
    public Guid UserId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }

    public string? PasswordHash { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<ResultTable> ResultTables { get; set; } = new List<ResultTable>();
}
