using Dapper;
using MediatR;
using Test_project.Context;
using Test_project.Entity;

namespace Test_project.Mediator
{
    public class GetEmployeeQuery:IRequest<List<Employee>>
    {
        internal sealed class GetEmployeeQueryhandler : IRequestHandler<GetEmployeeQuery, List<Employee>>
        {
            private readonly TestDbContext _context;
            public GetEmployeeQueryhandler(TestDbContext context)
            {
                _context = context;
            }
            public async Task<List<Employee>> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
            {
                string query = "SELECT * FROM Employee";
                using (var connection = _context.CreateConnection())
                {
                    var emplist = await connection.QueryAsync<Employee>(query);
                    return emplist.ToList();
                }
            }
        }
    }
}
