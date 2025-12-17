using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GuildManagement.Entities;
using GuildManagement.Data;
using Microsoft.AspNetCore.Authorization;
using GuildManagement.DTOs.Auth;

namespace GuildManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly GuildManagementContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            GuildManagementContext context,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Некорректные данные" });
                }
                var user = await _userManager.FindByEmailAsync(loginRequest.Email);
                if (user == null)
                {
                    return BadRequest(new { message = "Пользователь не найден" });
                }
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName, 
                    loginRequest.Password, 
                    isPersistent: false, 
                    lockoutOnFailure: false);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Неверный email или пароль" });
                }
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                var userRoles = await _userManager.GetRolesAsync(user);
                
                var response = new
                {
                    message = "Успешный вход в систему",
                    userId = user.Id,
                    email = user.Email,
                    username = user.UserName,
                    roles = userRoles,
                    memberId = user.MemberId,
                    lastLogin = user.LastLogin
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при входе в систему");
                return BadRequest(new { message = "Ошибка при входе в систему" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok(new { message = "Успешный выход из системы" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выходе из системы");
                return BadRequest(new { message = "Ошибка при выходе из системы" });
            }
        }
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            
            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                username = user.UserName,
                roles = userRoles,
                memberId = user.MemberId,
                createdAt = user.CreatedAt,
                lastLogin = user.LastLogin
            });
        }
    }
}