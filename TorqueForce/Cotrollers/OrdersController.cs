using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TorqueForce.Services;

namespace TorqueForce.Cotrollers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly CartService _cart;
        private readonly OrderService _orders;

        public OrdersController(CartService cart, OrderService orders)
        {
            _cart = cart;
            _orders = orders;
        }

        [HttpGet]
        public IActionResult Checkout() => View();

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutRequest req)
        {
            var cart = _cart.Get();
            var id = await _orders.CreateOrderAsync(req, cart);
            _cart.Clear();
            return RedirectToAction(nameof(Success), new { id });
        }

        public IActionResult Success(int id) => View(model: id);
    }
}
