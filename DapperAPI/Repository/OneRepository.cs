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
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static Dapper.SqlMapper;
using static DapperAPI.EntityModel.CustomAttributes;

namespace DapperAPI.Repository
{
    public class OneRepository<T> : IOneRepository<T> where T : class
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;
        //private readonly string _spName;
        private readonly string _primaryKey;
        private readonly string _tableName;

        private readonly AppSettings _appSettings;



        public OneRepository(IDbConnectionProvider dbConnectionProvider, IOptions<AppSettings> appSettings) 
        {
            _dbConnectionProvider = dbConnectionProvider;
            var obj= BaseEntity.CreateEmptyInstances<T>();
            _primaryKey = typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttributes<KeyAttribute>().Any())?.Name;
            _appSettings = appSettings.Value;
            _tableName = typeof(T).Name;
            //_spName=obj.StoreProcedureName;

        }

        private string GetCurrentDateTime()
        {
            // Return the current date and time in a format compatible with both SQL Server and Oracle
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        private PropertyInfo[] GetPrimaryKeyProperties<T>()
        {
            //return typeof(T).GetProperties().Where(p => Attribute.IsDefined(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute))).ToArray();

            return typeof(T).GetProperties()
        .Where(prop => Attribute.IsDefined(prop, typeof(PrimaryKeyAttribute)) || Attribute.IsDefined(prop, typeof(KeyAttribute))).ToArray();
        }
        public async Task<CommonResponse<IEnumerable<T>>> GetAll(string companyCode, string user)
        {
            var response = new CommonResponse<IEnumerable<T>>();
            using (var conn= _dbConnectionProvider.CreateConnection())
            {
                
                try
                {
                    var _tableName = typeof(T).Name;
                    var sql = $"SELECT * FROM {_tableName}";
                    var results = await conn.QueryAsync<T>(sql);
                    response.ValidationSuccess = true;
                    response.StatusCode = _appSettings.StatusCodes.Success;
                    response.ReturnCompleteRow = results;
                }
                catch (Exception ex)
                {

                    response.ValidationSuccess = false;
                    response.StatusCode = _appSettings.StatusCodes.Error;
                    response.ErrorString = ex.Message;
                }

            }
            return response;
        }

        public async Task<CommonResponse<T>> GetById(object id, string companyCode, string user)
        {
            var response = new CommonResponse<T>();
            try
            {
                var primaryKeyColumnName = typeof(T).GetProperties()
                    .FirstOrDefault(p => p.GetCustomAttributes<KeyAttribute>().Any())?.Name;

                if (primaryKeyColumnName == null)
                {
                    throw new InvalidOperationException($"{_appSettings.SuccessStrings.PrimaryKeyNotFound}");
                }

                var tableName = typeof(T).Name;
                var sql = $"SELECT * FROM {tableName} WHERE {primaryKeyColumnName} = @Id";

                using (var conn = _dbConnectionProvider.CreateConnection())
                {
                    var result = await conn.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
                    if (result != null)
                    {
                        response.ValidationSuccess = true;
                        response.StatusCode = _appSettings.StatusCodes.Success;
                        response.ReturnCompleteRow = result;
                    }
                    else
                    {
                        response.ValidationSuccess = false;
                        response.StatusCode = _appSettings.StatusCodes.NotFound;
                        response.ErrorString = _appSettings.SuccessStrings.NoData;
                    }
                }
            }
            catch (Exception ex)
            {
                response.ValidationSuccess = false;
                response.StatusCode = _appSettings.StatusCodes.Error;
                response.ErrorString = ex.Message;
            }

            return response;
        }

        public async Task<CommonResponse<T>> Insert(T obj, string companyCode, string user)
        {
            var response = new CommonResponse<T>();
            try
            {
                var sql = GenerateInsertSql(obj);
                using (var conn = _dbConnectionProvider.CreateConnection())
                {
                    var rowsAffected = await conn.ExecuteAsync(sql, obj);
                    if (rowsAffected > 0)
                    {
                        response.ValidationSuccess = true;
                        response.StatusCode = _appSettings.StatusCodes.Success;
                        response.ReturnCompleteRow = obj;
                    }
                    else
                    {
                        response.ValidationSuccess = false;
                        response.StatusCode = _appSettings.StatusCodes.Error;
                        response.ErrorString = _appSettings.SuccessStrings.InsertFail;
                    }
                }
            }
            catch (Exception ex)
            {
                response.ValidationSuccess = false;
                response.StatusCode = _appSettings.StatusCodes.Error;
                response.ErrorString = ex.Message;
            }

            return response;
        }

        ////public async Task<int> Update(T obj, string companyCode, string user)
        ////{
        ////    var primaryKeyColumnName = typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttributes<KeyAttribute>().Any())?.Name;


        ////    if (primaryKeyColumnName == null)
        ////    {
        ////        throw new InvalidOperationException("Primary key property not found.");
        ////    }

        ////    //var primaryKeyColumnName = primaryKeyProperty.Name;
        ////    var tableName = typeof(T).Name;
        ////    var properties = GetProperties(obj);
        ////    var updateColumns = string.Join(",", properties.Where(x => x.Name != primaryKeyColumnName).Select(x => $"{x.Name} = @{x.Name}"));
        ////    var sql = $"UPDATE {tableName} SET {updateColumns} WHERE {primaryKeyColumnName} = @{primaryKeyColumnName}";
        ////    var parameters = GetParameters(obj);
        ////    using (var conn = _dbConnectionProvider.CreateConnection())
        ////    {
        ////        var rowsAffected = await conn.ExecuteAsync(sql, parameters);
        ////        return rowsAffected;                
        ////    }
        ////}

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
        ////public async Task<int> Delete(Object id, string companyCode, string user)
        ////{
        ////    var primaryKeyColumnName = typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttributes<KeyAttribute>().Any())?.Name;


        ////    if (primaryKeyColumnName == null)
        ////    {
        ////        throw new InvalidOperationException("Primary key property not found.");
        ////    }

        ////    //var primaryKeyColumnName = primaryKeyProperty.Name;
        ////    var tableName = typeof(T).Name;
        ////    //var sql = $"SELECT * FROM {tableName} WHERE {primaryKeyColumnName} = @Id";

        ////    using (var conn = _dbConnectionProvider.CreateConnection())
        ////    {
        ////        var sql = $"DELETE FROM {tableName} WHERE {primaryKeyColumnName} = @id";
        ////        var rowsAffected = await conn.ExecuteAsync(sql, new { id });
        ////        return rowsAffected;
        ////    }
        ////}

        public async Task<CommonResponse<T>> Delete(T detail, string companyCode, string user)
        {
            var response = new CommonResponse<T>();
            var detailKeyProperties = GetPrimaryKeyProperties<T>();
            var updDtPropertyDetail = typeof(T).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_UPD_DT", StringComparison.OrdinalIgnoreCase));
            var compCodeProperty = typeof(T).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase));

            if (!detailKeyProperties.Any())
            {
                throw new InvalidOperationException($"{_appSettings.SuccessStrings.PrimaryKeyNotFound}");
            }

            var primaryKeyValues = detailKeyProperties.Select(p => p.GetValue(detail)?.ToString()).ToArray();
            if (primaryKeyValues.Any(string.IsNullOrEmpty))
            {
                throw new InvalidOperationException($"{_appSettings.SuccessStrings.PrimaryKeyError}");

            }

          

            // Generate the WHERE clause for the detail table
            var detailWhereClauses = detailKeyProperties
                .Select(p => $"{p.Name} = @{p.Name}")
                .ToList();
            if (compCodeProperty != null && companyCode != null)
            {
                detailWhereClauses.Add($"{compCodeProperty.Name} = @CompCode");
            }
            if (updDtPropertyDetail != null)
            {
                detailWhereClauses.Add($"({updDtPropertyDetail.Name} = @UpdDt OR {updDtPropertyDetail.Name} IS NULL)");
            }

            var deleteDetailSql = $@"
