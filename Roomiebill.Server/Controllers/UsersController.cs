using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.DataAccessLayer;
using System;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;
using Roomiebill.Server.DataAccessLayer.Dtos;

namespace Roomiebill.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private UserService _userService;

        public UsersController(ApplicationDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto user)
        {
            //_context.Users.Add(user);
            //await _context.SaveChangesAsync();

            await _userService.RegisterUserAsync(user);

            return Ok(new { Message = "User registered successfully" });
        }
    }
}
