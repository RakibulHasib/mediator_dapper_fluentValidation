using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Test_project.Context;
using Test_project.Controllers;
using Test_project.Entity;
using Test_project.Services;

namespace Test_project.Middleware
{
    public class AuthHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, UserService _user)
        {
            //string? token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            //var? UserID = _user.ValidateToken(token);

            //if (UserID != null)
            //{
            //context.Items["User"] = UserID;
            //    Claim claim = new Claim(ClaimTypes.Name, UserID.ToString() ?? "0");
            //    ClaimsIdentity? identity=new ClaimsIdentity(new[] { claim }, "BasicAuthentication");
            //    ClaimsPrincipal principal=new ClaimsPrincipal(identity);
            //    context.User= principal;
            //}
            //await _next(context);
        }
    }
    public static class AuthHandlerMiddlewareExtension
    {
        public static IApplicationBuilder UseAuthHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthHandlerMiddleware>();
        }
    }
}
