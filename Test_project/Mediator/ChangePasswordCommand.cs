using MediatR;
using Microsoft.EntityFrameworkCore;
using Test_project.Context;
using Test_project.Services;

namespace Test_project.Mediator
{
    public class ChangePasswordCommand:IRequest<(int result, string message)>
    {
        public int UserID { get; set; }
        public string? OldPass { get; set; }
        public string? NewPass { get; set; }
        public string? ReNewPass { get; set; }
        internal sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, (int result, string message)>
        {
            private readonly UserService _userService;
            private readonly TestDbContext _context;
            public ChangePasswordCommandHandler(UserService userService, TestDbContext context)
            {
                _userService = userService;
                _context = context;
            }
            public async Task<(int result, string message)> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
            {
                int result;
                string oldPass=_userService.PasswordHassher(request.OldPass??throw new FileNotFoundException("Enter your old password!!!"));
                if (request.NewPass != request.ReNewPass)
                {
                    throw new InvalidOperationException("New password not matched!!");
                }               
                string reNewPass = _userService.PasswordHassher(request.ReNewPass ?? throw new FileNotFoundException("Re Enter your new password"));              

                var user=await _context.UserLogInInfoTbl.Where(a=>a.UserId==request.UserID && a.Password==oldPass).SingleOrDefaultAsync();
                if (user==null)
                {
                    throw new InvalidOperationException("Previous password not matched!!");
                }
                try
                {
                    user.Password = reNewPass;
                    user.TokenSecretKey = "";
                    _context.UserLogInInfoTbl.Update(user);
                    result = await _context.SaveChangesAsync();
                    return (result,"Password changed successfully!!!");
                }
                catch (Exception)
                {

                    return (-1, "Password not changed");
                }
                
            }
        }
    }
}
