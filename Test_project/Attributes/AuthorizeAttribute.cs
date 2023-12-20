using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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
                context.Result = new UnauthorizedObjectResult(new {Message="Token is null!!"});
                return;
            }
            UserService? userService = context.HttpContext.RequestServices.GetService<UserService>();
            var response = userService.ValidateToken(token);
            switch (response.Response)
            {
                case -1:
                    context.Result = new UnauthorizedObjectResult(new { Message = "User Logged Out!!" });
                    return;
                case -2:
                    context.Result = new UnauthorizedObjectResult(new { Message = "Token Is Expired!!" });
                    return;
                case -3:
                    context.Result = new UnauthorizedObjectResult(new { Message = "User Not Found!!" });
                    return;
            }
            string? permissions = response.Permission;
            int? userID = response.UserID;
            var requiredPermission = (int)Permissions;
            var findPermissionString = permissions.Split(',');
            var findPermissionIds = Array.ConvertAll(findPermissionString, int.Parse);

            if (!findPermissionIds.Contains(requiredPermission))
            {
                context.Result = new UnauthorizedObjectResult(new {Message="You don't have permission!!!"});
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
                context.Result = new UnauthorizedObjectResult(new {Message="User not found"});
            }
        }
    }
}
