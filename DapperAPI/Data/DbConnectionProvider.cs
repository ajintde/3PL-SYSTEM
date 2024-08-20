using DapperAPI.Interface;
using DapperAPI.Setting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace DapperAPI.Data
{
    public class DbConnectionProvider : IDbConnectionProvider
    {
        private readonly SqlConnectionSetting _sqlOptions;
        private readonly OracleConnectionSetting _oracleOptions;
        private readonly string _databaseType;

        public DbConnectionProvider(IOptions<SqlConnectionSetting> sqlOptions,IOptions<OracleConnectionSetting> oracleOptions,
        IOptions<DatabaseTypeSetting> databaseTypeOptions) 
        {
            _sqlOptions = sqlOptions.Value;
            _oracleOptions = oracleOptions.Value;
            _databaseType = databaseTypeOptions.Value.DatabaseType;
        }
        public IDbConnection CreateConnection()
        {
            IDbConnection con;
            if (_databaseType == "SQL")
            {
                con = new SqlConnection(_sqlOptions.SqlConnectionString);
            }
            else if (_databaseType == "ORACLE")
            {
                con = new OracleConnection(_oracleOptions.OracleConnectionString);
            }
            else
            {
                throw new InvalidOperationException("Unsupported database type.");
            }

            con.Open();
            return con;
        }
        public string GetDatabaseType() => _databaseType;
    }
    public class DatabaseTypeSetting
    {
        public string DatabaseType { get; set; }
    }

    
}
