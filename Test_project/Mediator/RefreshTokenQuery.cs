using MediatR;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Test_project.Context;
using Test_project.DTO;
using Test_project.Services;

namespace Test_project.Mediator
{
    public class RefreshTokenQuery:IRequest<LoginResponse_DTO>
    {
        public string? RefreshToken { get; set; }
        internal sealed class RefreshTokenQueryHandler : IRequestHandler<RefreshTokenQuery, LoginResponse_DTO>
        {
            private readonly SqliteDbContext _sqdb;
            private readonly UserService _userService;
            private readonly IConfiguration _configuration;

            public RefreshTokenQueryHandler(SqliteDbContext sqdb, UserService userService, IConfiguration configuration)
            {
                _sqdb = sqdb;
                _userService = userService;
                _configuration = configuration;
            }

            public async Task<LoginResponse_DTO> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
            {
                if (request.RefreshToken == null)
                {
                    return null;
                }
                
                Guid sessionIdGuid = Guid.Parse(request.RefreshToken);

                var userData = _sqdb.UserLogInfo.Where(a => a.SessionID == sessionIdGuid).SingleOrDefault();
                int checkSession = _userService.CheckTokenExpiration(request.RefreshToken);
                if (checkSession == -2)
                {
                    string secretKey = _configuration.GetSection("JWT")["Secret"];
                    List<Claim> authClaims = new List<Claim>
                {
                    new("UID",userData.UserID.ToString()),
                    new("RID",userData.RoleID.ToString()),
                    new("Pmn",userData.Permissions),
                    new("Ssn",userData.SessionID.ToString())
                };
                    string Token = _userService.GetToken(authClaims, secretKey);
                    var dataList = await _userService.TokenResponse(userData.UserID);
                    LoginResponse_DTO response = new LoginResponse_DTO()
                    {
                        LoginID = dataList.LoginID,
                        UserID = dataList.UserID,
                        UserInfoID = dataList.UserInfoID,
                        UserName = dataList.UserName,
                        RoleID = dataList.RoleID,
                        RoleName = dataList.RoleName,
                        Token = Token,
                        RefreshToken= sessionIdGuid.ToString()
                    };
                    _userService.InsertUpdateCredential(sessionIdGuid, response.UserID, response.RoleID, Token, userData.Permissions);
                    return response;
                }
                return null;
            }
        }
    }
}
