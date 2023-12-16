using System;
using System.Collections.Generic;

namespace Test_project.Entity;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = null!;

    public int Age { get; set; }

    public DateTime Dob { get; set; }
}
