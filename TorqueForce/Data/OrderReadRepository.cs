using Dapper;

namespace TorqueForce.Data
{
    public record OrderRow(int CustomerOrderId, DateTime OrderDate, decimal TotalPrice, string ClientName, string Status);

    public class OrderReadRepository
    {
        private readonly SqlConnectionFactory _factory;
        public OrderReadRepository(SqlConnectionFactory factory) => _factory = factory;

        public async Task<IReadOnlyList<OrderRow>> GetAllAsync()
        {
            
            const string sql = """
        SELECT 
            co.customer_order_id as CustomerOrderId,
            co.order_date as OrderDate,
            co.total_price as TotalPrice,
            c.name_client as ClientName,
            ISNULL(s.name_step, N'—') as Status
        FROM customer_order co
        JOIN client c ON c.client_id = co.client_id
        OUTER APPLY (
            SELECT TOP(1) os.step_id
            FROM order_step os
            WHERE os.customer_order_id = co.customer_order_id
            ORDER BY os.date_start DESC
        ) lastos
        LEFT JOIN step s ON s.step_id = lastos.step_id
        ORDER BY co.customer_order_id DESC
        """;

            using var con = _factory.Create();
            var rows = await con.QueryAsync<OrderRow>(sql);
            return rows.ToList();
        }
    }
}
