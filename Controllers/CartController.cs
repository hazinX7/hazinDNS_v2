using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hazinDNS_v2.Models;
using hazinDNS_v2.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace hazinDNS_v2.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class CartController : Controller
    {
        public class AddToCartModel
        {
            public int ProductId { get; set; }
        }

        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<CartController> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cartItems = new List<CartItem>();

            if (userId != null) // Для авторизованных пользователей
            {
                var cartId = $"user_{userId}";
                cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.CartId == cartId)
                    .ToListAsync();
            }
            else // Для неавторизованных пользователей
            {
                var sessionCart = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(sessionCart))
                {
                    var sessionCartItems = JsonSerializer.Deserialize<List<CartItem>>(sessionCart);
                    foreach (var item in sessionCartItems)
                    {
                        item.Product = await _context.Products.FindAsync(item.ProductId);
                    }
                    cartItems = sessionCartItems;
                }
            }

            return View(cartItems);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddToCart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartModel model)
        {
            try
            {
                _logger.LogInformation($"Received request body: {JsonSerializer.Serialize(model)}");
                _logger.LogInformation($"Adding product to cart. ProductId: {model.ProductId}");
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"UserId: {userId}");
                var product = await _context.Products.FindAsync(model.ProductId);
                
                if (product == null)
                {
                    _logger.LogWarning($"Product not found: {model.ProductId}");
                    return Json(new { success = false, message = "Продукт не найден" });
                }

                if (userId != null) // Для авторизованных пользователей
                {
                    _logger.LogInformation("Processing cart for authorized user");
                    var cartId = $"user_{userId}";
                    var cartItem = await _context.CartItems
                        .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == model.ProductId);

                    if (cartItem == null)
                    {
                        cartItem = new CartItem
                        {
                            CartId = cartId,
                            ProductId = model.ProductId,
                            Quantity = 1
                        };
                        _context.CartItems.Add(cartItem);
                        _logger.LogInformation("Created new cart item");
                    }
                    else
                    {
                        cartItem.Quantity++;
                        _logger.LogInformation($"Updated quantity for existing cart item: {cartItem.Quantity}");
                    }
                    await _context.SaveChangesAsync();
                }
                else // Для неавторизованных пользователей
                {
                    _logger.LogInformation("Processing cart for unauthorized user");
                    var sessionCart = HttpContext.Session.GetString("Cart");
                    var cartItems = string.IsNullOrEmpty(sessionCart) 
                        ? new List<CartItem>() 
                        : JsonSerializer.Deserialize<List<CartItem>>(sessionCart);

                    var cartItem = cartItems.FirstOrDefault(ci => ci.ProductId == model.ProductId);
                    if (cartItem == null)
                    {
                        cartItems.Add(new CartItem
                        {
                            ProductId = model.ProductId,
                            Quantity = 1
                        });
                        _logger.LogInformation("Added new item to session cart");
                    }
                    else
                    {
                        cartItem.Quantity++;
                        _logger.LogInformation($"Updated quantity in session cart: {cartItem.Quantity}");
                    }

                    HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cartItems));
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product to cart");
                return StatusCode(500, new { success = false, message = "Произошла ошибка при добавлении товара в корзину" });
            }
        }

        [AllowAnonymous]
        [HttpGet("GetCartCount")]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int count = 0;

            if (userId != null)
            {
                var cartId = $"user_{userId}";
                count = await _context.CartItems
                    .Where(ci => ci.CartId == cartId)
                    .SumAsync(ci => ci.Quantity);
            }
            else
            {
                var sessionCart = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(sessionCart))
                {
                    var cartItems = JsonSerializer.Deserialize<List<CartItem>>(sessionCart);
                    count = cartItems.Sum(ci => ci.Quantity);
                }
            }

            return Json(new { count });
        }

        [AllowAnonymous]
        public async Task MergeCartAsync(string userId)
        {
            try 
            {
                _logger.LogInformation($"Starting cart merge for user {userId}");
                var sessionCart = HttpContext.Session.GetString("Cart");
                if (string.IsNullOrEmpty(sessionCart))
                {
                    _logger.LogInformation("No session cart found");
                    return;
                }

                var cartId = $"user_{userId}";
                var sessionCartItems = JsonSerializer.Deserialize<List<CartItem>>(sessionCart);
                foreach (var sessionItem in sessionCartItems)
                {
                    var cartItem = await _context.CartItems
                        .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == sessionItem.ProductId);

                    if (cartItem != null)
                    {
                        cartItem.Quantity += sessionItem.Quantity;
                        _logger.LogInformation($"Updated quantity for product {sessionItem.ProductId}");
                    }
                    else
                    {
                        cartItem = new CartItem
                        {
                            CartId = cartId,
                            ProductId = sessionItem.ProductId,
                            Quantity = sessionItem.Quantity
                        };
                        _context.CartItems.Add(cartItem);
                        _logger.LogInformation($"Added new cart item for product {sessionItem.ProductId}");
                    }
                }

                await _context.SaveChangesAsync();
                HttpContext.Session.Remove("Cart");
                _logger.LogInformation("Cart merge completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cart merge");
                throw;
            }
        }
    }
} 