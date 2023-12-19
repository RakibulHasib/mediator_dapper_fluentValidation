using MediatR;
using Microsoft.EntityFrameworkCore;
using Test_project.Context;

namespace Test_project.Mediator
{
    public class LogOutCommand:IRequest<int>
    {
        public int UserID { get; set; }
        internal sealed class LogOutCommandHandler : IRequestHandler<LogOutCommand, int>
        {
            private readonly TestDbContext _context;

            public LogOutCommandHandler(TestDbContext context)
            {
                _context = context;
            }

            public async Task<int> Handle(LogOutCommand request, CancellationToken cancellationToken)
            {
                int result = -1;
                var user = await _context.UserLogInInfoTbl.Where(a => a.UserId == request.UserID).SingleOrDefaultAsync();
                if (user != null)
                {
                    user.TokenSecretKey = "";
                    _context.UserLogInInfoTbl.Update(user);
                    result = await _context.SaveChangesAsync();
                }
                return result;
            }
        }
    }
}
