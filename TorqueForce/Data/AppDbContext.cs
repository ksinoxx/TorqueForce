using Microsoft.EntityFrameworkCore;
using TorqueForce.Models;

namespace TorqueForce.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Part> Parts => Set<Part>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();
        public DbSet<OrderPart> OrderParts => Set<OrderPart>();
        public DbSet<Step> Steps => Set<Step>();
        public DbSet<OrderStep> OrderSteps => Set<OrderStep>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);
        }
    }

}
