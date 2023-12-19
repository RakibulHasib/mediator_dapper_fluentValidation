using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public UserService(TestDbContext context, SqliteDbContext sqdb)
        {
            _context = context;
            _sqdb = sqdb;
        }

        public ValidateResponse_DTO? ValidateToken(string token)
        {
            if (token == null)
            {
                return null;
            }
            JwtSecurityTokenHandler? handler = new();
            JwtSecurityToken? jwtSecurityToken = handler.ReadJwtToken(token);
            string? UserID = jwtSecurityToken.Claims.First(claim => claim.Type == "UID").Value;

            UserLogInInfoTbl? userData = _context.UserLogInInfoTbl.Where(a => a.UserId == Convert.ToInt32(UserID)).FirstOrDefault();
            string? tokenSecretKey = "";
            if (userData.TokenSecretKey == null || userData.TokenSecretKey == "")
            {
                return null;
            }
            tokenSecretKey = userData.TokenSecretKey;
            ArgumentNullException.ThrowIfNull(tokenSecretKey, "Invalid User Data!!!");
            JwtSecurityTokenHandler? tokenhandler = new();
            byte[] key = Encoding.ASCII.GetBytes(tokenSecretKey);

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
                int roleId = int.Parse(jwtSecurity.Claims.First(x => x.Type == "RID").Value);
                string session = jwtSecurity.Claims.First(x => x.Type == "Ssn").Value;
                string permission = jwtSecurity.Claims.First(x => x.Type == "Pmn").Value;
                ValidateResponse_DTO response_DTO = new ValidateResponse_DTO()
                {
                    UserID = userID,
                    RoleID = roleId,
                    Session = session,
                    Permission= permission
                };

                return response_DTO;
            }
            catch
            {

                return null;
            }
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
                expires: DateTime.Now.AddHours(24),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<List<int>> GetPermission(int RoleID)
        {
            var permissions = await _context.PermissionAssignTbl.Where(a => a.RoleId == RoleID).Select(a => a.PermissionId).ToListAsync();
            return permissions;
        }

        public async void InsertUpdateCredential(Guid Session, int UserId, int RoleId, string Token,string Permission)
        {
            
            try
            {
                UserLogInfo userInfo = new UserLogInfo()
                {
                    SessionID = Session,
                    UserID = UserId,
                    RoleID = RoleId,
                    Token = Token,
                    LoginTime = DateTime.Now,
                    SessionTime = DateTime.Now.AddHours(24),
                    Permissions = Permission
                };
                await _sqdb.UserLogInfo.AddAsync(userInfo);
                await _sqdb.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw new InvalidOperationException("Data not inserteed!!!");
            }
           


        }
    }
}
