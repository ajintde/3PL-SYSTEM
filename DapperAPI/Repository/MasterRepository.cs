using Dapper;
using DapperAPI.EntityModel;
using DapperAPI.Interface;
using DapperAPI.Setting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;
using static Dapper.SqlMapper;
using static DapperAPI.EntityModel.CustomAttributes;

namespace DapperAPI.Repository
{
    public class MasterRepository<T> : IMasterRepository<T> where T : class
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;
        //private readonly string _spName;
        private readonly string _primaryKey;
        private readonly string _tableName;

        private readonly AppSettings _appSettings;



        public MasterRepository(IDbConnectionProvider dbConnectionProvider, IOptions<AppSettings> appSettings) 
        {
            _dbConnectionProvider = dbConnectionProvider;
            var obj= BaseEntity.CreateEmptyInstances<T>();
            _primaryKey = typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttributes<KeyAttribute>().Any())?.Name;
            _appSettings = appSettings.Value;
            _tableName = typeof(T).Name;
            //_spName=obj.StoreProcedureName;

        }

        private PropertyInfo[] GetPrimaryKeyProperties<T>()
        {
            //return typeof(T).GetProperties().Where(p => Attribute.IsDefined(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute))).ToArray();

            return typeof(T).GetProperties()
        .Where(prop => Attribute.IsDefined(prop, typeof(PrimaryKeyAttribute)) || Attribute.IsDefined(prop, typeof(KeyAttribute))).ToArray();
        }
        public async Task<IEnumerable<T>> GetAll(string companyCode, string user)
        {
            //var spName= "SP_UOM";
            using(var conn= _dbConnectionProvider.CreateConnection())
            {
                //////var parameters = new DynamicParameters();
                //////parameters.Add("OperationType", "SELECT",DbType.String,ParameterDirection.Input);
                //////parameters.Add("ID",null,DbType.Int32,ParameterDirection.Input);
                //////var getObj = await conn.QueryAsync<T>(spName, parameters, commandType: CommandType.StoredProcedure);
                //////return getObj.ToList();
                ///

                try
                {
                    var _tableName = typeof(T).Name;
                    var sql = $"SELECT * FROM {_tableName}";
                    var results = await conn.QueryAsync<T>(sql);
                    return results.ToList();
                }
                catch (Exception ex)
                {
                    
                    throw;
                }

            }
        }

        public async Task<T> GetById(object id, string companyCode, string user)
        {
        //    var primaryKeyProperty = typeof(T).GetProperties()
        //.FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Length > 0);
            var primaryKeyColumnName=typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttributes<KeyAttribute>().Any())?.Name;


            if (primaryKeyColumnName == null)
            {
                throw new InvalidOperationException("Primary key property not found.");
            }

            //var primaryKeyColumnName = primaryKeyProperty.Name;
            var tableName = typeof(T).Name;
            var sql = $"SELECT * FROM {tableName} WHERE {primaryKeyColumnName} = @Id";

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                return await conn.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
            }


        }

        public async Task<int> Insert(T obj, string companyCode, string user)
        {
            var sql = GenerateInsertSql(obj);
            using (var conn = _dbConnectionProvider.CreateConnection())
            {

                var rowsAffected = await conn.ExecuteAsync(sql, obj);
                return rowsAffected;
            }
        }

        public async Task<int> Update(T obj, string companyCode, string user)
        {
            var primaryKeyColumnName = typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttributes<KeyAttribute>().Any())?.Name;


            if (primaryKeyColumnName == null)
            {
                throw new InvalidOperationException("Primary key property not found.");
            }

            //var primaryKeyColumnName = primaryKeyProperty.Name;
            var tableName = typeof(T).Name;
            var properties = GetProperties(obj);
            var updateColumns = string.Join(",", properties.Where(x => x.Name != primaryKeyColumnName).Select(x => $"{x.Name} = @{x.Name}"));
            var sql = $"UPDATE {tableName} SET {updateColumns} WHERE {primaryKeyColumnName} = @{primaryKeyColumnName}";
            var parameters = GetParameters(obj);
            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                var rowsAffected = await conn.ExecuteAsync(sql, parameters);
                return rowsAffected;                
            }
        }


        public async Task<int> Delete(Object id, string companyCode, string user)
        {
            var primaryKeyColumnName = typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttributes<KeyAttribute>().Any())?.Name;


            if (primaryKeyColumnName == null)
            {
                throw new InvalidOperationException("Primary key property not found.");
            }

            //var primaryKeyColumnName = primaryKeyProperty.Name;
            var tableName = typeof(T).Name;
            //var sql = $"SELECT * FROM {tableName} WHERE {primaryKeyColumnName} = @Id";

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                var sql = $"DELETE FROM {tableName} WHERE {primaryKeyColumnName} = @id";
                var rowsAffected = await conn.ExecuteAsync(sql, new { id });
                return rowsAffected;
            }
        }

        public async Task<CommonResponse<IEnumerable<T>>> Search<T>(string jsonModel, string sortBy, int pageNo, int pageSize, string companyCode, string user, string whereClause, string showDetail)
        {
            var response = new CommonResponse<IEnumerable<T>>();
            try
            {
                var whereConditions = new List<string>();
                var parameters = new DynamicParameters();

                // Log the raw input data from Postman
                Console.WriteLine("Raw Input Data:");
                Console.WriteLine($"jsonModel: {jsonModel}");
                Console.WriteLine($"sortBy: {sortBy}");
                Console.WriteLine($"pageNo: {pageNo}");
                Console.WriteLine($"pageSize: {pageSize}");
                Console.WriteLine($"companyCode: {companyCode}");
                Console.WriteLine($"user: {user}");
                Console.WriteLine($"whereClause: {whereClause}");
                Console.WriteLine($"showDetail: {showDetail}");

                // Deserialize the JSON model into a dictionary if not null
                if (!string.IsNullOrEmpty(jsonModel))
                {
                    var modelDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonModel);

                    // Add properties from the model to the whereConditions list
                    foreach (var kvp in modelDict)
                    {
                        var columnName = kvp.Key.ToUpper();
                        var value = kvp.Value;

                        if (value != null && !string.IsNullOrEmpty(value.ToString()))
                        {
                            whereConditions.Add($"{columnName} LIKE @{columnName}");
                            parameters.Add($"@{columnName}", value.ToString());
                        }
                    }
                }

                // Add dynamic conditions for properties ending with _COMP_CODE, excluding fm_comp_code and to_comp_code
                var modelProperties = typeof(T).GetProperties();
                var compCodeProperties = modelProperties.Where(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase) &&
                                                                    !string.Equals(p.Name, "FM_COMP_CODE", StringComparison.OrdinalIgnoreCase) &&
                                                                    !string.Equals(p.Name, "TO_COMP_CODE", StringComparison.OrdinalIgnoreCase)).ToList();

                if (companyCode == null)
                {
                    // Query v_adm_user_locn to get all comp_code for the given user
                    var view_Query = "SELECT DISTINCT UL_COMP_CODE FROM v_adm_user_locn WHERE ul_frz_flag = 'N' AND ul_user_id = @User";
                    parameters.Add("@User", user);

                    using (var conn = _dbConnectionProvider.CreateConnection())
                    {
                        var compCodes = (await conn.QueryAsync<string>(view_Query, parameters)).ToList();

                        if (compCodes.Any())
                        {
                            // Add comp_code conditions to the where clause using IN clause
                            var compCodeCondition = $"{compCodeProperties.First().Name} IN ({string.Join(", ", compCodes.Select(compCode => $"'{compCode}'"))})";
                            whereConditions.Add($"({compCodeCondition})");
                        }
                    }
                }
                else
                {
                    foreach (var prop in compCodeProperties)
                    {
                        var columnName = prop.Name.ToUpper();
                        whereConditions.Add($"{columnName} = @{columnName}");
                        parameters.Add($"@{columnName}", companyCode);
                    }
                }

                // Process additional whereClause condition if provided
                if (!string.IsNullOrEmpty(whereClause))
                {
                    var modifiedWhereClause = whereClause;

                    // Handle _CR_DT and _UPD_DT columns with ConvertDate function
                    modifiedWhereClause = Regex.Replace(modifiedWhereClause, @"(\b\w+_CR_DT\b|\b\w+_UPD_DT\b)", match => $"dbo.ConvertDate({match.Value})");

                    whereConditions.Add(modifiedWhereClause);
                }

                // Construct the final SQL query with paging
                var whereSql = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : string.Empty;
                var offset = (pageNo - 1) * pageSize;

                // Determine the sort column
                string orderByColumn = !string.IsNullOrEmpty(sortBy) ? sortBy.ToUpper() : modelProperties.First(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Length > 0).Name.ToUpper();

                var sql = $@"
