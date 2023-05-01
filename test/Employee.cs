using System;
using System.Collections.Generic;

namespace test;

public partial class Employee
{
    public int Id { get; set; }

    public int? Department { get; set; }

    public string Fullname { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? Jobtitle { get; set; }

    public virtual DepartmentsName? DepartmentNavigation { get; set; }

    public virtual Jobtitle? JobtitleNavigation { get; set; }
}
