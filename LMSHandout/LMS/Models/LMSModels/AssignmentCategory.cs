using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels;

public partial class AssignmentCategory
{
    public uint CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public uint Weight { get; set; }

    public uint InClass { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual Class InClassNavigation { get; set; } = null!;
}
