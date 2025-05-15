using Microsoft.AspNetCore.Mvc;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;
using Roomiebill.Server.Services.Interfaces;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Common.Validators;

namespace Roomiebill.Server.Controllers
{
    /// <summary>
    /// Manages user operations including registration, authentication, and profile management.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="user">User registration details.</param>
        /// <returns>Success message if registration is completed.</returns>
        /// <response code="200">When registration is successful.</response>
        /// <response code="400">If registration fails due to invalid data or existing user.</response>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto user)
        {
            try
            {
                await _userService.RegisterUserAsync(user);
                return Ok(new MessageResponse { Message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Verifies user registration details and sends a verification email.
        /// </summary>
        /// <param name="user">User registration details to verify.</param>
        /// <returns>Verification code for email confirmation.</returns>
        /// <response code="200">Returns the verification code.</response>
        /// <response code="400">If verification fails or email sending fails.</response>
        [HttpPost("verifyUserRegisterDetails")]
        public async Task<IActionResult> VerifyUserRegisterDetails([FromBody] RegisterUserDto user)
        {
            try
            {
                await _userService.VerifyRegisterUserDetailsAsync(user);
                VerifiyCodeModel verifyCode = await RegisterVerify.SendVerificationEmail(user.Email);
                return Ok(verifyCode);
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Updates a user's password.
        /// </summary>
        /// <param name="updatePasswordDto">Password update details.</param>
        /// <returns>Updated user information.</returns>
        /// <response code="200">Returns the updated user.</response>
        /// <response code="400">If password update fails.</response>
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
                return BadRequest(new MessageResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Authenticates a user and creates a session.
        /// </summary>
        /// <param name="loginDto">Login credentials.</param>
        /// <returns>User information and session details.</returns>
        /// <response code="200">Returns the authenticated user.</response>
        /// <response code="400">If login fails.</response>
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
                return BadRequest(new MessageResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Ends a user's session.
        /// </summary>
        /// <param name="username">Username of the user to log out.</param>
        /// <returns>Success message.</returns>
        /// <response code="200">When logout is successful.</response>
        /// <response code="400">If logout fails.</response>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string username)
        {
            try
            {
                await _userService.LogoutAsync(username);
                return Ok(new MessageResponse { Message = "User logged out successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new MessageResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all pending invites for a user.
        /// </summary>
        /// <param name="username">Username of the user.</param>
        /// <returns>List of pending invites.</returns>
        /// <response code="200">Returns the list of invites.</response>
        /// <response code="400">If retrieval fails.</response>
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
                return BadRequest(new MessageResponse { Message = ex.Message });
            }
        }
    }
}
