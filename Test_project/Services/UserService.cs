using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Test_project.Context;
using Test_project.DTO;
using Test_project.Entity;
using Test_project.SqliteEntity;

namespace Test_project.Services
{
    public class UserService
    {
        private readonly TestDbContext _context;
        private readonly SqliteDbContext _sqdb;
        private readonly IConfiguration _configuration;
        public UserService(TestDbContext context, SqliteDbContext sqdb, IConfiguration configuration)
        {
            _context = context;
            _sqdb = sqdb;
            _configuration = configuration;
        }


        public async Task<LoginResponse_DTO> TokenResponse(int? UserID)
        {
            var data = await (
            from uli in _context.UserLogInInfoTbl
            join ui in _context.UserInfoTbl on uli.UserInfoId equals ui.UserInfoId
            join r in _context.RoleMasterTbl on uli.RoleId equals r.RoleId
            where uli.UserId == UserID
            select new LoginResponse_DTO
            {
                LoginID = uli.LoginId,
                UserID = uli.UserId,
                UserInfoID = uli.UserInfoId,
                UserName = ui.FirstName + " " + ui.LastName,
                RoleID = uli.RoleId,
                RoleName = r.RoleName,
                Token = ""
            }).FirstOrDefaultAsync();
            return data;
        }

        public ValidateResponse_DTO? ValidateToken(string token)
        {
            ValidateResponse_DTO response_DTO = new ValidateResponse_DTO();
            JwtSecurityTokenHandler? handler = new();
            JwtSecurityToken? jwtSecurityToken = handler.ReadJwtToken(token);
            string? UserID = jwtSecurityToken.Claims.First(claim => claim.Type == "UID").Value;
            string? SessionID = jwtSecurityToken.Claims.First(claim => claim.Type == "Ssn").Value;
            int checkSession = CheckTokenExpiration(SessionID);
            switch (checkSession)
            {
                case -1:
                    response_DTO.Response = -1;
                    return response_DTO;
                case -2:
                    response_DTO.Response = -2;
                    return response_DTO;
            }
            UserLogInInfoTbl? userData = _context.UserLogInInfoTbl.Where(a => a.UserId == Convert.ToInt32(UserID)).FirstOrDefault();
            if (userData == null)
            {
                response_DTO.Response = -3;
                return response_DTO;
            }
            JwtSecurityTokenHandler? tokenhandler = new();
            string secretKey = _configuration.GetSection("JWT")["Secret"];
            byte[] key = Encoding.ASCII.GetBytes(secretKey);

            try
            {
                tokenhandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                JwtSecurityToken jwtSecurity = (JwtSecurityToken)validatedToken;
                int userID = int.Parse(jwtSecurity.Claims.First(x => x.Type == "UID").Value);
                string permission = jwtSecurity.Claims.First(x => x.Type == "Pmn").Value;
                response_DTO.UserID = userID;
                response_DTO.Permission = permission;
                response_DTO.Response = 1;
                return response_DTO;
            }
            catch
            {

                return null;
            }
        }
        //ExpirationCheck
        public int CheckTokenExpiration(string SessionID)
        {
            Guid sessionIdGuid = Guid.Parse(SessionID);
            UserLogInfo? userData = _sqdb.UserLogInfo.Where(a => a.SessionID == sessionIdGuid).SingleOrDefault();
            if (userData == null)
            {
                return -1;
            }
            var deleteExpired = _sqdb.UserLogInfo.Where(a => a.UserID == userData.UserID && a.SessionID != sessionIdGuid && a.SessionTime < DateTime.Now).ToList();
            if (deleteExpired.Any())
            {
                _sqdb.UserLogInfo.RemoveRange(deleteExpired);
                _sqdb.SaveChanges();
            }
            if (DateTime.Now > userData.SessionTime)
            {
                return -2;
            }
            return 1;
        }

        // Password Encoder
        public string PasswordHassher(string password)
        {
            Byte[] clearBytes = new UnicodeEncoding().GetBytes(password);
            Byte[] hashedBytes = (CryptoConfig.CreateFromName("MD5") as HashAlgorithm).ComputeHash(clearBytes);
            return BitConverter.ToString(hashedBytes);
        }

        //Generate Token
        public string GetToken(List<Claim> authClaims, string Secret)
        {
            SymmetricSecurityKey? authSigninKey = new(Encoding.UTF8.GetBytes(Secret));
            JwtSecurityToken token = new(
                expires: DateTime.Now.AddMinutes(5),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async void InsertUpdateCredential(Guid Session, int? UserId, int? RoleId, string Token, string Permission)
        {
            var data = await _sqdb.UserLogInfo.Where(a => a.SessionID == Session).SingleOrDefaultAsync();
            try
            {
                if (data == null)
                {
                    UserLogInfo userInfo = new UserLogInfo()
                    {
                        SessionID = Session,
                        UserID = UserId,
                        RoleID = RoleId,
                        Token = Token,
                        LoginTime = DateTime.Now,
                        SessionTime = DateTime.Now.AddMinutes(5),
                        Permissions = Permission
                    };
                    await _sqdb.UserLogInfo.AddAsync(userInfo);
                    await _sqdb.SaveChangesAsync();
                }
                else
                {
                    data.Token = Token;
                    data.LoginTime = DateTime.Now;
                    data.SessionTime = DateTime.Now.AddMinutes(5);
                    _sqdb.UserLogInfo.Update(data);
                    await _sqdb.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw new InvalidOperationException("Data not inserteed!!!");
            }



        }
    }
}
