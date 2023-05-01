using System;
using System.Collections.Generic;

namespace test;

public partial class DepartmentsName
{
    public int DepartmentId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Department? Department { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
