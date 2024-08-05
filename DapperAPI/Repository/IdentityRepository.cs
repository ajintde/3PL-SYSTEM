using Dapper;
using DapperAPI.Data;
using DapperAPI.EntityModel;
using DapperAPI.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using static DapperAPI.EntityModel.CustomAttributes;

namespace DapperAPI.Repository
{
    public class IdentityRepository<T, TDetail> : IMasterDetailRepository<T, TDetail> where T : class where TDetail : class
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;
        private readonly string _tableName;
        private readonly string _detailTableName;
        private readonly ILogger<T> _logger;

        public IdentityRepository(IDbConnectionProvider dbConnectionProvider, string tableName, string detailTableName, ILogger<T> logger)
        {
            _dbConnectionProvider = dbConnectionProvider;
            _tableName = tableName;
            _detailTableName = detailTableName;
            _logger = logger;
        }

        private PropertyInfo GetPrimaryKeyPropertyName()
        {

            return typeof(T).GetProperties()
            .FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
        }

        private PropertyInfo GetForeignKeyPropertyName()
        {
            return typeof(TDetail).GetProperties()
            .FirstOrDefault(p => p.GetCustomAttribute<CustomAttributes.ForeignKeyAttribute>() != null);
        }

        private IEnumerable<string> GetColumnNames<T>(bool forInsert = false)
        {
            var properties = typeof(T).GetProperties()
        .Where(p => !Attribute.IsDefined(p, typeof(NotMappedAttribute))); // Exclude properties marked with [NotMapped]

            if (forInsert)
            {
                // Include all properties for INSERT, including those ending with "UPD_DT"
                return properties.Select(p => p.Name);
            }
            else
            {
                // Exclude properties ending with "UPD_DT" for UPDATE
                return properties.Select(p => p.Name).Where(c => !c.EndsWith("_UPD_DT"));
            }
        }

        public Task<CommonResponse<T>> Delete(T obj, string companyCode, string user)
        {
            throw new NotImplementedException();
        }

        

        public Task<IEnumerable<T>> GetAll(string companyCode, string user)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetById(string id, string companyCode, string user)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponse<T>> Insert(T obj, string companyCode, string user)
        {
            throw new NotImplementedException();
        }

        public async Task<CommonResponse<T>> InsertByIdentity(T obj, string companyCode, string user)
        {
            var response = new CommonResponse<T>();
            var primaryKeyProperty = GetPrimaryKeyPropertyName();
            var foreignKeyProperty = GetForeignKeyPropertyName();

            if (primaryKeyProperty == null || foreignKeyProperty == null)
            {
                response.ValidationSuccess = false;
                response.SuccessString = "500";
                response.ErrorString = "Primary key or foreign key property not found.";
                return response;
            }

            // Generate the INSERT SQL statement for the header
            var insertHeaderColumns = GetColumnNames<T>(true).ToList();
            var insertHeaderValues = insertHeaderColumns.Select(c => $"@{c}").ToList();

            // Exclude the primary key column from the INSERT statement
            var primaryKeyColumnName = primaryKeyProperty.Name;
            insertHeaderColumns = insertHeaderColumns.Where(c => c != primaryKeyColumnName).ToList();
            insertHeaderValues = insertHeaderValues.Where(v => v != $"@{primaryKeyColumnName}").ToList();

            // Replace _CR_DT columns with GETDATE()
            for (int i = 0; i < insertHeaderColumns.Count; i++)
            {
                if (insertHeaderColumns[i].EndsWith("_CR_DT"))
                {
                    insertHeaderValues[i] = "GETDATE()";
                }
            }

            var insertHeaderSql = $@"
INSERT INTO {_tableName} ({string.Join(",", insertHeaderColumns)})
VALUES ({string.Join(",", insertHeaderValues)});
SELECT CAST(SCOPE_IDENTITY() as int);

            ";

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert the header and get the identity value
                        var primaryKeyValue = await conn.ExecuteScalarAsync<int>(insertHeaderSql, obj, transaction);

                        // Generate the INSERT SQL statement for the details
                        var insertDetailColumns = GetColumnNames<TDetail>(true).ToList();
                        var insertDetailValues = insertDetailColumns.Select(c => $"@{c}").ToList();

                        // Exclude the identity column from the detail insert
                        var detailPrimaryKeyProperty = typeof(TDetail).GetProperties().FirstOrDefault(p => Attribute.IsDefined(p, typeof(KeyAttribute)));
                        if (detailPrimaryKeyProperty != null)
                        {
                            var detailPrimaryKeyColumnName = detailPrimaryKeyProperty.Name;
                            insertDetailColumns = insertDetailColumns.Where(c => c != detailPrimaryKeyColumnName).ToList();
                            insertDetailValues = insertDetailValues.Where(v => v != $"@{detailPrimaryKeyColumnName}").ToList();
                        }

                        // Replace _CR_DT columns with GETDATE()
                        for (int i = 0; i < insertDetailColumns.Count; i++)
                        {
                            if (insertDetailColumns[i].EndsWith("_CR_DT"))
                            {
                                insertDetailValues[i] = "GETDATE()";
                            }
                        }

                        var insertDetailSql = $@"
INSERT INTO {_detailTableName} ({string.Join(",", insertDetailColumns)})
VALUES ({string.Join(",", insertDetailValues)});
";

                        // Get the detail list property and insert each detail entity
                        var detailListProperty = typeof(T).GetProperty(_tableName + "_" + _detailTableName);
                        var detailList = detailListProperty.GetValue(obj) as IList<TDetail>;

                        foreach (var detail in detailList)
                        {
                            // Set the foreign key value to match the primary key of the header
                            foreignKeyProperty.SetValue(detail, primaryKeyValue);

                            await conn.ExecuteAsync(insertDetailSql, detail, transaction);
                        }

                        // Commit the transaction
                        transaction.Commit();

                        var newData = await GetById(primaryKeyValue.ToString(), companyCode, user);

                        response.ValidationSuccess = true;
                        response.SuccessString = "200";
                        response.ReturnCompleteRow = newData;
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();
                        response.ValidationSuccess = false;
                        response.SuccessString = "500";
                        response.ErrorString = ex.Message;
                    }
                }
            }

            return response;
        }

        public Task<CommonResponse<IEnumerable<T1>>> Search<T1>(string jsonModel, string SortBy, int pageNo, int pageSize, string companyCode, string user, string whereClause, string showDetail)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponse<T>> Update(T obj, string companyCode, string user)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponse<TDetail>> UpdateDetail(TDetail detail, string companyCode, string user)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponse<TDetail>> DeleteDetail(TDetail detail, string companyCode, string user)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponse<TDetail>> UpdateDetailByIdentity(TDetail detail, string companyCode, string user)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponse<T>> InsertBySeq(T obj, string companyCode, string user)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponse<TDetail>> InsertDetail(TDetail detail, string companyCode, string user)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponse<T>> InsertDeatilBySeq(TDetail obj, string companyCode, string user)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponse<TDetail>> InsertDetailBySeq(TDetail obj, string companyCode, string user)
        {
            throw new NotImplementedException();
        }
        public Task<CommonResponse<object>> Import(List<T> obj, string companyCode, string user, string result)
        {
            throw new NotImplementedException();
        }
    }
}
