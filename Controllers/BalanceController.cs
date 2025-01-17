using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using hazinDNS_v2.Data;
using Microsoft.EntityFrameworkCore;

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
    }

    public class AddBalanceModel
    {
        public decimal Amount { get; set; }
    }
} 