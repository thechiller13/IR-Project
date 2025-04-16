using Microsoft.AspNetCore.Mvc;
using SkinCare.Services;
using System.Threading.Tasks;

namespace SkinCare.ViewComponents
{
    public class ShoppingCartSummaryViewComponent : ViewComponent
    {
        private readonly ShoppingCartService _shoppingCartService;

        public ShoppingCartSummaryViewComponent(ShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await _shoppingCartService.GetCartCountAsync();
            return View(items);
        }
    }
}
