using System.ComponentModel.DataAnnotations;

namespace Test_project.Entity
{
    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public int Age { get; set; }
        public DateTime DOB { get; set; }
    }
}