SELECT * 
FROM {_tableName}
{whereSql}
ORDER BY {orderByColumn}
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

                parameters.Add("@Offset", offset);
                parameters.Add("@PageSize", pageSize);

                using (var conn = _dbConnectionProvider.CreateConnection())
                {
                    // Log the SQL query and parameters for debugging
                    Console.WriteLine("SQL Query: " + sql);
                    foreach (var param in parameters.ParameterNames)
                    {
                        Console.WriteLine($"{param}: {parameters.Get<dynamic>(param)}");
                    }

                    var result = (await conn.QueryAsync<T>(sql, parameters)).ToList();

                    if (showDetail == "Y")
                    {
                        var detailProperty = modelProperties.FirstOrDefault(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>));
                        if (detailProperty != null)
                        {
                            var detailType = detailProperty.PropertyType.GetGenericArguments().First();
                            var detailTableName = detailType.Name;

                            // Get primary key and foreign key property names
                            var primaryKeyProperties = GetPrimaryKeyProperties<T>();
                            var foreignKeyProperties = detailType.GetProperties().Where(p =>
                            {
                                var foreignKeyAttribute = AttributeHelper.GetCustomAttribute<CustomAttributes.ForeignKeyAttribute>(p);
                                return foreignKeyAttribute != null && foreignKeyAttribute.ModelType == typeof(T);
                            }).ToList();

                            // Filter out _COMP_CODE if it is not a KeyAttribute
                            var keyAttributeProperties = detailType.GetProperties().Where(p => Attribute.IsDefined(p, typeof(KeyAttribute)));
                            var filteredForeignKeyProperties = foreignKeyProperties.Where(fk => keyAttributeProperties.Contains(fk)).ToList();

                            // Construct the join query
                            var joinConditions = string.Join(" AND ", filteredForeignKeyProperties.Select(fk =>
                            {
                                var foreignKeyAttribute = AttributeHelper.GetCustomAttribute<CustomAttributes.ForeignKeyAttribute>(fk);
                                return $"{detailTableName}.{fk.Name} = {_tableName}.{foreignKeyAttribute.ColumnName}";
                            }));

                            if (!string.IsNullOrEmpty(joinConditions))
                            {
                                var detailSql = $@"
SELECT * 
FROM {detailTableName}
JOIN {_tableName} ON {joinConditions}
{whereSql}
ORDER BY {orderByColumn}
;";

                                var details = (await conn.QueryAsync(detailType, detailSql, parameters)).ToList();
                                foreach (var item in result)
                                {
                                    var detailList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(detailType));
                                    var primaryKeyValue = primaryKeyProperties.First().GetValue(item);

                                    foreach (var detail in details)
                                    {
                                        var foreignKeyValue = foreignKeyProperties.First().GetValue(detail);
                                        if (primaryKeyValue.Equals(foreignKeyValue))
                                        {
                                            detailList.Add(detail);
                                        }
                                    }
                                    detailProperty.SetValue(item, detailList);
                                }
                            }
                        }
                    }

                    response.ValidationSuccess = true;
                    response.SuccessString = "200";
                    response.ReturnCompleteRow = result;
                }
            }
            catch (Exception ex)
            {
                response.ValidationSuccess = false;
                response.SuccessString = "500";
                response.ErrorString = ex.Message;
            }

            return response;
        }

        

        private string GenerateInsertSql(T obj)
        {
            ////var type = typeof(T);
            ////var tableName = type.Name; // Assuming the table name is the same as the class name
            ////var properties = type.GetProperties();

            ////var columnNames = string.Join(", ", properties.Select(p => p.Name));
            ////var parameterNames = string.Join(", ", properties.Select(p => "@" + p.Name));

            ////return $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames}); SELECT SCOPE_IDENTITY();";
            ///
            var type = typeof(T);
            var tableName = type.Name; // Assuming the table name is the same as the class name
            var properties = type.GetProperties();

            // Identify properties ending with _CR_DT
            var crDtProperties = properties.Where(p => p.Name.EndsWith("_CR_DT", StringComparison.OrdinalIgnoreCase)).ToList();

            // Prepare column names and parameter names excluding _CR_DT properties
            var columnNames = string.Join(", ", properties.Select(p => p.Name));
            var parameterNames = string.Join(", ", properties.Select(p =>
                crDtProperties.Contains(p) ? "GETDATE()" : "@" + p.Name));

            return $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames}); SELECT SCOPE_IDENTITY();";
        }

        //private string GenerateUpdateSql(T obj)
        //{
        //    //var type = typeof(T);
        //    //var tableName = type.Name;
        //    //var properties = type.GetProperties();

        //    //var updateClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

        //    //return $"UPDATE {tableName} SET {updateClause} WHERE Id = @Id";
        //}

        private static IEnumerable<PropertyInfo> GetProperties(object entity)
        {
            return entity.GetType().GetProperties();
        }
        private IDictionary<string, object> GetParameters(T entity)
        {
            var properties = GetProperties(entity);
            var parameters = new Dictionary<string, object>();
            foreach (var property in properties)
            {
                parameters.Add(property.Name, property.GetValue(entity));
            }
            return parameters;
        }
    }
}
