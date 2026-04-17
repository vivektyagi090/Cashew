using Microsoft.Data.SqlClient;

namespace WebApi.Data
{
    public class DbConnectionFactory
    {
        private readonly IConfiguration _config;

        public DbConnectionFactory(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(
                _config.GetConnectionString("DefaultConnection"));
        }
    }
}
