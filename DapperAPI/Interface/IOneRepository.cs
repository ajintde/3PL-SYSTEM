using DapperAPI.EntityModel;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace DapperAPI.Interface
{
    public interface IOneRepository<T> where T : class
    {
       Task<IEnumerable<T>> GetAll(string companyCode, string user);
        Task<T> GetById(object id, string companyCode, string user);
        Task<int> Insert(T obj, string companyCode, string user);
        Task<int> Update(T obj, string companyCode, string user);
        Task<int> Delete(object id, string companyCode, string user);

        Task<CommonResponse<IEnumerable<T>>> Search<T>(string jsonModel, string SortBy, int pageNo, int pageSize, string companyCode, string user, string whereClause, string showDetail);
    }
}
