using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Test_project.Context;
using Test_project.Entity;

namespace Test_project.Folder
{
    public class UserService
    {
        private readonly TestDbContext _context;
        public UserService(TestDbContext context)
        {
            _context = context;
        }

        public int? ValidateToken(string token)
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

                return userID;
            }
            catch
            {

                return null;
            }
        }
    }
}
