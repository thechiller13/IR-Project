using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using SkinCare.Models;
using SkinCare.Services;
using System.Threading.Tasks;

namespace SkinCare.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        
        public ProductsController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Products
        public async Task<IActionResult> Index(int? categoryId)
        {
            ViewData["Categories"] = new SelectList(await _unitOfWork.GetAllCategoriesAsync(), "Id", "Name", categoryId);
            
            var products = categoryId.HasValue
                ? await _unitOfWork.GetProductsByCategoryAsync(categoryId.Value)
                : await _unitOfWork.GetAllProductsAsync();
            
            return View(products.Where(p => p.IsActive).ToList());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _unitOfWork.GetProductByIdAsync(id);
            
            if (product == null || !product.IsActive)
            {
                return NotFound();
            }

            return View(product);
        }
        
        // GET: Products/Search
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction(nameof(Index));
            }

            var products = await _unitOfWork.GetAllProductsAsync();
            
            var searchResults = products
                .Where(p => p.IsActive && 
                       (p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        p.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        p.Category.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .ToList();
                
            ViewData["SearchQuery"] = query;
            ViewData["Categories"] = new SelectList(await _unitOfWork.GetAllCategoriesAsync(), "Id", "Name");
            
            return View("Index", searchResults);
        }
        
        // GET: Products/Featured
        public async Task<IActionResult> Featured()
        {
            // Here you can implement logic to get featured products
            // For now, just getting the first 4 products as an example
            var products = (await _unitOfWork.GetAllProductsAsync())
                .Where(p => p.IsActive)
                .Take(4)
                .ToList();
                
            return View(products);
        }
        
        // GET: Products/BestSellers
        public async Task<IActionResult> BestSellers()
        {
            // Here you can implement logic to get bestselling products
            // For this example, just getting 4 random products
            var random = new Random();
            var products = (await _unitOfWork.GetAllProductsAsync())
                .Where(p => p.IsActive)
                .OrderBy(p => random.Next())
                .Take(4)
                .ToList();
                
            return View(products);
        }
        
        // GET: Products/GetSearchSuggestions
        [AllowAnonymous] // Allow anonymous access for search suggestions
        public async Task<IActionResult> GetSearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new string[0]);
            }

            var products = await _unitOfWork.GetAllProductsAsync();
            
            // Get only products that contain the query in their name
            // Limit to active products and take maximum 10 suggestions for performance
            var suggestions = products
                .Where(p => p.IsActive && 
                       p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Name)
                .Distinct()
                .Take(10)
                .ToList();
                
            return Json(suggestions);
        }
    }
}
