using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Test_project.Context;
using Test_project.EnumList;
using Test_project.Services;

namespace Test_project.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public int Permissions { get; }

        public AuthorizeAttribute(int permissions)
        {
            Permissions = permissions;
        }

        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            string? token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            var _context = context.HttpContext.RequestServices.GetService<TestDbContext>();
            UserService? userService = context.HttpContext.RequestServices.GetService<UserService>();
            var response = userService.ValidateToken(token);
            int? userID = response.UserID;
            int? roles = response.RoleID;
            var requiredPermission = Permissions;
            var findPermissionIds = await _context.PermissionAssignTbl.Where(a => a.RoleId == roles).Select(a=>a.PermissionId).ToListAsync();
    

            if (!findPermissionIds.Contains(requiredPermission))
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