DELETE FROM {_tableName}
WHERE {string.Join(" AND ", detailWhereClauses)};
";

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Flag variable to track if rows were deleted
                        bool detailRowsDeleted = false;

                        var deleteDetailParams = new DynamicParameters();
                        foreach (var keyProp in detailKeyProperties)
                        {
                            var keyPropValue = keyProp.GetValue(detail)?.ToString();
                            if (!string.IsNullOrEmpty(keyPropValue))
                            {
                                deleteDetailParams.Add($"@{keyProp.Name}", keyPropValue);
                            }
                        }
                        deleteDetailParams.Add("@CompCode", companyCode);
                        if (updDtPropertyDetail != null)
                        {
                            var updDtValue = updDtPropertyDetail.GetValue(detail);
                            deleteDetailParams.Add("@UpdDt", updDtValue ?? DBNull.Value, dbType: DbType.Object);
                        }

                        // Delete the detail
                        var rowsAffected = await conn.ExecuteAsync(deleteDetailSql, deleteDetailParams, transaction);
                        if (rowsAffected == 1)
                        {
                            detailRowsDeleted = true;
                        }
                        else
                        {
                            throw new InvalidOperationException($"{_appSettings.SuccessStrings.DeleteNotFound}");
                        }

                        // Commit the transaction
                        transaction.Commit();

                        response.ValidationSuccess = detailRowsDeleted;
                        response.StatusCode = _appSettings.StatusCodes.Success;
                        response.SuccessString = _appSettings.SuccessStrings.DeleteSuccess;
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();
                        response.ValidationSuccess = false;
                        response.StatusCode = _appSettings.StatusCodes.Error;
                        response.ErrorString = ex.Message;
                    }
                }
            }

            return response;
        }

        public async Task<CommonResponse<T>> Update(T detail, string companyCode, string user)
        {
            var response = new CommonResponse<T>();
            var detailKeyProperties = GetPrimaryKeyProperties<T>();

            if (!detailKeyProperties.Any())
            {
                throw new InvalidOperationException($"{_appSettings.SuccessStrings.PrimaryKeyNotFound}");
            }

            var primaryKeyValues = detailKeyProperties.Select(p => p.GetValue(detail)?.ToString()).ToArray();

            if (primaryKeyValues.Any(string.IsNullOrEmpty))
            {
                throw new InvalidOperationException($"{_appSettings.SuccessStrings.PrimaryKeyNotFound}");
            }

            var updateDateDetailProperty = typeof(T).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_UPD_DT"));
            if (updateDateDetailProperty == null)
            {
                throw new InvalidOperationException($"{_appSettings.SuccessStrings.UpdateDateNotFound}");
            }

            // Get the company code property from the detail type
            var compCodeProperty = typeof(T).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase));

            // Get the current date and time
            var currentDateTime = GetCurrentDateTime();

            var updateDetailSql = $@"
