using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hazinDNS_v2.Models;
using hazinDNS_v2.Data;

namespace hazinDNS_v2.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GetCartId()
        {
            string? cartId = HttpContext.Session.GetString("CartId");
            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartId", cartId);
                _logger.LogInformation($"Created new cart with ID: {cartId}");
            }
            return cartId;
        }

        public async Task<IActionResult> Index()
        {
            var cartId = GetCartId();
            _logger.LogInformation($"Fetching cart items for cart ID: {cartId}");
            
            var items = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            _logger.LogInformation($"Found {items.Count} items in cart");
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartModel model)
        {
            try
            {
                _logger.LogInformation($"Adding product {model.ProductId} to cart");
                
                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)
                {
                    _logger.LogWarning($"Product {model.ProductId} not found");
                    return NotFound("Товар не найден");
                }

                var cartId = GetCartId();
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == model.ProductId);

                if (cartItem == null)
                {
                    _logger.LogInformation($"Creating new cart item for product {model.ProductId}");
                    cartItem = new CartItem
                    {
                        CartId = cartId,
                        ProductId = model.ProductId,
                        Quantity = 1,
                        Product = product
                    };
                    _context.CartItems.Add(cartItem);
                }
                else
                {
                    _logger.LogInformation($"Increasing quantity for existing cart item {cartItem.Id}");
                    cartItem.Quantity++;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Cart updated successfully");
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, "Произошла ошибка при добавлении товара в корзину");
            }
        }

        // Удаление товара из корзины
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var cartId = GetCartId();
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // Очистка корзины
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var cartId = GetCartId();
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var cartId = GetCartId();
                var count = await _context.CartItems
                    .Where(ci => ci.CartId == cartId)
                    .SumAsync(ci => ci.Quantity);
                
                return Json(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count");
                return Json(new { count = 0 });
            }
        }
    }

    public class AddToCartModel
    {
        public int ProductId { get; set; }
    }
} 