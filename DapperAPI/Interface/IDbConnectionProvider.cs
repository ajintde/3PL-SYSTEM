using System.Data;

namespace DapperAPI.Interface
{
    public interface IDbConnectionProvider
    {
        IDbConnection CreateConnection();
        string GetDatabaseType();
    }
}
