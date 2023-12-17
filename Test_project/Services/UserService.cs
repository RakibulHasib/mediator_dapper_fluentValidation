using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Test_project.Context;
using Test_project.DTO;
using Test_project.Entity;

namespace Test_project.Services
{
    public class UserService
    {
        private readonly TestDbContext _context;
        public UserService(TestDbContext context)
        {
            _context = context;
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
            string? RoleID = jwtSecurityToken.Claims.First(claim => claim.Type == "RID").Value;

            UserLogInInfoTbl? userData = _context.UserLogInInfoTbl.Where(a => a.UserId == Convert.ToInt32(UserID)).FirstOrDefault();
            string? tokenSecretKey = "";
            if (userData != null)
            {
                tokenSecretKey = userData.TokenSecretKey;
            }
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
                string roleName = jwtSecurity.Claims.First(x => x.Type == "RID").Value;
                ValidateResponse_DTO response_DTO = new ValidateResponse_DTO()
                {
                    UserID = userID,
                    RoleName = roleName
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
    }
}
