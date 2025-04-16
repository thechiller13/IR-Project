using Microsoft.AspNetCore.Mvc;
using SkinCare.Services;
using System.Threading.Tasks;

namespace SkinCare.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly UnitOfWork _unitOfWork;

        public CategoryMenuViewComponent(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _unitOfWork.GetAllCategoriesAsync();
            return View(categories);
        }
    }
}
