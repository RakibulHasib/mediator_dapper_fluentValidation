using MediatR;
using Microsoft.AspNetCore.Mvc;
using Test_project.DTO;
using Test_project.Mediator;

namespace Test_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
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

        [HttpPut("ChangePassWord")]
        public async Task<(int result, string message)> ChangePassWord(ChangePasswordCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPut("LogOut")]
        public async Task<int> LogOut(LogOutCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<LoginResponse_DTO>> RefreshToken(RefreshTokenQuery query)
        {
            return await _mediator.Send(query);
        }
    }
}
