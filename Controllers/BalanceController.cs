using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using hazinDNS_v2.Data;
using Microsoft.EntityFrameworkCore;
using hazinDNS_v2.Models;

namespace hazinDNS_v2.Controllers
{
    [Authorize]
    public class BalanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BalanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddBalance([FromBody] AddBalanceModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(int.Parse(userId));

            if (user != null)
            {
                user.Balance += model.Amount;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Баланс успешно пополнен" });
            }

            return Json(new { success = false, message = "Ошибка при пополнении баланса" });
        }

        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Json(new { balance = 0 });
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            return Json(new { balance = user?.Balance ?? 0 });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBalance([FromBody] UpdateBalanceModel model)
        {
            try 
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Некорректные данные" });
                }

                // Проверка срока действия карты
                var expiryParts = model.ExpiryDate.Split('/');
                if (expiryParts.Length != 2)
                {
                    return BadRequest(new { success = false, message = "Неверный формат даты" });
                }

                if (!int.TryParse(expiryParts[0], out int expiryMonth) || !int.TryParse(expiryParts[1], out int expiryYear))
                {
                    return BadRequest(new { success = false, message = "Неверный формат даты" });
                }

                // Добавляем 2000 к году только если он меньше 100
                if (expiryYear < 100)
                {
                    expiryYear += 2000;
                }

                // Проверяем, что месяц в диапазоне 1-12
                if (expiryMonth < 1 || expiryMonth > 12)
                {
                    return BadRequest(new { success = false, message = "Неверный месяц" });
                }

                var currentDate = DateTime.Now;
                var expiryDate = new DateTime(expiryYear, expiryMonth, 1).AddMonths(1).AddDays(-1);

                if (expiryDate < currentDate)
                {
                    return BadRequest(new { success = false, message = "Срок действия карты истек" });
                }

                var username = User.Identity.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return NotFound(new { success = false, message = "Пользователь не найден" });
                }

                // Проверка суммы пополнения
                if (model.Amount <= 0 || model.Amount > 750000)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Сумма пополнения должна быть от 1 до 750 000 рублей" 
                    });
                }

                // Проверка максимального баланса
                decimal maxBalance = 5000000;
                if (user.Balance + model.Amount > maxBalance)
                {
                    decimal maxPossibleTopUp = maxBalance - user.Balance;
                    if (maxPossibleTopUp <= 0)
                    {
                        return BadRequest(new { 
                            success = false, 
                            message = "Достигнут максимальный баланс в 5 000 000 ₽" 
                        });
                    }

                    return BadRequest(new { 
                        success = false, 
                        message = $"Превышен максимальный баланс. Максимально возможная сумма пополнения: {maxPossibleTopUp:N0} ₽" 
                    });
                }

                user.Balance += model.Amount;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, balance = user.Balance });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Произошла ошибка при обновлении баланса" });
            }
        }
    }

    public class AddBalanceModel
    {
        public decimal Amount { get; set; }
    }
} 