UPDATE {_tableName}
SET {string.Join(",", GetColumnNames<T>().Where(c => !c.EndsWith("_CR_DT") && c != detailKeyProperties.FirstOrDefault().Name).Select(c => $"{c} = @{c}"))}   
{(updateDateDetailProperty != null ? $", {updateDateDetailProperty.Name} = '{currentDateTime}'" : "")} 
WHERE {string.Join(" AND ", detailKeyProperties.Select(p => $"{p.Name} = @{p.Name}"))}
{(compCodeProperty != null && companyCode != null ? $"AND {compCodeProperty.Name} = @CompCode" : "")}
{(updateDateDetailProperty != null ? $" AND ({updateDateDetailProperty.Name} IS NULL OR {updateDateDetailProperty.Name} = @{updateDateDetailProperty.Name})" : "")};
";


            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var parameters = new DynamicParameters(detail);
                        if (compCodeProperty != null && companyCode != null)
                        {
                            parameters.Add("@CompCode", companyCode);
                        }

                        var rowsAffected = await conn.ExecuteAsync(updateDetailSql, parameters, transaction);

                        if (rowsAffected != 1)
                        {
                            response.ValidationSuccess = false;
                            response.SuccessString = _appSettings.StatusCodes.NotFound;
                            response.ErrorString = _appSettings.SuccessStrings.UpdateNotFound;  //// add throw
                            response.ReturnCompleteRow = null;
                        }
                        else
                        {
                            // Retrieve the updated row
                            var getUpdatedRowSql = $@"
SELECT * FROM {_tableName}
WHERE {string.Join(" AND ", detailKeyProperties.Select(p => $"{p.Name} = @{p.Name}"))}
{(compCodeProperty != null ? $"AND {compCodeProperty.Name} = @CompCode" : "")}
";

                            var getUpdatedRowParams = new DynamicParameters();
                            foreach (var keyProp in detailKeyProperties)
                            {
                                getUpdatedRowParams.Add($"@{keyProp.Name}", keyProp.GetValue(detail));
                            }
                            if (compCodeProperty != null)
                            {
                                getUpdatedRowParams.Add("@CompCode", companyCode);
                            }

                            var updatedRow = await conn.QueryFirstOrDefaultAsync<T>(getUpdatedRowSql, getUpdatedRowParams, transaction);

                            response.ValidationSuccess = true;
                            response.SuccessString = _appSettings.StatusCodes.Success;
                            response.ReturnCompleteRow = updatedRow;
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.ValidationSuccess = false;
                        response.StatusCode = _appSettings.StatusCodes.Error;
                        response.ErrorString = ex.Message;
                    }
                }
            }

            return response;
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

                if (compCodeProperties != null && companyCode == null)
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
