﻿using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels;

public partial class Student
{
    public string UId { get; set; } = null!;

    public string FName { get; set; } = null!;

    public string LName { get; set; } = null!;

    public DateTime Dob { get; set; }

    public string Major { get; set; } = null!;


    
    public virtual Department MajorNavigation { get; set; } = null!;
    public virtual ICollection<Enrolled> Enrolleds { get; set; } = new List<Enrolled>();
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
