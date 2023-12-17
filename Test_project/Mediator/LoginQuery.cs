using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Test_project.Context;
using Test_project.DTO;
using Test_project.Services;

namespace Test_project.Mediator
{
    public class LoginQuery : IRequest<LoginResponse_DTO>
    {
        public string LoginID { get; set; }
        public string Password { get; set; }
        internal sealed class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResponse_DTO>
        {
            private readonly TestDbContext _context;
            private readonly UserService _userService;

            public LoginQueryHandler(TestDbContext context, UserService userService)
            {
                _context = context;
                _userService = userService;
            }

            public async Task<LoginResponse_DTO> Handle(LoginQuery request, CancellationToken cancellationToken)
            {
                request.Password = _userService.PasswordHassher(request.Password);
                var passwordCheck = await _context.UserLogInInfoTbl.Where(a => a.Password == request.Password && a.LoginId == request.LoginID).FirstOrDefaultAsync();
                var loginIdCheck = await _context.UserLogInInfoTbl.AnyAsync(a => a.LoginId == request.LoginID);
                if (passwordCheck == null)
                {
                    throw new UnauthorizedAccessException("Invalid Password!!");
                }
                if (loginIdCheck == false)
                {
                    throw new UnauthorizedAccessException("Invalid LoginId!!");
                }

                var dataList = @"SELECT TOP 1
                uli.LoginId AS LoginID,
                ui.UserInfoId AS UserInfoID,
                uli.UserId AS UserID,
                ui.FirstName + ' ' + ui.LastName AS UserName,
                r.RoleId AS RoleID,
                r.RoleName AS RoleName,
                '' AS Token,
                '' AS Secret
                FROM
                User_Info_tbl ui
                INNER JOIN 
                User_LogInInfo_tbl uli ON ui.UserInfoId = uli.UserInfoId 
                INNER JOIN 
                RoleMaster_tbl r ON uli.RoleId = r.RoleId 
                WHERE uli.LoginId = @LoginID AND uli.Password = @Password";


                var loginData = (await _context.CreateConnection().QueryAsync<LoginResponse_DTO>(dataList, new { LoginID = request.LoginID, Password = request.Password })).FirstOrDefault();

                Guid tokenSecretKey = Guid.NewGuid();
                loginData.Secret = tokenSecretKey.ToString();
                passwordCheck.TokenSecretKey = loginData.Secret;
                _context.UserLogInInfoTbl.Update(passwordCheck);
                await _context.SaveChangesAsync();

                if (loginData != null)
                {
                    List<Claim> authClaims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new("UID",loginData.UserID.ToString()),
                    new("RID",loginData.RoleName)
                };
                    //token Create
                    if (loginData.Secret != null)
                    {
                        string token = _userService.GetToken(authClaims, loginData.Secret);
                        loginData.Secret = null;
                        loginData.Token = token;

                    }
                }
                return loginData;
            }
        }
    }
}
