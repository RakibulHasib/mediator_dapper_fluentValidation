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
        public PermissionList Permissions { get; }

        public AuthorizeAttribute(PermissionList permissions)
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
            UserService? userService = context.HttpContext.RequestServices.GetService<UserService>();
            var response = userService.ValidateToken(token);
            if (response == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            string? permissions = response.Permission;
            string? session=response.Session;
            int? userID = response.UserID;
            int role = response.RoleID;
            var requiredPermission = (int)Permissions;
            //var findPermissionIds = await userService.GetPermission(role);
            var findPermissionString = permissions.Split(',');
            var findPermissionIds = Array.ConvertAll(findPermissionString, int.Parse);

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
