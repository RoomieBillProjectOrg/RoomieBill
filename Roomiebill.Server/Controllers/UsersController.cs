using Microsoft.AspNetCore.Mvc;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Common.Validators;

namespace Roomiebill.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto user)
        {
            try
            {
                await _userService.RegisterUserAsync(user);

                return Ok(new { Message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("verifyEmailRegister")]
        public async Task<IActionResult> VerifyEmailRegister([FromQuery] string email)
        {
            try
            {
                var verifyCode = await RegisterVerify.SendVerificationEmail(email);

                return Ok(verifyCode);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("updatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto updatePasswordDto)
        {
            try
            {
                var user = await _userService.UpdatePasswordAsync(updatePasswordDto);

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message});
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _userService.LoginAsync(loginDto);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string username)
        {
            try
            {
                await _userService.LogoutAsync(username);
                return Ok(new { Message = "User logged out successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("getUserInvites")]
        public async Task<IActionResult> GetUserGroups([FromQuery] string username)
        {
            try
            {
                List<Invite> userInvites = await _userService.GetUserInvitesAsync(username);
                return Ok(userInvites);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
