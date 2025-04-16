using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkinCare.Models;
using SkinCare.Services;
using System.Threading.Tasks;

namespace SkinCare.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AdminProductsController(UnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        // GET: AdminProducts
        public async Task<IActionResult> Index()
        {
            var products = await _unitOfWork.GetAllProductsAsync();
            return View(products);
        }

        // GET: AdminProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _unitOfWork.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: AdminProducts/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CategoryId"] = new SelectList(await _unitOfWork.GetAllCategoriesAsync(), "Id", "Name");
            return View();
        }        // POST: AdminProducts/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            // Remove Category navigation property validation errors
            ModelState.Remove("Category");

            // Initialize ImageUrl if null (from the hidden field)
            if (product.ImageUrl == null)
            {
                product.ImageUrl = "";
            }

            // If image file was uploaded, it takes precedence over the hidden field
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();
                        string base64String = Convert.ToBase64String(imageBytes);

                        string imageType = imageFile.ContentType;
                        product.ImageUrl = $"data:{imageType};base64,{base64String}";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing image: {ex.Message}");
                    product.ImageUrl = "";
                }
            }
            // If no image file but we have ImageUrl from the hidden field (base64), use that
            else if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                // Already set from the form
            }
            else
            {
                product.ImageUrl = "";
            }

            if (ModelState.IsValid)
            {
                await _unitOfWork.AddProductAsync(product);
                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(await _unitOfWork.GetAllCategoriesAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: AdminProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _unitOfWork.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(await _unitOfWork.GetAllCategoriesAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }        // POST: AdminProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,CategoryId,Stock,IsActive,ImageUrl")] Product product,
            IFormFile imageFile, bool clearImage = false)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            // Remove Category navigation property validation errors
            ModelState.Remove("Category");
            if(imageFile == null && product.ImageUrl != null)
            {
                ModelState.Remove("imageFile");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing product from database
                    var existingProduct = await _unitOfWork.GetProductByIdAsync(id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Handle image updates
                    if (clearImage)
                    {
                        // User requested to remove the image
                        product.ImageUrl = string.Empty;
                    }
                    else if (imageFile != null && imageFile.Length > 0)
                    {
                        // New image uploaded - process it
                        using (var memoryStream = new MemoryStream())
                        {
                            await imageFile.CopyToAsync(memoryStream);
                            byte[] imageBytes = memoryStream.ToArray();
                            string base64String = Convert.ToBase64String(imageBytes);
                            string imageType = imageFile.ContentType;
                            product.ImageUrl = $"data:{imageType};base64,{base64String}";
                        }
                    }
                    else
                    {
                        // No new image and not clearing - keep existing image
                        product.ImageUrl = existingProduct.ImageUrl;
                    }

                    // Update other properties
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.Stock = product.Stock;
                    existingProduct.IsActive = product.IsActive;
                    existingProduct.ImageUrl = product.ImageUrl;

                    _unitOfWork.UpdateProduct(existingProduct);
                    await _unitOfWork.CompleteAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["CategoryId"] = new SelectList(await _unitOfWork.GetAllCategoriesAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }
        // GET: AdminProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _unitOfWork.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: AdminProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _unitOfWork.GetProductByIdAsync(id);
            if (product != null)
            {                // Delete product image if one exists
                if (!string.IsNullOrEmpty(product.ImageUrl) &&
                    System.IO.File.Exists(Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'))))
                {
                    System.IO.File.Delete(Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/')));
                }

                _unitOfWork.DeleteProduct(product);
                await _unitOfWork.CompleteAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProductExists(int id)
        {
            return await _unitOfWork.GetProductByIdAsync(id) != null;
        }
    }
}
