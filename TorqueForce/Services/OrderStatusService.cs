using Microsoft.EntityFrameworkCore;
using TorqueForce.Data;
using TorqueForce.Models;

namespace TorqueForce.Services
{
    public class OrderStatusService
    {
        private readonly AppDbContext _db;
        public OrderStatusService(AppDbContext db) => _db = db;

        private static readonly string[] Allowed = ["Принят", "В обработке", "Выдан"];

        public async Task SetStatusAsync(int orderId, string newStatus)
        {
            if (!Allowed.Contains(newStatus))
                throw new InvalidOperationException("Недопустимый статус.");

            await using var tx = await _db.Database.BeginTransactionAsync();

            var stepId = await _db.Steps
                .Where(s => s.NameStep == newStatus)
                .Select(s => s.StepId)
                .SingleAsync();

            
            var current = await _db.OrderSteps
                .Where(x => x.CustomerOrderId == orderId)
                .OrderByDescending(x => x.DateStart)
                .FirstOrDefaultAsync();

            
            if (current is not null && current.StepId == stepId)
                return;

            
            if (current is not null && current.DateEnd is null)
            {
                current.DateEnd = DateTime.Now;
            }

            
            _db.OrderSteps.Add(new OrderStep
            {
                CustomerOrderId = orderId,
                StepId = stepId,
                DateStart = DateTime.Now,
                DateEnd = null
            });

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }
}
