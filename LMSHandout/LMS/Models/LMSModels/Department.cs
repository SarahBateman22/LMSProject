using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels;

public partial class Department
{
    public string Name { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Professor> Professors { get; set; } = new List<Professor>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
