using Dapper;

namespace TorqueForce.Data
{
    public record PartListItem(int PartId, string PartName, decimal Price, int Stock);

    public class PartReadRepository
    {
        private readonly SqlConnectionFactory _factory;
        public PartReadRepository(SqlConnectionFactory factory) => _factory = factory;

        public async Task<IReadOnlyList<PartListItem>> GetAllAsync()
        {
            const string sql = """
        SELECT part_id as PartId, part_name as PartName, price as Price, stock as Stock
        FROM part
        ORDER BY part_name
        """;

            using var con = _factory.Create();
            var rows = await con.QueryAsync<PartListItem>(sql);
            return rows.ToList();
        }

        public async Task<PartListItem?> GetByIdAsync(int partId)
        {
            const string sql = """
        SELECT part_id as PartId, part_name as PartName, price as Price, stock as Stock
        FROM part
        WHERE part_id = @partId
        """;
            using var con = _factory.Create();
            return await con.QuerySingleOrDefaultAsync<PartListItem>(sql, new { partId });
        }
    }
}
