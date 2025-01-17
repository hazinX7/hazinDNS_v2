using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using hazinDNS_v2.Data;
using hazinDNS_v2.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace hazinDNS_v2.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Получаем username вместо userId
            var username = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound();
            }

            // Проверяем текущий пароль
            if (user.Password != model.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Неверный текущий пароль");
                return View(model);
            }

            // Обновляем пароль
            user.Password = model.NewPassword;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Пароль успешно изменен";
            return RedirectToAction("Index", "Profile");
        }
    }
} 