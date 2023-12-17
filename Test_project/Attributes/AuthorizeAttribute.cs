using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;
using Test_project.Context;
using Test_project.Services;

namespace Test_project.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        //private readonly TestDbContext _context;
        public string Roles { get; }

        public AuthorizeAttribute(string roles)
        {
            Roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string? token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            UserService? userService = context.HttpContext.RequestServices.GetService(typeof(UserService)) as UserService;
            var response = userService.ValidateToken(token);
            int? userID = response.UserID;
            string? roles = response.RoleName;
            var requiredRoles = Roles.Split(',');

            if (!requiredRoles.Contains(roles))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            if (userID != null)
            {

                Claim claim = new Claim(ClaimTypes.Name, userID.ToString() ?? "0");
                ClaimsIdentity? identity = new ClaimsIdentity(new[] { claim }, "BasicAuthentication");
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                context.HttpContext.User = principal;
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
