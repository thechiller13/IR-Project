using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SkinCare.Models;
using SkinCare.Services;
using System.Threading.Tasks;

namespace SkinCare.Controllers
{
    public class CartController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ShoppingCartService _shoppingCartService;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(UnitOfWork unitOfWork, ShoppingCartService shoppingCartService, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _shoppingCartService = shoppingCartService;
            _userManager = userManager;
        }        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var cartItems = await _shoppingCartService.GetCartItemsAsync();
            ViewBag.ShoppingCartService = _shoppingCartService;
            return View(cartItems);
        }

        // POST: Cart/AddToCart/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            // Check if product exists
            var product = await _unitOfWork.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _shoppingCartService.AddToCartAsync(id, quantity);
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/RemoveFromCart/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            await _shoppingCartService.RemoveFromCartAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateQuantity/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            if (quantity <= 0)
            {
                return BadRequest();
            }

            await _shoppingCartService.UpdateQuantityAsync(id, quantity);
            return RedirectToAction(nameof(Index));
        }        // GET: Cart/Checkout
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cartItems = await _shoppingCartService.GetCartItemsAsync();

            if (cartItems.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var order = new Order();
            ViewBag.ShoppingCartService = _shoppingCartService;
            return View(order);
        }

        // POST: Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Checkout([Bind("ShippingAddress,City,PostalCode,Country")] Order order)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Set user-related properties
            order.UserId = user.Id;
            order.OrderDate = DateTime.Now;
            order.Status = OrderStatus.Pending;
            order.User = user;

            // Get cart items
            var cartItems = await _shoppingCartService.GetCartItemsAsync();
            if (cartItems.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            // Calculate total and add order items
            decimal total = 0;
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                };
                total += orderItem.Quantity * orderItem.UnitPrice;
                order.OrderItems.Add(orderItem);
            }
            order.TotalAmount = total;

            // Clear and re-validate model state
            ModelState.Clear();

            TryValidateModel(order);
            foreach (var key in ModelState.Keys
    .Where(k => k.StartsWith("OrderItems") &&
               (k.Contains(".Product") || k.Contains(".Order")))
    .ToList())
            {
                ModelState.Remove(key);
            }
            if (ModelState.IsValid)
            {
                await _unitOfWork.AddOrderAsync(order);
                await _unitOfWork.CompleteAsync();
                await _shoppingCartService.EmptyCartAsync();
                return RedirectToAction("OrderConfirmation", new { id = order.Id });
            }

            ViewBag.ShoppingCartService = _shoppingCartService;
            return View(order);
        }

        // GET: Cart/OrderConfirmation/5
        [Authorize]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _unitOfWork.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }            // Check if the order belongs to the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null || order.UserId != user.Id)
            {
                return Forbid();
            }

            ViewBag.ShoppingCartService = _shoppingCartService;
            return View(order);
        }
    }
}
