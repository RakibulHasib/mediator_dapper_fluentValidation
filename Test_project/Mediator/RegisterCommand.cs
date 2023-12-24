using Dapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Test_project.Context;
using Test_project.Entity;
using Test_project.EnumList;
using Test_project.Services;

namespace Test_project.Mediator
{
    public class RegisterCommand : IRequest<int>
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int Phone { get; set; }
        public string LoginId { get; set; } = null!;

        public string Password { get; set; } = null!;
        public List<int> PermissionID { get; set; } = null!;
        internal sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, int>
        {
            private readonly TestDbContext _context;
            private readonly UserService _userService;

            public RegisterCommandHandler(TestDbContext context, UserService userService)
            {
                _context = context;
                _userService = userService;
            }
            public async Task<int> Handle(RegisterCommand request, CancellationToken cancellationToken)
            {
                int result = 0;
                var hasLoginID = await _context.UserLogInInfoTbl.Where(a => a.LoginId == request.LoginId).FirstOrDefaultAsync();
                if (hasLoginID != null)
                {
                    throw new InvalidOperationException("Login Id already exist!!");
                }
                await _context.Database.BeginTransactionAsync();
                try
                {
                    UserInfoTbl userInfo = new UserInfoTbl()
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Email = request.Email,
                        Phone = request.Phone
                    };
                    await _context.UserInfoTbl.AddAsync(userInfo);
                    await _context.SaveChangesAsync();

                    RoleMasterTbl roleMaster = new RoleMasterTbl()
                    {
                        RoleName = "Role " + await _context.RoleMasterTbl.CountAsync(),
                    };
                    await _context.RoleMasterTbl.AddAsync(roleMaster);
                    await _context.SaveChangesAsync();

                    UserLogInInfoTbl userLogInInfo = new UserLogInInfoTbl()
                    {
                        UserInfoId = userInfo.UserInfoId,
                        LoginId = request.LoginId,
                        Password = _userService.PasswordHassher(request.Password),
                        RoleId = roleMaster.RoleId,
                        TokenSecretKey = ""
                    };
                    await _context.UserLogInInfoTbl.AddAsync(userLogInInfo);


                    if (request.PermissionID != null)
                    {
                        foreach (var item in request.PermissionID)
                        {
                            var permissionCheck = await _context.PermissionTbl.AnyAsync(a => a.PermissionId == item);
                            if (permissionCheck)
                            {
                                PermissionAssignTbl assignTbl = new PermissionAssignTbl()
                                {
                                    PermissionId = item,
                                    RoleId = roleMaster.RoleId
                                };
                                await _context.PermissionAssignTbl.AddAsync(assignTbl);
                            }
                        }
                    }
                    result = await _context.SaveChangesAsync();
                    await _context.Database.CommitTransactionAsync();
                }
                catch (Exception)
                {
                    await _context.Database.RollbackTransactionAsync();
                }
                
                return result;
            }
        }
    }

    public class RegisterValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterValidator()
        {
            RuleFor(a => a.FirstName)
                .NotEmpty().WithMessage("FirstName is Required!!")
                .NotNull().WithMessage("FirstName is Required!!");

            RuleFor(a => a.LastName)
                .NotEmpty().WithMessage("LastName is Required!!")
                .NotNull().WithMessage("LastName is Required!!");

            RuleFor(a => a.Email)
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .EmailAddress().WithMessage("Enter valid email address");

            RuleFor(a => a.Phone)
                .NotEmpty().WithMessage("Phone number is required!!");

            RuleFor(a => a.Password)
                .NotEmpty().WithMessage("Pasword required!!!")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Must(password =>
                        password.Any(char.IsLower) &&
                        password.Any(char.IsUpper) &&
                        password.Any(char.IsDigit))
                                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one digit.");

            RuleFor(a => a.LoginId)
                .NotEmpty().WithMessage("Login ID required!!!")
                .MinimumLength(4).WithMessage("LoginId must be at least 4 characters long.")
                .MaximumLength(6).WithMessage("LoginId will not greter than 6 characters long");

            RuleFor(a => a.PermissionID)
                .NotEmpty().WithMessage("Permission is required!!")
                .NotNull().WithMessage("Permission is required!!");
        }
    }
}
