using DapperAPI.EntityModel;
using System.Linq.Expressions;

namespace DapperAPI.Interface
{
    public interface ITwoRepository<T, TDetail> where T : class where TDetail : class
    {
        //Task<T> GetAll(T obj);

        Task<IEnumerable<T>> GetAll(string companyCode,string user);
        Task<T> GetById(string id, string companyCode, string user);
        Task<CommonResponse<T>> Insert(T obj, string companyCode, string user);
        Task<CommonResponse<TDetail>> InsertDetail(TDetail detail, string companyCode, string user);
        Task<CommonResponse<T>> InsertBySeq(T obj, string companyCode, string user);
        Task<CommonResponse<TDetail>> InsertDetailBySeq(TDetail obj, string companyCode, string user);
        Task<CommonResponse<T>> Update(T obj, string companyCode, string user);
        Task<CommonResponse<TDetail>> UpdateDetail(TDetail detail, string companyCode, string user);
        Task<CommonResponse<TDetail>> UpdateDetailByIdentity(TDetail detail, string companyCode, string user);
        Task<CommonResponse<IEnumerable<T>>> Search<T>(string jsonModel, string SortBy, int pageNo, int pageSize, string companyCode, string user, string whereClause, string showDetail); Task<CommonResponse<T>> Delete(T obj, string companyCode, string user);
        Task<CommonResponse<TDetail>> DeleteDetail(TDetail detail, string companyCode, string user);
        Task<CommonResponse<object>> Import(List<T> obj, string companyCode, string user, string result);
    }
}
