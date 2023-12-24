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
                using (var connection = _context.CreateConnection())
                {
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        
                        try
                        {
                            int role = await connection.QueryFirstOrDefaultAsync<int>("SELECT COUNT(*) FROM RoleAssign_tbl", null, transaction);

                            string query = "INSERT INTO Employee(EmployeeName,Age,DOB) values (@EmployeeName,@Age,@DOB); SELECT SCOPE_IDENTITY();";
                            var parameters = new DynamicParameters();
                            parameters.Add("EmployeeName", request.EmployeeName, DbType.String);
                            parameters.Add("Age", request.Age, DbType.Int32);
                            parameters.Add("DOB", request.DOB, DbType.Date);
                            int employeeId = await connection.ExecuteScalarAsync<int>(query, parameters,transaction);


                            string roleQuery = "INSERT INTO RoleAssign_tbl(RoleID,UserID) values (@RoleID,@UserID)";
                            var roleParameters = new DynamicParameters();
                            roleParameters.Add("RoleID", role + 1, DbType.Int32);
                            roleParameters.Add("UserID", employeeId, DbType.Int32);
                            response = await connection.ExecuteAsync(roleQuery, roleParameters,transaction);
                            await transaction.CommitAsync();
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                        }
                    }
                };

                return response;
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
