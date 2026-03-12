using Microsoft.Data.SqlClient;

namespace TorqueForce.Data
{
    public class SqlConnectionFactory
    {
        private readonly IConfiguration _cfg;
        public SqlConnectionFactory(IConfiguration cfg) => _cfg = cfg;

        public SqlConnection Create()
            => new SqlConnection(_cfg.GetConnectionString("Default"));
    }
}
