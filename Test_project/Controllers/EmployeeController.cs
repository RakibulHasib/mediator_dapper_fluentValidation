﻿using Dapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Test_project.Context;
using Test_project.Entity;
using Test_project.Mediator;

namespace Test_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EmployeeController(IMediator mediator)
        {
            this._mediator = mediator;
        }

        [HttpGet("GetEmployee")]
        public async Task<ActionResult<List<Employee>>> GetEmployee()
        {
            return await _mediator.Send(new GetEmployeeQuery());
        }

        [HttpPost("InsertEmplyoee")]
        public async Task<ActionResult<int>> InsertEmplyoee(InsertEmployeeCommand command)
        {
             return await _mediator.Send(command);
        }

        //[HttpPost("InsertEmplyoee")]
        //public async Task<ActionResult<int>> InsertEmplyoee(Employee command)
        //{
        //    return await _mediator.Send(new InsertEmployeeCommand { employee = command });
        //}
    }
}
