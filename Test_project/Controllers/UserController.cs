using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Test_project.Context;
using Test_project.DTO;
using Test_project.Entity;

namespace Test_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly TestDbContext _context;

        public UserController(TestDbContext context)
        {
            _context = context;
        }
        [HttpPost("Register")]
        public async Task<int> Register(UserRegister_DTO userRegister)
        {
            int result = 0;
            var hasLoginID = await _context.UserLogInInfoTbl.Where(a => a.LoginId == userRegister.LoginId).FirstOrDefaultAsync();
            if (hasLoginID != null)
            {
                throw new InvalidOperationException("Login Id already exist!!");
            }
            UserInfoTbl userInfo = new UserInfoTbl()
            {
                FirstName = userRegister.FirstName,
                LastName = userRegister.LastName,
                Email = userRegister.Email,
                Phone = userRegister.Phone
            };
            await _context.UserInfoTbl.AddAsync(userInfo);
            result = await _context.SaveChangesAsync();

            UserLogInInfoTbl userLogInInfo = new UserLogInInfoTbl()
            {
                UserInfoId = userInfo.UserInfoId,
                LoginId = userRegister.LoginId,
                Password =PasswordHassher(userRegister.Password),
                RoleId = 0,
                TokenSecretKey = ""
            };
            await _context.UserLogInInfoTbl.AddAsync(userLogInInfo);
            await _context.SaveChangesAsync();

            return result;
        }

        // Password Encoder
        private string PasswordHassher(string password)
        {
            Byte[] clearBytes=new UnicodeEncoding().GetBytes(password);
            Byte[] hashedBytes = (CryptoConfig.CreateFromName("MD5") as HashAlgorithm).ComputeHash(clearBytes);
            return BitConverter.ToString(hashedBytes);
        }

        [HttpPost("Login")]
        public async Task<LoginResponse_DTO> Login(Login_DTO _login)
        {
            _login.Password = PasswordHassher(_login.Password);
            var passwordCheck = await _context.UserLogInInfoTbl.Where(a => a.Password == _login.Password).FirstOrDefaultAsync();
            var loginIdCheck = await _context.UserLogInInfoTbl.AnyAsync(a => a.LoginId == _login.LoginID);
            if (passwordCheck == null)
            {
                throw new UnauthorizedAccessException("Invalid Password!!");
            }
            if (loginIdCheck == false)
            {
                throw new UnauthorizedAccessException("Invalid LoginId!!");
            }
            var loginData = await (from ui in _context.UserInfoTbl
                                   join uli in _context.UserLogInInfoTbl on ui.UserInfoId equals uli.UserInfoId
                                   join r in _context.RoleMasterTbl on uli.RoleId equals r.RoleId
                                   where uli.LoginId == _login.LoginID
                                   && uli.Password == _login.Password
                                   select new LoginResponse_DTO
                                   {
                                       LoginID=uli.LoginId,
                                       UserInfoID=ui.UserInfoId,
                                       UserID=uli.UserId,
                                       UserName=ui.FirstName+" "+ui.LastName,
                                       RoleID=r.RoleId,
                                       RoleName=r.RoleName,
                                       Token="",
                                       Secret=""
                                   }).FirstOrDefaultAsync();

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
                    new("UID",loginData.UserID.ToString())
                };
                //token Create
                if (loginData.Secret != null)
                {
                    string token = GetToken(authClaims, loginData.Secret);
                    loginData.Secret = null;
                    loginData.Token = token;

                }
            }
            return loginData;
        }

        //Generate Token
        private string GetToken(List<Claim> authClaims,string Secret)
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
