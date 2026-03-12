using System.Text.Json;

namespace TorqueForce.Services
{
    public record CartItem(int PartId, string PartName, decimal Price, int Quantity);

    public class CartService
    {
        private const string Key = "CART";
        private readonly IHttpContextAccessor _http;

        public CartService(IHttpContextAccessor http) => _http = http;

        private ISession Session => _http.HttpContext!.Session;

        public List<CartItem> Get()
        {
            var json = Session.GetString(Key);
            return json is null ? new List<CartItem>() : (JsonSerializer.Deserialize<List<CartItem>>(json) ?? new());
        }

        public void Save(List<CartItem> cart) =>
            Session.SetString(Key, JsonSerializer.Serialize(cart));

        public void Clear() => Session.Remove(Key);
    }
}
