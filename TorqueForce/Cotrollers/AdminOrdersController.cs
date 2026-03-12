using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TorqueForce.Data;
using TorqueForce.Services;

namespace TorqueForce.Cotrollers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminOrdersController : Controller
    {
        private readonly OrderReadRepository _read;
        private readonly OrderStatusService _status;

        public AdminOrdersController(OrderReadRepository read, OrderStatusService status)
        {
            _read = read;
            _status = status;
        }

        public async Task<IActionResult> Index()
        {
            var rows = await _read.GetAllAsync();
            return View(rows);
        }

        [HttpPost]
        public async Task<IActionResult> SetStatus(int orderId, string status)
        {
            await _status.SetStatusAsync(orderId, status);
            return RedirectToAction(nameof(Index));
        }
    }
}
