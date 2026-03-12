using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TorqueForce.Data;
using TorqueForce.Services;

namespace TorqueForce.Cotrollers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : Controller
    {
        private readonly PartReadRepository _parts;
        private readonly CartService _cart;

        public CatalogController(PartReadRepository parts, CartService cart)
        {
            _parts = parts;
            _cart = cart;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _parts.GetAllAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int partId, int qty = 1)
        {
            var part = await _parts.GetByIdAsync(partId);
            if (part is null) return NotFound();

            var cart = _cart.Get();
            var existing = cart.FirstOrDefault(x => x.PartId == partId);

            qty = Math.Max(1, qty);

            if (existing is null)
                cart.Add(new CartItem(part.PartId, part.PartName, part.Price, qty));
            else
                cart = cart.Select(x => x.PartId == partId ? x with { Quantity = x.Quantity + qty } : x).ToList();

            _cart.Save(cart);
            return RedirectToAction("Index", "Cart");
        }
    }
}
