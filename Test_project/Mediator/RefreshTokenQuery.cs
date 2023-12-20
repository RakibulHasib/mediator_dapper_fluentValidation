using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Test_project.Context;
using Test_project.DTO;
using Test_project.Services;

namespace Test_project.Mediator
{
    public class RefreshTokenQuery:IRequest<LoginResponse_DTO>
    {
        public string? token { get; set; }
        internal sealed class RefreshTokenQueryHandler : IRequestHandler<RefreshTokenQuery, LoginResponse_DTO>
        {
            private readonly SqliteDbContext _sqdb;
            private readonly UserService _userService;

            public RefreshTokenQueryHandler(SqliteDbContext sqdb, UserService userService)
            {
                _sqdb = sqdb;
                _userService = userService;
            }

            public async Task<LoginResponse_DTO> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
            {
                if (request.token == null)
                {
                    return null;
                }
                JwtSecurityTokenHandler? handler = new();
                JwtSecurityToken? jwtSecurityToken = handler.ReadJwtToken(request.token);
                string? SessionID = jwtSecurityToken.Claims.First(claim => claim.Type == "Ssn").Value;
                string? Secret = jwtSecurityToken.Claims.First(claim => claim.Type == "Sec").Value;
                Guid sessionIdGuid = Guid.Parse(SessionID);

                var userData = _sqdb.UserLogInfo.Where(a => a.SessionID == sessionIdGuid).SingleOrDefault();
                int checkSession = _userService.CheckTokenExpiration(SessionID);
                if (checkSession == -2)
                {
                    List<Claim> authClaims = new List<Claim>
                {
                    new("UID",userData.UserID.ToString()),
                    new("RID",userData.RoleID.ToString()),
                    new("Pmn",userData.Permissions),
                    new("Ssn",userData.SessionID.ToString())
                };
                    string Token = _userService.GetToken(authClaims, Secret);
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
                        Secret = ""
                    };
                    _userService.InsertUpdateCredential(sessionIdGuid, response.UserID, response.RoleID, Token, userData.Permissions);
                    return response;
                }
                return null;
            }
        }
    }
}
