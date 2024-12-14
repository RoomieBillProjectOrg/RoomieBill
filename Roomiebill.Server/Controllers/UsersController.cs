using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.BillingService;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.GroupService;
using Roomiebill.Server.UserService;
using System;

namespace Roomiebill.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User user)
        {
            if (user == null)
                return BadRequest("Invalid user data.");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully" });
        }
    }
}
