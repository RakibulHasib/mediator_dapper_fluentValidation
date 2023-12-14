using Dapper;
using FluentValidation;
using MediatR;
using System.Data;
using Test_project.Context;
using Test_project.Entity;

namespace Test_project.Mediator
{
    public class InsertEmployeeCommand : IRequest<int>
    {
        //public Employee Employee { get; set; }
        public string? EmployeeName { get; set; }
        public int Age { get; set; }
        public DateTime DOB { get; set; }

        internal sealed class InsertEmployeeCommandHandler : IRequestHandler<InsertEmployeeCommand, int>
        {
            private readonly TestDbContext _context;
            public InsertEmployeeCommandHandler(TestDbContext context)
            {
                _context = context;
            }
            public async Task<int> Handle(InsertEmployeeCommand request, CancellationToken cancellationToken)
            {
                int response = 0;
                string query = "INSERT INTO Employee(EmployeeName,Age,DOB) values (@EmployeeName,@Age,@DOB)";
                var parameters = new DynamicParameters();
                parameters.Add("EmployeeName", request.EmployeeName, DbType.String);
                parameters.Add("Age", request.Age, DbType.Int32);
                parameters.Add("DOB", request.DOB, DbType.Date);

                using (var connection = _context.CreateConnection())
                {
                    await connection.ExecuteAsync(query, parameters);
                    return response = 1;
                }
            }
        }
    }

    public class EmployeeValidator : AbstractValidator<InsertEmployeeCommand>
    {
        public EmployeeValidator()
        {
            RuleFor(a => a.EmployeeName)
                .NotEmpty()
                .WithMessage("Employee Name is required!!");

            RuleFor(a => a.Age)
                .NotNull()
                .GreaterThan(1).WithMessage("Age should greter than one")
                .LessThan(100).WithMessage("Age should less than equal one hundred");

            RuleFor(a => a.DOB)
                .NotEmpty().WithMessage("Date of Birth is required!!")
                .GreaterThan(new DateTime(1950, 1, 1)).WithMessage("Date of birth must be greater than 01-01-1950.")
                .LessThan(DateTime.Now).WithMessage("Date of birth must be greater than 01-01-1950.");

        }
    }
}
