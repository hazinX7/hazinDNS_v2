using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using hazinDNS_v2.Models;
using hazinDNS_v2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace hazinDNS_v2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly CartController _cartController;

        public AuthController(IConfiguration configuration, ApplicationDbContext context, ILogger<AuthController> logger, CartController cartController)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _cartController = cartController;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username);

                if (user == null || user.Password != model.Password)
                {
                    return Unauthorized(new { message = "Неверное имя пользователя или пароль" });
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var identity = new ClaimsIdentity(claims, "Cookies");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("Cookies", principal);

                return Ok(new { success = true, message = "Вы успешно вошли в систему" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при входе пользователя");
                return StatusCode(500, new { message = "Произошла ошибка при входе" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            _logger.LogInformation($"Попытка регистрации пользователя: {model.Username}");

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Ошибка! Пароль должен содержать минимум 6 символов" });
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => 
                    u.Username.ToLower() == model.Username.ToLower() || 
                    u.Email.ToLower() == model.Email.ToLower());

            if (existingUser != null)
            {
                _logger.LogWarning($"Пользователь уже существует: {model.Username}");
                return BadRequest(new { message = "Пользователь с таким именем или email уже существует" });
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Пользователь успешно зарегистрирован: {model.Username}");
            return Ok(new { message = "Регистрация успешна" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            HttpContext.Session.Clear();
            return Ok(new { success = true });
        }

        [HttpGet("checkAuth")]
        public IActionResult CheckAuth()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Ok();
            }
            return Unauthorized();
        }

        public class LoginModel
        {
            [Required(ErrorMessage = "Введите имя пользователя")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Введите пароль")]
            public string Password { get; set; } = string.Empty;
        }

        public class RegisterModel
        {
            [Required(ErrorMessage = "Введите имя пользователя")]
            [StringLength(50, ErrorMessage = "Имя пользователя должно быть от {2} до {1} символов", MinimumLength = 3)]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Введите email")]
            [EmailAddress(ErrorMessage = "Некорректный email адрес")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Введите пароль")]
            [StringLength(100, ErrorMessage = "Пароль должен содержать минимум {2} символов", MinimumLength = 6)]
            public string Password { get; set; } = string.Empty;
        }
    }
}    