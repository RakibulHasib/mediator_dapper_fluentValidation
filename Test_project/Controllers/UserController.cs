using MediatR;
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
using Test_project.Attributes;
using Test_project.Context;
using Test_project.DTO;
using Test_project.Entity;
using Test_project.Mediator;

namespace Test_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly TestDbContext _context;
        private readonly IMediator _mediator;

        public UserController(TestDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        [HttpPost("Register")]
        public async Task<int> Register(RegisterCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPost("Login")]
        public async Task<LoginResponse_DTO> Login(LoginQuery query)
        {
           return await _mediator.Send(query);
        }

        
    }
}
