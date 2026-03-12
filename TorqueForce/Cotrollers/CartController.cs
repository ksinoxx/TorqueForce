using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TorqueForce.Services;

namespace TorqueForce.Cotrollers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : Controller
    {
        private readonly CartService _cart;
        public CartController(CartService cart) => _cart = cart;

        public IActionResult Index()
        {
            var cart = _cart.Get();
            return View(cart);
        }

        [HttpPost]
        public IActionResult Remove(int partId)
        {
            var cart = _cart.Get().Where(x => x.PartId != partId).ToList();
            _cart.Save(cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Clear()
        {
            _cart.Clear();
            return RedirectToAction(nameof(Index));
        }
    }
}
