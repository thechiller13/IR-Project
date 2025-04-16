using Microsoft.AspNetCore.Http;
using SkinCare.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinCare.Services
{
    public class ShoppingCartService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "CartId";

        public ShoppingCartService(UnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCartId()
        {
            // If user is authenticated, use their user ID as the cart ID
            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return _httpContextAccessor.HttpContext.User.Identity.Name;
            }
            
            // Otherwise, use a session-based cart ID
            if (_httpContextAccessor.HttpContext.Session.GetString(CartSessionKey) == null)
            {
                // Create new cart ID if none exists
                var cartId = Guid.NewGuid().ToString();
                _httpContextAccessor.HttpContext.Session.SetString(CartSessionKey, cartId);
            }
            
            return _httpContextAccessor.HttpContext.Session.GetString(CartSessionKey);
        }

        public async Task AddToCartAsync(int productId, int quantity = 1)
        {
            var cartId = GetCartId();
            
            // Check if product already exists in cart
            var cartItem = await _unitOfWork.GetCartItemAsync(cartId, productId);
            
            if (cartItem == null)
            {
                // Create new cart item
                cartItem = new CartItem
                {
                    ProductId = productId,
                    CartId = cartId,
                    Quantity = quantity,
                    DateCreated = DateTime.Now
                };
                
                await _unitOfWork.AddCartItemAsync(cartItem);
            }
            else
            {
                // Update quantity of existing cart item
                cartItem.Quantity += quantity;
                _unitOfWork.UpdateCartItem(cartItem);
            }
            
            await _unitOfWork.CompleteAsync();
        }

        public async Task<int> RemoveFromCartAsync(int id)
        {
            var cartItem = await _unitOfWork.GetCartItemAsync(id);
            
            if (cartItem == null)
                return 0;
            
            _unitOfWork.RemoveCartItem(cartItem);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<int> UpdateQuantityAsync(int id, int quantity)
        {
            var cartItem = await _unitOfWork.GetCartItemAsync(id);
            
            if (cartItem == null)
                return 0;
                
            cartItem.Quantity = quantity;
            _unitOfWork.UpdateCartItem(cartItem);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<List<CartItem>> GetCartItemsAsync()
        {
            return await _unitOfWork.GetCartItemsAsync(GetCartId());
        }

        public async Task<int> GetCartCountAsync()
        {
            var cartItems = await GetCartItemsAsync();
            return cartItems.Sum(item => item.Quantity);
        }

        public async Task<decimal> GetCartTotalAsync()
        {
            var cartItems = await GetCartItemsAsync();
            return cartItems.Sum(item => item.Product.Price * item.Quantity);
        }

        public async Task EmptyCartAsync()
        {
            await _unitOfWork.ClearCartAsync(GetCartId());
            await _unitOfWork.CompleteAsync();
        }

        public async Task MigrateCartAsync(string userName)
        {
            var cartId = GetCartId();
            
            // Get all items from the current cart
            var cartItems = await _unitOfWork.GetCartItemsAsync(cartId);
            
            // Update the cart ID to the user's name (which is used as cart ID for authenticated users)
            foreach (var item in cartItems)
            {
                item.CartId = userName;
                _unitOfWork.UpdateCartItem(item);
            }
            
            await _unitOfWork.CompleteAsync();
            
            // Clear the session-based cart ID
            _httpContextAccessor.HttpContext.Session.Remove(CartSessionKey);
        }
    }
}
