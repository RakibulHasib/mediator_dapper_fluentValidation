using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Text;
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
            private readonly IConfiguration _configuration;

            public LoginQueryHandler(TestDbContext context, UserService userService, IConfiguration configuration)
            {
                _context = context;
                _userService = userService;
                _configuration = configuration;
            }

            public async Task<LoginResponse_DTO?> Handle(LoginQuery request, CancellationToken cancellationToken)
            {
                LoginResponse_DTO? loginData=new LoginResponse_DTO();

                request.Password = _userService.PasswordHassher(request.Password);
                var passwordCheck = await _context.UserLogInInfoTbl.Where(a => a.Password == request.Password && a.LoginId == request.LoginID).FirstOrDefaultAsync();
                var loginIdCheck = await _context.UserLogInInfoTbl.AnyAsync(a => a.LoginId == request.LoginID);
                if (loginIdCheck == false)
                {
                    loginData.response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    loginData.response.Message = "Invalid loginId";
                    loginData.response.Detail = "Login Id didn't match!!";
                    return loginData;
                }
                if (passwordCheck == null)
                {
                    loginData.response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    loginData.response.Message = "Invalid passwor";
                    loginData.response.Detail = "Password didn't match!!";
                    return loginData;
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


                loginData = (await _context.CreateConnection().QueryAsync<LoginResponse_DTO>(dataList, new { LoginID = request.LoginID, Password = request.Password })).FirstOrDefault();
                var Permissions = await _context.PermissionAssignTbl.Where(a => a.RoleId == loginData.RoleID).Select(a => a.PermissionId).ToListAsync();
                string permissionString = string.Join(",", Permissions);

                Guid Session= Guid.NewGuid();
                string secretKey = _configuration.GetSection("JWT")["Secret"];
               

                if (loginData != null)
                {
                    List<Claim> authClaims = new List<Claim>
                {
                    new("UID",loginData.UserID.ToString()),
                    new("Pmn",permissionString),
                    new("Ssn",Session.ToString())
                };
                    //token Create
                    if (secretKey != null)
                    {
                        string token = _userService.GetToken(authClaims, secretKey);
                        loginData.Token = token;
                        loginData.RefreshToken = Session.ToString();
                    }
                }               
                _userService.InsertUpdateCredential(Session, loginData.UserID, loginData.RoleID, loginData.Token, permissionString);

                loginData.response.Message = "Login Successfully";
                loginData.response.Detail = "Succesfully logged in";
                loginData.response.StatusCode = (int)HttpStatusCode.OK;
                return loginData;
            }
        }
    }
}
