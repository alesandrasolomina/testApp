using System;
using System.Collections.Generic;

namespace test;

public partial class Department
{
    public int Id { get; set; }

    public int? Parentid { get; set; }

    public int? Managerid { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? ManagerName { get; set; }
    

    public virtual DepartmentsName IdNavigation { get; set; } = null!;

    public virtual ICollection<Department> InverseParent { get; set; } = new List<Department>();

    public virtual Department? Parent { get; set; }
}
