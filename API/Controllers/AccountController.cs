using System.Security.Claims;
using API.Common;
using API.DTOs;
using API.Services;
using Domain.Core;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly UserManager<User> _userManager;

        public AccountController(UserManager<User> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);
            if (user == null)
            {
                return Unauthorized(ApiResponse<string>.Error("User with this email address doesn't exist"));
            }

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordCorrect)
            {
                return Unauthorized(ApiResponse<string>.Error("Password is not correct"));
            }
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.Contains(Roles.Owner) ? Roles.Owner :
                       roles.Contains(Roles.Admin) ? Roles.Admin : Roles.User;

            return Ok(ApiResponse<UserDto>.Success(await CreateUserDto(user, role)));
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Email))
            {
                return BadRequest(ApiResponse<string>.Error("User with already exist"));
            }
            if (await _userManager.Users.AnyAsync(x => x.Email.ToLower() == registerDto.Email.ToLower()))
            {
                return BadRequest(ApiResponse<string>.Error("User with this email address already exists"));
            }


            var user = new User
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                Name = registerDto.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(ApiResponse<string>.Error("Failed to create a new user"));
            }

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(ApiResponse<UserDto>.Success(await CreateUserDto(user, Roles.User)));
        }


        [Authorize]
        [HttpGet("current")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

            if (user == null)
            {
                return Unauthorized(ApiResponse<UserDto>.Error("No user found!"));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.Contains(Roles.Owner) ? Roles.Owner :
                       roles.Contains(Roles.Admin) ? Roles.Admin : Roles.User;

            return Ok(ApiResponse<UserDto>.Success(await CreateUserDto(user, role)));
        }


        private async Task<UserDto> CreateUserDto(User user, string role)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                UserName = user.UserName,
                Email = user.Email,
                Bio = user.Bio,
                Image = user.Image,
                IsConfirmed = user.IsConfirmed,
                Role = role,
                TelegramId = user.TelegramId,
                Token = await _tokenService.CreateToken(user),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
            };
        }

    }
}