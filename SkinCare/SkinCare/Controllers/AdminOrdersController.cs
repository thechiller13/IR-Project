using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SkinCare.Models;
using SkinCare.Services;
using System.Threading.Tasks;

namespace SkinCare.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public AdminOrdersController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: AdminOrders
        public async Task<IActionResult> Index()
        {
            var orders = await _unitOfWork.GetAllOrdersAsync();
            return View(orders);
        }

        // GET: AdminOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _unitOfWork.GetOrderByIdAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: AdminOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _unitOfWork.GetOrderByIdAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }
            
            ViewData["Statuses"] = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Select(s => new SelectListItem
                {
                    Text = s.ToString(),
                    Value = ((int)s).ToString(),
                    Selected = s == order.Status
                });
                
            return View(order);
        }

        // POST: AdminOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Status")] Order orderUpdate)
        {
            if (id != orderUpdate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var order = await _unitOfWork.GetOrderByIdAsync(id);
                    if (order == null)
                    {
                        return NotFound();
                    }
                    
                    order.Status = orderUpdate.Status;
                    _unitOfWork.UpdateOrder(order);
                    await _unitOfWork.CompleteAsync();
                }
                catch
                {
                    if (!await OrderExists(orderUpdate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            return View(orderUpdate);
        }

        private async Task<bool> OrderExists(int id)
        {
            return await _unitOfWork.GetOrderByIdAsync(id) != null;
        }
    }
}
