using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels;

public partial class Class
{
    public uint ClassId { get; set; }

    public string Season { get; set; } = null!;

    public uint Year { get; set; }

    public string Location { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public uint Listing { get; set; }

    public string? TaughtBy { get; set; }

    public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; } = new List<AssignmentCategory>();

    public virtual ICollection<Enrolled> Enrolleds { get; set; } = new List<Enrolled>();

    public virtual Course ListingNavigation { get; set; } = null!;

    public virtual Professor? TaughtByNavigation { get; set; }
}
