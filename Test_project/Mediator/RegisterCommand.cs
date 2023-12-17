using Azure;
using Dapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Test_project.Context;
using Test_project.Entity;
using Test_project.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
                string? password = _userService.PasswordHassher(request.Password);
                string? SP_Register = "InsertRegister_prc";
                var parameters = new DynamicParameters();
                parameters.Add("FirstName", request.FirstName, DbType.String);
                parameters.Add("LastName", request.LastName, DbType.String);
                parameters.Add("Email", request.Email, DbType.String);
                parameters.Add("Phone", request.Phone, DbType.Int32);
                parameters.Add("LoginId", request.LoginId, DbType.String);
                parameters.Add("Password", password, DbType.String);
                parameters.Add("Result", 0, DbType.Int32);

                using (var connection = _context.CreateConnection())
                {
                    var result=await connection.ExecuteAsync(SP_Register, parameters,commandType:CommandType.StoredProcedure);
                    return result;
                }
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
        }
    }
}
