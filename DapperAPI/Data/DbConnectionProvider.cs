using DapperAPI.Interface;
using DapperAPI.Setting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace DapperAPI.Data
{
    public class DbConnectionProvider : IDbConnectionProvider
    {
        private readonly SqlConnectionSetting _options;

        public DbConnectionProvider(IOptions<SqlConnectionSetting> options) 
        {
            _options = options.Value;
        }
        public IDbConnection CreateConnection()
        {
            var con = new SqlConnection(_options.SqlConnectionString);
            con.Open();
            return con;
        }
    }
}
