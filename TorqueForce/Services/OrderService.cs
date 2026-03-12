using Microsoft.EntityFrameworkCore;
using TorqueForce.Data;
using TorqueForce.Models;

namespace TorqueForce.Services
{
    public record CheckoutRequest(string Name, string Phone, string? Email);

    public class OrderService
    {
        private readonly AppDbContext _db;

        public OrderService(AppDbContext db) => _db = db;

        public async Task<int> CreateOrderAsync(CheckoutRequest req, IReadOnlyList<CartItem> cart)
        {
            if (cart.Count == 0) throw new InvalidOperationException("Корзина пуста.");

            await using var tx = await _db.Database.BeginTransactionAsync();

            
            var client = new Client { NameClient = req.Name, Phone = req.Phone, Email = req.Email };
            _db.Clients.Add(client);
            await _db.SaveChangesAsync();

            
            var partIds = cart.Select(x => x.PartId).ToArray();
            var parts = await _db.Parts.Where(p => partIds.Contains(p.PartId)).ToListAsync();

            foreach (var item in cart)
            {
                var p = parts.SingleOrDefault(x => x.PartId == item.PartId)
                    ?? throw new InvalidOperationException($"Запчасть {item.PartId} не найдена.");

                if (item.Quantity <= 0) throw new InvalidOperationException("Некорректное количество.");
                if (p.Stock < item.Quantity) throw new InvalidOperationException($"Недостаточно на складе: {p.PartName}");
            }

            var total = cart.Sum(x => x.Price * x.Quantity);

            var order = new CustomerOrder
            {
                ClientId = client.ClientId,
                OrderDate = DateTime.Now,
                TotalPrice = total
            };
            _db.CustomerOrders.Add(order);
            await _db.SaveChangesAsync();

            
            foreach (var item in cart)
            {
                _db.OrderParts.Add(new OrderPart
                {
                    CustomerOrderId = order.CustomerOrderId,
                    PartId = item.PartId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });

                var p = parts.Single(x => x.PartId == item.PartId);
                p.Stock -= item.Quantity;
            }

            
            var acceptedStepId = await _db.Steps
                .Where(s => s.NameStep == "Принят")
                .Select(s => s.StepId)
                .SingleAsync();

            _db.OrderSteps.Add(new OrderStep
            {
                CustomerOrderId = order.CustomerOrderId,
                StepId = acceptedStepId,
                DateStart = DateTime.Now,
                DateEnd = null
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return order.CustomerOrderId;
        }
    }
}
