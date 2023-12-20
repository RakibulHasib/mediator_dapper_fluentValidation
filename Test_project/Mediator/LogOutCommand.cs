using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Test_project.Context;

namespace Test_project.Mediator
{
    public class LogOutCommand:IRequest<int>
    {
        public string? token { get; set; }
        internal sealed class LogOutCommandHandler : IRequestHandler<LogOutCommand, int>
        {
            private readonly SqliteDbContext _sqdb;

            public LogOutCommandHandler(SqliteDbContext sqdb)
            {
                _sqdb = sqdb;
            }

            public async Task<int> Handle(LogOutCommand request, CancellationToken cancellationToken)
            {
                int result = -1;
                if (request.token == null)
                {
                    return -1;
                }
                JwtSecurityTokenHandler? handler = new();
                JwtSecurityToken? jwtSecurityToken = handler.ReadJwtToken(request.token);
                string? SessionID = jwtSecurityToken.Claims.First(claim => claim.Type == "Ssn").Value;
                Guid sessionIdGuid = Guid.Parse(SessionID);

                var userData =await _sqdb.UserLogInfo.Where(a => a.SessionID == sessionIdGuid).SingleOrDefaultAsync();
                if (userData != null)
                {
                    _sqdb.UserLogInfo.Remove(userData);
                    result = await _sqdb.SaveChangesAsync();
                }
                return result;
            }
        }
    }
}
