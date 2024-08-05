using AutoMapper;
using AutoMapper.Internal;
using Azure;
using Dapper;
using DapperAPI.EntityModel;
using DapperAPI.Interface;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using static DapperAPI.EntityModel.CustomAttributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Collections;
using Microsoft.AspNetCore.Components;
using Azure.Core;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using DapperAPI.Setting;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;


namespace DapperAPI.Repository
{
    public class TwoRepository<T, TDetail> : ITwoRepository<T, TDetail> where T : class where TDetail : class
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;
        private readonly string _tableName;
        private readonly string _detailTableName;
        private readonly ILogger<T> _logger;

        private readonly AppSettings _appSettings;

        public TwoRepository(IDbConnectionProvider dbConnectionProvider, IOptions<AppSettings> appSettings)
        {
            _dbConnectionProvider = dbConnectionProvider;
            _tableName = typeof(T).Name;
            _detailTableName = typeof(TDetail).Name;
            _appSettings = appSettings.Value;


        }

        private string GetCurrentDateTime()
        {
            // Return the current date and time in a format compatible with both SQL Server and Oracle
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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

        
        private PropertyInfo[] GetPrimaryKeyProperties<T>()
        {
            //return typeof(T).GetProperties().Where(p => Attribute.IsDefined(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute))).ToArray();

            return typeof(T).GetProperties()
        .Where(prop => Attribute.IsDefined(prop, typeof(PrimaryKeyAttribute)) || Attribute.IsDefined(prop, typeof(KeyAttribute))).ToArray();
        }        

        private PropertyInfo[] GetForeignKeyProperties<T>()
        {
            return typeof(T).GetProperties().Where(p => Attribute.IsDefined(p, typeof(CustomAttributes.ForeignKeyAttribute))).ToArray();
        }

        private PropertyInfo GetSequenceKeyProperty<T>()
        {
            return typeof(T).GetProperties()
                .FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(SequenceKeyAttribute)));
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

        private PropertyInfo[] GetKeyPropertyName()
        {
            //return typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Any());
            return typeof(T).GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Any()).ToArray();
        }

        private void SetForeignKeyValues<TDetail>(TDetail detail, object header, PropertyInfo foreignKeyProperty)
        {
            var foreignKeyAttributes = foreignKeyProperty.GetCustomAttributes(typeof(CustomAttributes.ForeignKeyAttribute), false)
                .Cast<CustomAttributes.ForeignKeyAttribute>();

            foreach (var attr in foreignKeyAttributes)
            {
                var headerProperty = header.GetType().GetProperty(attr.ColumnName);
                if (headerProperty != null)
                {
                    var headerValue = headerProperty.GetValue(header);
                    foreignKeyProperty.SetValue(detail, Convert.ChangeType(headerValue, foreignKeyProperty.PropertyType));
                }
            }
        }

        private PropertyInfo[] GetAllProperties<T>()
        {
            return typeof(T).GetProperties().ToArray();
        }

        public async Task<IEnumerable<T>> GetAll(string companyCode, string user)
        {
            var primaryKeyProperty = GetPrimaryKeyPropertyName();
            var foreignKeyProperty = GetForeignKeyPropertyName();

            if (primaryKeyProperty == null || foreignKeyProperty == null)
            {
                throw new InvalidOperationException("Primary key or foreign key property not found.");
            }

            // //Find the Comp_Code property
            ////var getCompCodeProperty = typeof(T).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_Comp_Code"));
            ///

            var whereConditions = new List<string>();
            var parameters = new DynamicParameters();

            // Add dynamic conditions for properties ending with _COMP_CODE, excluding fm_comp_code and to_comp_code
            var modelProperties = typeof(T).GetProperties();
            foreach (var prop in modelProperties)
            {
                if (prop.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(prop.Name, "FM_COMP_CODE", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(prop.Name, "TO_COMP_CODE", StringComparison.OrdinalIgnoreCase))
                {
                    var columnName = prop.Name.ToUpper();
                    if (!string.IsNullOrEmpty(companyCode))
                    {
                        whereConditions.Add($"{columnName} = @{columnName}");
                        parameters.Add($"@{columnName}", companyCode);
                    }
                        
                }
            }

            ////// Add user condition
            ////whereConditions.Add("USER_ID = @UserId");
            ////parameters.Add("@UserId", user);

            // Construct the final SQL query
            var whereSql = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : string.Empty;

            var sql = $@"
                SELECT * FROM {_tableName} {whereSql};
                SELECT * FROM {_detailTableName};
            ";

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var multi = await conn.QueryMultipleAsync(sql, parameters))
                {
                    var headers = (await multi.ReadAsync<T>()).ToList();
                    var details = (await multi.ReadAsync<TDetail>()).ToList();

                    var headerDict = headers.ToDictionary(h => primaryKeyProperty.GetValue(h).ToString());

                    foreach (var detail in details)
                    {
                        var foreignKeyValue = foreignKeyProperty.GetValue(detail).ToString();
                        if (headerDict.TryGetValue(foreignKeyValue, out var header))
                        {
                            var detailListProperty = typeof(T).GetProperty(_tableName + "_" + _detailTableName);
                            var detailList = detailListProperty.GetValue(header) as IList<TDetail>;
                            detailList.Add(detail);
                        }
                    }

                    return headers;
                }
            }
        }

        public async Task<T> GetById(string id, string companyCode, string user)
        {
            var primaryKeyProperty = GetPrimaryKeyPropertyName();
            var foreignKeyProperty = GetForeignKeyPropertyName();

            if (primaryKeyProperty == null || foreignKeyProperty == null)
            {
                throw new InvalidOperationException("Primary key or foreign key property not found.");
            }

            var whereConditions = new List<string>();
            var parameters = new DynamicParameters();

            // Add primary key condition
            var primaryKeyColumnName = primaryKeyProperty.Name.ToUpper();
            whereConditions.Add($"{primaryKeyColumnName} = @{primaryKeyColumnName}");
            parameters.Add($"@{primaryKeyColumnName}", id);

            // Add _COMP_CODE condition for the main table
            var modelProperties = typeof(T).GetProperties();
            foreach (var prop in modelProperties)
            {
                if (prop.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(prop.Name, "FM_COMP_CODE", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(prop.Name, "TO_COMP_CODE", StringComparison.OrdinalIgnoreCase))
                {
                    var compCodeColumnName = prop.Name.ToUpper();
                    if (!string.IsNullOrEmpty(companyCode))
                    {
                        whereConditions.Add($"{compCodeColumnName} = @{compCodeColumnName}");
                        parameters.Add($"@{compCodeColumnName}", companyCode);
                    }
                }
            }

            // Construct the WHERE clause for the main table
            var whereSql = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : string.Empty;

            // Find the _COMP_CODE property for the detail table
            var detailCompCodeColumnName = typeof(TDetail).GetProperties()
                .FirstOrDefault(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase))?.Name.ToUpper();

            // Construct the SQL query for both main and detail tables
            var sql = $@"
        SELECT * FROM {_tableName} {whereSql};
        SELECT * FROM {_detailTableName} WHERE {foreignKeyProperty.Name} = @Id 
        {(detailCompCodeColumnName != null ? $"AND {detailCompCodeColumnName} = @Detail_COMP_CODE" : string.Empty)};
    ";

            parameters.Add("@Id", id);
            if (detailCompCodeColumnName != null)
            {
                parameters.Add("@Detail_COMP_CODE", companyCode);
            }

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var multi = await conn.QueryMultipleAsync(sql, parameters))
                {
                    var header = await multi.ReadFirstOrDefaultAsync<T>();
                    if (header == null)
                    {
                        return null;
                    }

                    var details = (await multi.ReadAsync<TDetail>()).ToList();
                    var detailListProperty = typeof(T).GetProperty(_tableName + "_" + _detailTableName);
                    var detailList = detailListProperty.GetValue(header) as IList<TDetail>;
                    foreach (var detail in details)
                    {
                        detailList.Add(detail);
                    }

                    return header;
                }
            }
        }

        public async Task<CommonResponse<T>> Insert(T obj, string companyCode, string user)
        {
            var response = new CommonResponse<T>();
            var primaryKeyProperty = GetPrimaryKeyPropertyName();
            var foreignKeyProperty = GetForeignKeyPropertyName();

            if (primaryKeyProperty == null || foreignKeyProperty == null)
            {
                response.ValidationSuccess = false;
                response.StatusCode = _appSettings.StatusCodes.Error;
                response.ErrorString = _appSettings.SuccessStrings.PrimaryKeyNotFound;
                return response;
            }

            // Get the primary key value from the object
            var primaryKeyValue = primaryKeyProperty.GetValue(obj)?.ToString();

            if (string.IsNullOrEmpty(primaryKeyValue))
            {
                response.ValidationSuccess = false;
                response.SuccessString = _appSettings.StatusCodes.Error;
                response.ErrorString = _appSettings.SuccessStrings.PrimaryKeyError;
                return response;
            }

            // Generate the INSERT SQL statement for the header
            var insertHeaderColumns = GetColumnNames<T>(true).ToList();
            var insertHeaderValues = insertHeaderColumns.Select(c => $"@{c}").ToList();

            // Replace _CR_DT columns with the current date and time
            var currentDateTime = GetCurrentDateTime();
            for (int i = 0; i < insertHeaderColumns.Count; i++)
            {
                if (insertHeaderColumns[i].EndsWith("_CR_DT"))
                {
                    insertHeaderValues[i] = $"'{currentDateTime}'";
                }
            }

            var insertHeaderSql = $@"
        INSERT INTO {_tableName} ({string.Join(",", insertHeaderColumns)})
        VALUES ({string.Join(",", insertHeaderValues)});
    ";

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert the header
                        await conn.ExecuteAsync(insertHeaderSql, obj, transaction);

                        // Generate the INSERT SQL statement for the details
                        var insertDetailColumns = GetColumnNames<TDetail>(true).ToList();
                        var insertDetailValues = insertDetailColumns.Select(c => $"@{c}").ToList();

                        // Replace _CR_DT columns with GETDATE()
                        for (int i = 0; i < insertDetailColumns.Count; i++)
                        {
                            if (insertDetailColumns[i].EndsWith("_CR_DT"))
                            {
                                insertDetailValues[i] = $"'{currentDateTime}'";
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

                        var NewData = await GetById(primaryKeyValue,companyCode,user);

                        response.ValidationSuccess = true;
                        response.StatusCode =_appSettings.StatusCodes.Success;
                        response.SuccessString = _appSettings.SuccessStrings.InsertSuccess;
                        response.ReturnCompleteRow = NewData;
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

        public async Task<CommonResponse<TDetail>> InsertDetail(TDetail detail, string companyCode, string user)
        {
            var response = new CommonResponse<TDetail>();
            var detailProperties = typeof(TDetail).GetProperties();
            var detailKeyProperties = GetPrimaryKeyProperties<TDetail>();
            var crDtPropertyDetail = detailProperties.FirstOrDefault(p => p.Name.EndsWith("_CR_DT", StringComparison.OrdinalIgnoreCase));
            var compCodeProperty = detailProperties.FirstOrDefault(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase));

            if (!detailKeyProperties.Any())
            {
                response.ValidationSuccess = false;
                response.StatusCode = _appSettings.StatusCodes.Error;
                response.ErrorString = _appSettings.SuccessStrings.PrimaryKeyNotFound;
                return response;
            }

            var primaryKeyValues = detailKeyProperties.Select(p => p.GetValue(detail)?.ToString()).ToArray();
            if (primaryKeyValues.Any(string.IsNullOrEmpty))
            {
                response.ValidationSuccess = false;
                response.SuccessString = _appSettings.StatusCodes.Error;
                response.ErrorString = _appSettings.SuccessStrings.PrimaryKeyError;
                return response;
            }

            if (compCodeProperty == null)
            {
                response.ValidationSuccess = false;
                response.SuccessString = "500";
                response.ErrorString = "Company code property not found in the detail type.";
                return response;
            }

            // Generate the INSERT clause for the detail table
            var detailColumns = detailProperties.Select(p => p.Name).ToList();
            var detailValues = detailProperties.Select(p => $"@{p.Name}").ToList();

            var insertDetailSql = $@"
INSERT INTO {_detailTableName} ({string.Join(", ", detailColumns)})
VALUES ({string.Join(", ", detailValues)});
";

            // Generate the SELECT query to retrieve the inserted row
            var getInsertedRowSql = $@"
SELECT * FROM {_detailTableName}
WHERE {string.Join(" AND ", detailKeyProperties.Select(p => $"{p.Name} = @{p.Name}"))}
AND {compCodeProperty.Name} = @CompCode
";

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var insertDetailParams = new DynamicParameters();
                        foreach (var prop in detailProperties)
                        {
                            var propValue = prop.GetValue(detail);
                            insertDetailParams.Add($"@{prop.Name}", propValue);
                        }
                        insertDetailParams.Add("@CompCode", companyCode);

                        // Insert the detail
                        var rowsAffected = await conn.ExecuteAsync(insertDetailSql, insertDetailParams, transaction);

                        if (rowsAffected > 0)
                        {
                            // Retrieve the inserted row
                            var insertedRow = await conn.QueryFirstOrDefaultAsync<TDetail>(getInsertedRowSql, insertDetailParams, transaction);

                            if (insertedRow != null)
                            {
                                response.ValidationSuccess = true;
                                response.StatusCode = _appSettings.StatusCodes.Success;
                                response.SuccessString= _appSettings.SuccessStrings.InsertSuccess;
                                response.ReturnCompleteRow = insertedRow;
                            }
                            else
                            {
                                response.ValidationSuccess = false;
                                response.StatusCode = _appSettings.StatusCodes.Error; // 204 No Content if nothing was found
                            }
                        }
                        else
                        {
                            response.ValidationSuccess = false;
                            response.StatusCode = _appSettings.StatusCodes.Error; // 204 No Content if nothing was inserted
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();
                        response.ValidationSuccess = false;
                        response.SuccessString = _appSettings.StatusCodes.Error;
                        response.ErrorString = ex.Message;
                    }
                }
            }

            return response;
        }

        public async Task<CommonResponse<TDetail>> InsertDetailBySeq(TDetail detail, string companyCode, string user)
        {
            var response = new CommonResponse<TDetail>();
            var detailKeyProperties = GetPrimaryKeyProperties<TDetail>().ToList();
            var sequenceKeyProperty = GetSequenceKeyProperty<TDetail>();
            var foreignKeyProperties = GetForeignKeyProperties<TDetail>().ToList();

            if (sequenceKeyProperty == null || !foreignKeyProperties.Any())
            {
                response.ValidationSuccess = false;
                response.SuccessString = "500";
                response.ErrorString = "Sequence key property or foreign key property not found.";
                return response;
            }

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Get the next sequence value for the detail using stored procedure
                        var detailSequenceName = $"SEQ_{_detailTableName.ToUpper()}";
                        var detailSeqSql = "EXEC FUNC_GET_SEQ_NO @SequenceName, @NEXTVALUE OUTPUT";
                        var detailSeqParams = new DynamicParameters();
                        detailSeqParams.Add("SequenceName", detailSequenceName);
                        detailSeqParams.Add("NEXTVALUE", dbType: DbType.Int32, direction: ParameterDirection.Output);

                        await conn.ExecuteAsync(detailSeqSql, detailSeqParams, transaction);
                        var detailPrimaryKeyValue = detailSeqParams.Get<int>("NEXTVALUE");

                        // Set the sequence key value for the detail
                        sequenceKeyProperty.SetValue(detail, Convert.ChangeType(detailPrimaryKeyValue, sequenceKeyProperty.PropertyType));

                        // Set the foreign key values based on attributes
                        foreach (var foreignKeyProperty in foreignKeyProperties)
                        {
                            SetForeignKeyValues(detail, detail, foreignKeyProperty);
                        }

                        // Generate the INSERT SQL statement for the details
                        var detailColumns = typeof(TDetail).GetProperties().Select(p => p.Name).ToList();
                        var detailValues = detailColumns.Select(c => $"@{c}").ToList();

                        var currentDateTime = GetCurrentDateTime();
                        for (int i = 0; i < detailColumns.Count; i++)
                        {
                            if (detailColumns[i].EndsWith("_CR_DT"))
                            {
                                detailValues[i] = $"'{currentDateTime}'";
                            }
                        }

                        var insertDetailSql = $@"
INSERT INTO {_detailTableName} ({string.Join(", ", detailColumns)})
VALUES ({string.Join(", ", detailValues)});
";

                        // Insert the detail
                        var insertDetailParams = new DynamicParameters();
                        foreach (var prop in detailColumns)
                        {
                            var propValue = typeof(TDetail).GetProperty(prop).GetValue(detail);
                            insertDetailParams.Add($"@{prop}", propValue);
                        }

                        var rowsAffected = await conn.ExecuteAsync(insertDetailSql, insertDetailParams, transaction);
                        if (rowsAffected > 0)
                        {
                            // Retrieve the inserted row
                            var whereConditions = detailKeyProperties.Select(p => $"{p.Name} = @{p.Name}").ToList();
                            var foreignKeyCompCodeConditions = foreignKeyProperties
                                .Where(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase))
                                .Select(p => $"{p.Name} = @CompCode");

                            whereConditions.AddRange(foreignKeyCompCodeConditions);

                            var getInsertedRowSql = $@"
SELECT * FROM {_detailTableName}
WHERE {string.Join(" AND ", whereConditions)}
";

                            var getInsertedRowParams = new DynamicParameters();
                            foreach (var keyProp in detailKeyProperties)
                            {
                                getInsertedRowParams.Add($"@{keyProp.Name}", keyProp.GetValue(detail));
                            }
                            getInsertedRowParams.Add("@CompCode", companyCode);

                            var insertedRow = await conn.QueryFirstOrDefaultAsync<TDetail>(getInsertedRowSql, getInsertedRowParams, transaction);

                            response.ValidationSuccess = true;
                            response.SuccessString = "200";
                            response.ReturnCompleteRow = insertedRow;
                        }
                        else
                        {
                            response.ValidationSuccess = false;
                            response.SuccessString = "204"; // 204 No Content if nothing was inserted
                        }

                        // Commit the transaction
                        transaction.Commit();
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

        public async Task<CommonResponse<T>> InsertBySeq(T obj, string companyCode, string user)
        {
            var response = new CommonResponse<T>();
            var primaryKeyProperties = GetPrimaryKeyProperties<T>().ToList();
            var sequenceKeyProperty = GetSequenceKeyProperty<T>();
            var foreignKeyProperties = GetForeignKeyProperties<TDetail>().ToList();

            if (sequenceKeyProperty == null || !foreignKeyProperties.Any())
            {
                response.ValidationSuccess = false;
                response.SuccessString = "500";
                response.ErrorString = "Sequence key property or foreign key property not found.";
                return response;
            }

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Get the next sequence value for the header using stored procedure
                        var headerSequenceName = $"SEQ_{_tableName.ToUpper()}";
                        var headerSeqSql = "EXEC FUNC_GET_SEQ_NO @SequenceName, @NEXTVALUE OUTPUT";
                        var headerSeqParams = new DynamicParameters();
                        headerSeqParams.Add("SequenceName", headerSequenceName);
                        headerSeqParams.Add("NEXTVALUE", dbType: DbType.Int32, direction: ParameterDirection.Output);

                        await conn.ExecuteAsync(headerSeqSql, headerSeqParams, transaction);
                        var headerPrimaryKeyValue = headerSeqParams.Get<int>("NEXTVALUE");

                        // Set the sequence key value for the header
                        sequenceKeyProperty.SetValue(obj, Convert.ChangeType(headerPrimaryKeyValue, sequenceKeyProperty.PropertyType));

                        // Generate the INSERT SQL statement for the header
                        var insertHeaderColumns = GetColumnNames<T>(true).ToList();
                        var insertHeaderValues = insertHeaderColumns.Select(c => $"@{c}").ToList();

                        // Replace _CR_DT columns with the current date and time

                        var currentDateTime = GetCurrentDateTime();
                        for (int i = 0; i < insertHeaderColumns.Count; i++)
                        {
                            if (insertHeaderColumns[i].EndsWith("_CR_DT"))
                            {
                                insertHeaderValues[i] = $"'{currentDateTime}'";
                            }
                        }

                        var insertHeaderSql = $@"
INSERT INTO {_tableName} ({string.Join(",", insertHeaderColumns)})
VALUES ({string.Join(",", insertHeaderValues)});
";

                        // Insert the header
                        await conn.ExecuteAsync(insertHeaderSql, obj, transaction);

                        // Get the detail list property and insert each detail entity
                        var detailListProperty = typeof(T).GetProperty(_tableName + "_" + _detailTableName);
                        var detailList = detailListProperty.GetValue(obj) as IList<TDetail>;

                        if (detailList != null)
                        {
                            foreach (var detail in detailList)
                            {
                                // Get the next sequence value for the detail using stored procedure
                                var detailSequenceName = $"SEQ_{_detailTableName.ToUpper()}";
                                var detailSeqSql = "EXEC FUNC_GET_SEQ_NO @SequenceName, @NEXTVALUE OUTPUT";
                                var detailSeqParams = new DynamicParameters();
                                detailSeqParams.Add("SequenceName", detailSequenceName);
                                detailSeqParams.Add("NEXTVALUE", dbType: DbType.Int32, direction: ParameterDirection.Output);

                                await conn.ExecuteAsync(detailSeqSql, detailSeqParams, transaction);
                                var detailPrimaryKeyValue = detailSeqParams.Get<int>("NEXTVALUE");

                                // Set the primary key value(s) for the detail
                                var detailSequenceKeyProperty = GetSequenceKeyProperty<TDetail>();
                                if (detailSequenceKeyProperty != null)
                                {
                                    detailSequenceKeyProperty.SetValue(detail, Convert.ChangeType(detailPrimaryKeyValue, detailSequenceKeyProperty.PropertyType));
                                }

                                // Set the foreign key values based on attributes
                                foreach (var foreignKeyProperty in foreignKeyProperties)
                                {
                                    SetForeignKeyValues(detail, obj, foreignKeyProperty);
                                }

                                // Generate the INSERT SQL statement for the details
                                var insertDetailColumns = GetColumnNames<TDetail>(true).ToList();
                                var insertDetailValues = insertDetailColumns.Select(c => $"@{c}").ToList();

                                // Replace _CR_DT columns with the current date and time
                                for (int i = 0; i < insertDetailColumns.Count; i++)
                                {
                                    if (insertDetailColumns[i].EndsWith("_CR_DT"))
                                    {
                                        insertDetailValues[i] = $"'{currentDateTime}'";
                                    }
                                }

                                var insertDetailSql = $@"
INSERT INTO {_detailTableName} ({string.Join(",", insertDetailColumns)})
VALUES ({string.Join(",", insertDetailValues)});
";

                                await conn.ExecuteAsync(insertDetailSql, detail, transaction);
                            }
                        }

                        // Commit the transaction
                        transaction.Commit();

                        var newData = await GetById(headerPrimaryKeyValue.ToString(), companyCode, user);

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
                        response.ErrorString = ex.ToString();
                    }
                }
            }

            return response;
        }

        
        public async Task<CommonResponse<T>> Update(T obj, string companyCode, string user)
        {
            var response = new CommonResponse<T>();
            var primaryKey = GetPrimaryKeyPropertyName();
            var primaryKeyProperties = GetPrimaryKeyProperties<T>();
            var foreignKeyProperties = GetForeignKeyProperties<TDetail>();
            var detailKeyProperties = GetPrimaryKeyProperties<TDetail>();

            if (!primaryKeyProperties.Any() || !foreignKeyProperties.Any())
            {
                response.ValidationSuccess = false;
                response.SuccessString = "500";
                response.ErrorString = "Primary key or foreign key properties not found. For Update";
                return response;
            }

            var primaryKeyValues = primaryKeyProperties.Select(p => p.GetValue(obj)?.ToString()).ToArray();
            var primaryKeyValue = primaryKey.GetValue(obj)?.ToString();

            if (primaryKeyValues.Any(string.IsNullOrEmpty))
            {
                response.ValidationSuccess = false;
                response.SuccessString = "500";
                response.ErrorString = "Primary key values are required. For Update";
                return response;
            }

            // Find the UPD_DT property
            var updateDateProperty = typeof(T).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_UPD_DT"));

            if (updateDateProperty == null)
            {
                response.ValidationSuccess = false;
                response.SuccessString = "500";
                response.ErrorString = "Updated date column not found. For Update";
                return response;
            }

            var updateDateDetailProperty = typeof(TDetail).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_UPD_DT"));

            // Get the current date and time
            var currentDateTime = GetCurrentDateTime();

            // Exclude the primary key column from the UPDATE statement
            var updateHeaderSql = $@"
    UPDATE {_tableName}
    SET {string.Join(",", GetColumnNames<T>().Where(c => !c.EndsWith("_CR_DT") && c != primaryKey.Name).Select(c => $"{c} = @{c}"))}
    {(updateDateProperty != null ? $", {updateDateProperty.Name} = {currentDateTime}" : "")}
    WHERE {string.Join(" AND ", primaryKeyProperties.Select(p => $"{p.Name} = @{p.Name}"))}
    {(updateDateProperty != null ? $" AND ({updateDateProperty.Name} = @{updateDateProperty.Name} OR {updateDateProperty.Name} IS NULL)" : "")};
";

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Update the header
                        var cnt = await conn.ExecuteAsync(updateHeaderSql, obj, transaction);

                        // Generate the SELECT SQL statement to get existing details
                        var selectDetailSql = $@"
                SELECT * FROM {_detailTableName}
                WHERE {string.Join(" AND ", foreignKeyProperties.Select(p => $"{p.Name} = @{p.Name}"))};
            ";

                        // Create an anonymous type with the correct foreign key names and values
                        var selectParams = new DynamicParameters();
                        for (int i = 0; i < foreignKeyProperties.Length; i++)
                        {
                            selectParams.Add(foreignKeyProperties[i].Name, primaryKeyValues.ElementAtOrDefault(i));
                        }

                        var existingDetails = (await conn.QueryAsync<TDetail>(selectDetailSql, selectParams, transaction)).ToList();

                        // Get the detail list property and process each detail entity
                        var detailListProperty = typeof(T).GetProperty(_tableName + "_" + _detailTableName);
                        var detailList = detailListProperty.GetValue(obj) as IList<TDetail>;

                        foreach (var detail in detailList)
                        {
                            // Set the foreign key values to match the primary key of the header
                            for (int i = 0; i < foreignKeyProperties.Length; i++)
                            {
                                foreignKeyProperties[i].SetValue(detail, primaryKeyValues.ElementAtOrDefault(i));
                            }

                            var existingDetail = existingDetails.FirstOrDefault(d =>
                            {
                                var isForeignKeyMatch = foreignKeyProperties.All(fk =>
                                {
                                    var detailProperty = typeof(TDetail).GetProperty(fk.Name);
                                    var detailValue = detailProperty?.GetValue(detail);
                                    var existingValue = detailProperty?.GetValue(d);
                                    return detailValue != null && detailValue.Equals(existingValue);
                                });

                                var isDetailKeyMatch = detailKeyProperties.All(dk =>
                                {
                                    var detailProperty = typeof(TDetail).GetProperty(dk.Name);
                                    var detailValue = detailProperty?.GetValue(detail);
                                    var existingValue = detailProperty?.GetValue(d);
                                    return detailValue != null && detailValue.Equals(existingValue);
                                });

                                return isForeignKeyMatch && isDetailKeyMatch;
                            });

                            if (existingDetail != null)
                            {
                                // Exclude the primary key column from the detail update statement
                                var updateDetailSql = $@"
                        UPDATE {_detailTableName}
                        SET {string.Join(",", GetColumnNames<TDetail>().Where(c => c != detailKeyProperties.FirstOrDefault()?.Name).Select(c => $"{c} = @{c}"))}
                        {(updateDateDetailProperty != null ? $", {updateDateDetailProperty.Name} = {currentDateTime}" : "")}
                        WHERE {string.Join(" AND ", detailKeyProperties.Select(p => $"{p.Name} = @{p.Name}"))}
                        {(updateDateDetailProperty != null ? $" AND ({updateDateDetailProperty.Name} = @{updateDateDetailProperty.Name} OR {updateDateDetailProperty.Name} IS NULL)" : "")};
                    ";

                                await conn.ExecuteAsync(updateDetailSql, detail, transaction);
                            }
                            else
                            {
                                // Generate the INSERT SQL statement for the new detail
                                var insertDetailSql = $@"
                        INSERT INTO {_detailTableName} ({string.Join(",", GetColumnNames<TDetail>(true))})
                        VALUES ({string.Join(",", GetColumnNames<TDetail>(true).Select(c => $"@{c}"))});
                    ";

                                await conn.ExecuteAsync(insertDetailSql, detail, transaction);
                            }
                        }

                        // Commit the transaction
                        transaction.Commit();

                        if (cnt > 0)
                        {
                            var newData = await GetById(primaryKeyValue, companyCode, user);

                            response.ValidationSuccess = true;
                            response.SuccessString = "200";
                            response.ReturnCompleteRow = newData;
                        }
                        else
                        {
                            response.ValidationSuccess = true;
                            response.SuccessString = "204";
                            response.ErrorString = "No rows updated";
                            response.ReturnCompleteRow = obj;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        transaction.Rollback();
                        response.ValidationSuccess = false;
                        response.SuccessString = "500";
                        response.ErrorString = ex.Message;
                        return response;
                    }
                }
            }
            return response;
        }

        public async Task<CommonResponse<TDetail>> UpdateDetail(TDetail detail, string companyCode, string user)
        {
            var response = new CommonResponse<TDetail>();

            // Retrieve values from appsettings.json
            

            var detailKeyProperties = GetPrimaryKeyProperties<TDetail>();

            if (!detailKeyProperties.Any())
            {
                
                throw new InvalidOperationException($"{_appSettings.SuccessStrings.PrimaryKeyNotFound}");
            }

            var primaryKeyValues = detailKeyProperties.Select(p => p.GetValue(detail)?.ToString()).ToArray();

            if (primaryKeyValues.Any(string.IsNullOrEmpty))
            {
                throw new InvalidOperationException($"{_appSettings.SuccessStrings.PrimaryKeyError}");
            }

            var updateDateDetailProperty = typeof(TDetail).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_UPD_DT"));
            if (updateDateDetailProperty == null)
            {
                throw new InvalidOperationException($"{_appSettings.SuccessStrings.UpdateDateNotFound}");
            }

            // Get the company code property from the detail type
            var compCodeProperty = typeof(TDetail).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase));
            

            // Get the current date and time
            var currentDateTime = GetCurrentDateTime();

            var updateDetailSql = $@"
UPDATE {_detailTableName}
SET {string.Join(",", GetColumnNames<TDetail>().Where(c => !c.EndsWith("_CR_DT") && c != detailKeyProperties.FirstOrDefault().Name).Select(c => $"{c} = @{c}"))}   
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

                        if (rowsAffected == 0)
                        {
                           throw new InvalidOperationException($"{_appSettings.SuccessStrings.UpdateNotFound}");
                        }
                        else
                        {
                            // Retrieve the updated row
                            var getUpdatedRowSql = $@"
SELECT * FROM {_detailTableName}
WHERE {string.Join(" AND ", detailKeyProperties.Select(p => $"{p.Name} = @{p.Name}"))}
{(compCodeProperty != null && companyCode != null ? $"AND {compCodeProperty.Name} = @CompCode" : "")}
";

                            var getUpdatedRowParams = new DynamicParameters();
                            foreach (var keyProp in detailKeyProperties)
                            {
                                getUpdatedRowParams.Add($"@{keyProp.Name}", keyProp.GetValue(detail));
                            }
                            if (compCodeProperty != null && companyCode != null)
                            {
                                getUpdatedRowParams.Add("@CompCode", companyCode);
                            }

                            var updatedRow = await conn.QueryFirstOrDefaultAsync<TDetail>(getUpdatedRowSql, getUpdatedRowParams, transaction);

                            response.ValidationSuccess = true;
                            response.StatusCode = _appSettings.StatusCodes.Success;
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

        public async Task<CommonResponse<TDetail>> UpdateDetailByIdentity_old(TDetail detail, string companyCode, string user)
        {
            var response = new CommonResponse<TDetail>();
            var detailKeyProperties = GetPrimaryKeyProperties<TDetail>();

            if (!detailKeyProperties.Any())
            {
                response.ValidationSuccess = false;
                response.SuccessString = "500";
                response.ErrorString = "Primary key not found for Detail Update.";
                return response;
            }

            var primaryKeyValues = detailKeyProperties.Select(p => p.GetValue(detail)?.ToString()).ToArray();

            if (primaryKeyValues.Any(string.IsNullOrEmpty))
            {
                response.ValidationSuccess = false;
                response.SuccessString = "500";
                response.ErrorString = "Primary key value is required for Detail Update.";
                return response;
            }

            var updateDateDetailProperty = typeof(TDetail).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_UPD_DT"));
            if (updateDateDetailProperty == null)
            {
                response.ValidationSuccess = false;
                response.SuccessString = "500";
                response.ErrorString = "Updated date field not found.";
                return response;
            }

            // Get the company code property from the detail type
            var compCodeProperty = typeof(TDetail).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase));
            if (compCodeProperty == null)
            {
                response.ValidationSuccess = false;
                response.SuccessString = "500";
                response.ErrorString = "Company code property not found in the detail type.";
                return response;
            }

            // Get the current date and time
            var currentDateTime = GetCurrentDateTime();

            var updateDetailSql = $@"
UPDATE {_detailTableName}
SET {string.Join(",", GetColumnNames<TDetail>().Where(c => !c.EndsWith("_CR_DT") && c != detailKeyProperties.FirstOrDefault().Name).Select(c => $"{c} = @{c}"))}   
{(updateDateDetailProperty != null ? $", {updateDateDetailProperty.Name} ={currentDateTime}" : "")} 
WHERE {string.Join(" AND ", detailKeyProperties.Select(p => $"{p.Name} = @{p.Name}"))}
AND {compCodeProperty.Name} = @CompCode
{(updateDateDetailProperty != null ? $" AND ({updateDateDetailProperty.Name} IS NULL OR {updateDateDetailProperty.Name} = @{updateDateDetailProperty.Name})" : "")};
";

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

            // Replace _CR_DT columns with the current date and time

            for (int i = 0; i < insertDetailColumns.Count; i++)
            {
                if (insertDetailColumns[i].EndsWith("_CR_DT"))
                {
                    insertDetailValues[i] = $"'{currentDateTime}'";
                }
            }

            ////            var insertDetailSql = $@"
            ////INSERT INTO {_detailTableName} ({string.Join(",", insertDetailColumns)})
            ////VALUES ({string.Join(",", insertDetailValues)});
            ////";

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var parameters = new DynamicParameters(detail);
                        parameters.Add("@CompCode", companyCode);

                        var rowsAffected = await conn.ExecuteAsync(updateDetailSql, parameters, transaction);

                        ////if (rowsAffected == 0)
                        ////{
                        ////    await conn.ExecuteAsync(insertDetailSql, parameters, transaction);
                        ////}

                        transaction.Commit();

                        response.ValidationSuccess = true;
                        response.SuccessString = "200";
                        response.ReturnCompleteRow = detail;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.ValidationSuccess = false;
                        response.SuccessString = "500";
                        response.ErrorString = ex.Message;
                    }
                }
            }

            return response;
        }

        public async Task<CommonResponse<TDetail>> UpdateDetailByIdentity(TDetail detail, string companyCode, string user)
        {
            var response = new CommonResponse<TDetail>();
            var detailKeyProperties = GetPrimaryKeyProperties<TDetail>();

            if (!detailKeyProperties.Any())
            {
                throw new InvalidOperationException($"{_appSettings.SuccessStrings.PrimaryKeyNotFound}");
            }

            var primaryKeyValues = detailKeyProperties.Select(p => p.GetValue(detail)?.ToString()).ToArray();

            if (primaryKeyValues.Any(string.IsNullOrEmpty))
            {
                throw new InvalidOperationException($"{_appSettings.SuccessStrings.PrimaryKeyNotFound}");
            }

            var updateDateDetailProperty = typeof(TDetail).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_UPD_DT"));
            if (updateDateDetailProperty == null)
            {
                throw new InvalidOperationException($"{_appSettings.SuccessStrings.UpdateDateNotFound}");
            }

            // Get the company code property from the detail type
            var compCodeProperty = typeof(TDetail).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase));

            // Get the current date and time
            var currentDateTime = GetCurrentDateTime();

            var updateDetailSql = $@"
UPDATE {_detailTableName}
SET {string.Join(",", GetColumnNames<TDetail>().Where(c => !c.EndsWith("_CR_DT") && c != detailKeyProperties.FirstOrDefault().Name).Select(c => $"{c} = @{c}"))}   
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
                            response.SuccessString = "404";
                            response.ErrorString = "No Detail Found For Update";  //// add throw
                            response.ReturnCompleteRow = null;
                        }
                        else
                        {
                            // Retrieve the updated row
                            var getUpdatedRowSql = $@"
SELECT * FROM {_detailTableName}
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

                            var updatedRow = await conn.QueryFirstOrDefaultAsync<TDetail>(getUpdatedRowSql, getUpdatedRowParams, transaction);

                            response.ValidationSuccess = true;
                            response.SuccessString = "200";
                            response.ReturnCompleteRow = updatedRow;
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.ValidationSuccess = false;
                        response.SuccessString = "500";
                        response.ErrorString = ex.Message;
                    }
                }
            }

            return response;
        }

        public async Task<CommonResponse<T>> Delete_old(T obj, string companyCode, string user)
        {
            var response = new CommonResponse<T>();

            var primaryKeyProperties = GetKeyPropertyName();
            if (primaryKeyProperties == null || !primaryKeyProperties.Any())
            {
                throw new InvalidOperationException("Primary key properties not found.");
            }

            // Get primary key values
            var primaryKeyValues = new Dictionary<string, object>();
            foreach (var prop in primaryKeyProperties)
            {
                var value = prop.GetValue(obj);
                if (value == null)
                {
                    throw new InvalidOperationException($"Primary key property '{prop.Name}' value is null.");
                }
                primaryKeyValues.Add(prop.Name, value);
            }

            

            // Get the detail property dynamically
            var detailProperty = typeof(T).GetProperties().FirstOrDefault(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>));
            var detailType = detailProperty?.PropertyType.GetGenericArguments().FirstOrDefault();

            // Get key properties and company code property from the detail type
            var keyProperties = detailType?.GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Any()).ToList();
            var compCodePropertyDetail = detailType?.GetProperties().FirstOrDefault(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase));
            var updDtPropertyDetail = detailType?.GetProperties().FirstOrDefault(p => p.Name.EndsWith("_UPD_DT", StringComparison.OrdinalIgnoreCase));

            // Get the company code and update date property from the header type
            var headerType = typeof(T);
            var compCodePropertyHeader = headerType.GetProperties().FirstOrDefault(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase));
            var updDtPropertyHeader = headerType.GetProperties().FirstOrDefault(p => p.Name.EndsWith("_UPD_DT", StringComparison.OrdinalIgnoreCase));

            if (compCodePropertyHeader == null)
            {
                throw new InvalidOperationException("Company code property not found in the header type.");
            }

            // Generate the WHERE clause for the detail table
            var detailWhereClauses = keyProperties
                .Select(p => $"{p.Name} = @{p.Name}")
                .ToList();         
                detailWhereClauses.Add($"{compCodePropertyDetail?.Name} = @CompCode");  
            if (updDtPropertyDetail != null)
            {
                detailWhereClauses.Add($"({updDtPropertyDetail.Name} = @UpdDt OR {updDtPropertyDetail.Name} IS NULL)");
            }

            var deleteDetailSql = $@"
DELETE FROM {_detailTableName}
WHERE {string.Join(" AND ", detailWhereClauses)};
";

            // Generate the WHERE clause for the header table
            var headerWhereClauses = primaryKeyProperties
                .Select(p => $"{p.Name} = @{p.Name}")
                .ToList();            
                headerWhereClauses.Add($"{compCodePropertyHeader.Name} = @CompCode");            
            if (updDtPropertyHeader != null)
            {
                headerWhereClauses.Add($"({updDtPropertyHeader.Name} = @UpdDt OR {updDtPropertyHeader.Name} IS NULL)");
            }

            var deleteHeaderSql = $@"
DELETE FROM {_tableName}
WHERE {string.Join(" AND ", headerWhereClauses)};
";

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Flag variables to track if rows were deleted
                        bool detailRowsDeleted = false;
                        bool headerRowsDeleted = false;

                        // Create a dictionary for the detail deletion parameters
                        var detailList = detailProperty?.GetValue(obj) as IList;
                        if (detailList != null && detailList.Count > 0)
                        {
                            foreach (var detailObj in detailList)
                            {
                                var deleteDetailParams = new DynamicParameters();
                                foreach (var keyProp in keyProperties)
                                {
                                    var keyPropValue = keyProp.GetValue(detailObj)?.ToString();
                                    if (!string.IsNullOrEmpty(keyPropValue))
                                    {
                                        deleteDetailParams.Add($"@{keyProp.Name}", keyPropValue);
                                    }
                                }
                                deleteDetailParams.Add("@CompCode", companyCode);
                                if (updDtPropertyDetail != null)
                                {
                                    var updDtValue = updDtPropertyDetail.GetValue(detailObj);
                                    deleteDetailParams.Add("@UpdDt", updDtValue ?? DBNull.Value, dbType: DbType.Object);
                                }

                                // Delete the details
                                var rowsAffected = await conn.ExecuteAsync(deleteDetailSql, deleteDetailParams, transaction);
                                if (rowsAffected > 0)
                                {
                                    detailRowsDeleted = true;
                                }
                            }
                        }

                        // Create a dictionary for the header deletion parameters
                        var deleteHeaderParams = new DynamicParameters();
                        foreach (var pk in primaryKeyValues)
                        {
                            deleteHeaderParams.Add($"@{pk.Key}", pk.Value);
                        }
                        deleteHeaderParams.Add("@CompCode", companyCode);
                        if (updDtPropertyHeader != null)
                        {
                            var updDtValue = updDtPropertyHeader.GetValue(obj);
                            deleteHeaderParams.Add("@UpdDt", updDtValue ?? DBNull.Value, dbType: DbType.Object);
                        }

                        // Delete the header
                        var headerRowsAffected = await conn.ExecuteAsync(deleteHeaderSql, deleteHeaderParams, transaction);
                        if (headerRowsAffected > 0)
                        {
                            headerRowsDeleted = true;
                        }

                        // Commit the transaction
                        transaction.Commit();

                        response.ValidationSuccess = detailRowsDeleted || headerRowsDeleted;
                        response.SuccessString = response.ValidationSuccess ? "200" : "204"; // 204 No Content if nothing was deleted
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

        public async Task<CommonResponse<T>> Delete(T obj, string companyCode, string user)
        {
            var response = new CommonResponse<T>();

            var primaryKeyProperties = GetKeyPropertyName();
            if (primaryKeyProperties == null || !primaryKeyProperties.Any())
            {
                throw new InvalidOperationException("Primary key properties not found.");
            }

            // Get primary key values
            var primaryKeyValues = new Dictionary<string, object>();
            foreach (var prop in primaryKeyProperties)
            {
                var value = prop.GetValue(obj);
                if (value == null)
                {
                   
                    throw new InvalidOperationException($"{_appSettings.SuccessStrings.PrimaryKeyNotFound} '{prop.Name}'");
                }
                primaryKeyValues.Add(prop.Name, value);
            }

            // Get the detail property dynamically
            var detailProperty = typeof(T).GetProperties().FirstOrDefault(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>));
            var detailType = detailProperty?.PropertyType.GetGenericArguments().FirstOrDefault();

            // Get key properties and company code property from the detail type
            var keyProperties = detailType?.GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Any()).ToList();
            var compCodePropertyDetail = detailType?.GetProperties().FirstOrDefault(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase));
            var updDtPropertyDetail = detailType?.GetProperties().FirstOrDefault(p => p.Name.EndsWith("_UPD_DT", StringComparison.OrdinalIgnoreCase));

            // Get the company code and update date property from the header type
            var headerType = typeof(T);
            var compCodePropertyHeader = headerType.GetProperties().FirstOrDefault(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase));
            var updDtPropertyHeader = headerType.GetProperties().FirstOrDefault(p => p.Name.EndsWith("_UPD_DT", StringComparison.OrdinalIgnoreCase));

            if (compCodePropertyHeader == null)
            {
                throw new InvalidOperationException($"{_appSettings.SuccessStrings.UpdateNotFound}");
            }

            // Generate the WHERE clause for the detail table
            var detailWhereClauses = keyProperties
                .Select(p => $"{p.Name} = @{p.Name}")
                .ToList();
            if (compCodePropertyDetail != null && companyCode != null)
            {
                detailWhereClauses.Add($"{compCodePropertyDetail?.Name} = @CompCode");
            }
            if (updDtPropertyDetail != null)
            {
                detailWhereClauses.Add($"({updDtPropertyDetail.Name} = @UpdDt OR {updDtPropertyDetail.Name} IS NULL)");
            }

            var deleteDetailSql = $@"
DELETE FROM {_detailTableName}
WHERE {string.Join(" AND ", detailWhereClauses)};
";

            // Generate the WHERE clause for the header table
            var headerWhereClauses = primaryKeyProperties
                .Select(p => $"{p.Name} = @{p.Name}")
                .ToList();
            if (companyCode != null)
            {
                headerWhereClauses.Add($"{compCodePropertyHeader.Name} = @CompCode");
            }
            if (updDtPropertyHeader != null)
            {
                headerWhereClauses.Add($"({updDtPropertyHeader.Name} = @UpdDt OR {updDtPropertyHeader.Name} IS NULL)");
            }

            var deleteHeaderSql = $@"
DELETE FROM {_tableName}
WHERE {string.Join(" AND ", headerWhereClauses)};
";

            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Flag variables to track if rows were deleted
                        bool detailRowsDeleted = false;
                        bool headerRowsDeleted = false;

                        // Create a dictionary for the detail deletion parameters
                        var detailList = detailProperty?.GetValue(obj) as IList;
                        if (detailList != null && detailList.Count > 0)
                        {
                            foreach (var detailObj in detailList)
                            {
                                var deleteDetailParams = new DynamicParameters();
                                foreach (var keyProp in keyProperties)
                                {
                                    var keyPropValue = keyProp.GetValue(detailObj)?.ToString();
                                    if (!string.IsNullOrEmpty(keyPropValue))
                                    {
                                        deleteDetailParams.Add($"@{keyProp.Name}", keyPropValue);
                                    }
                                }
                                deleteDetailParams.Add("@CompCode", companyCode);
                                if (updDtPropertyDetail != null)
                                {
                                    var updDtValue = updDtPropertyDetail.GetValue(detailObj);
                                    deleteDetailParams.Add("@UpdDt", updDtValue ?? DBNull.Value, dbType: DbType.Object);
                                }

                                // Delete the details
                                var rowsAffected = await conn.ExecuteAsync(deleteDetailSql, deleteDetailParams, transaction);
                                if (rowsAffected == 1)
                                {
                                    detailRowsDeleted = true;
                                }
                                else
                                {
                                    throw new InvalidOperationException("204 No Content if nothing was deleted");
                                }
                            }
                        }

                        // Create a dictionary for the header deletion parameters
                        var deleteHeaderParams = new DynamicParameters();
                        foreach (var pk in primaryKeyValues)
                        {
                            deleteHeaderParams.Add($"@{pk.Key}", pk.Value);
                        }
                        deleteHeaderParams.Add("@CompCode", companyCode);
                        if (updDtPropertyHeader != null)
                        {
                            var updDtValue = updDtPropertyHeader.GetValue(obj);
                            deleteHeaderParams.Add("@UpdDt", updDtValue ?? DBNull.Value, dbType: DbType.Object);
                        }

                        // Delete the header
                        var headerRowsAffected = await conn.ExecuteAsync(deleteHeaderSql, deleteHeaderParams, transaction);
                        if (headerRowsAffected == 1)
                        {
                            headerRowsDeleted = true;
                        }

                        // Commit the transaction
                        transaction.Commit();

                        response.ValidationSuccess = detailRowsDeleted || headerRowsDeleted;
                        response.StatusCode = "200";
                        response.SuccessString = "Header And Details Deleted Succesfully"; 
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

        public async Task<CommonResponse<TDetail>> DeleteDetail(TDetail detail, string companyCode, string user)
        {
            var response = new CommonResponse<TDetail>();
            var detailKeyProperties = GetPrimaryKeyProperties<TDetail>();
            var updDtPropertyDetail = typeof(TDetail).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_UPD_DT", StringComparison.OrdinalIgnoreCase));
            var compCodeProperty = typeof(TDetail).GetProperties().FirstOrDefault(p => p.Name.EndsWith("_COMP_CODE", StringComparison.OrdinalIgnoreCase));

            if (!detailKeyProperties.Any())
            {
                throw new InvalidOperationException("Primary key properties not found.");
            }

            var primaryKeyValues = detailKeyProperties.Select(p => p.GetValue(detail)?.ToString()).ToArray();
            if (primaryKeyValues.Any(string.IsNullOrEmpty))
            {
                throw new InvalidOperationException("Primary key value is required for Detail.");
                
            }

            //if (compCodeProperty == null)
            //{
            //    response.ValidationSuccess = false;
            //    response.SuccessString = "500";
            //    response.ErrorString = "Company code property not found in the detail type.";
            //    return response;
            //}

            // Generate the WHERE clause for the detail table
            var detailWhereClauses = detailKeyProperties
                .Select(p => $"{p.Name} = @{p.Name}")
                .ToList();
            if(compCodeProperty != null && companyCode != null) 
            {
                detailWhereClauses.Add($"{compCodeProperty.Name} = @CompCode");
            }            
            if (updDtPropertyDetail != null)
            {
                detailWhereClauses.Add($"({updDtPropertyDetail.Name} = @UpdDt OR {updDtPropertyDetail.Name} IS NULL)");
            }

            var deleteDetailSql = $@"
DELETE FROM {_detailTableName}
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
                            throw new InvalidOperationException("204 No Content if nothing was deleted");
                        }

                        // Commit the transaction
                        transaction.Commit();

                        response.ValidationSuccess = detailRowsDeleted;
                        response.StatusCode = "200";
                        response.SuccessString = "Row Deleted Successfully";
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

        


        public async Task<CommonResponse<object>> Import(List<T> obj, string companyCode, string user, string result)
        {
            CommonResponse<object> response = new CommonResponse<object>();
            var primaryKeyProperty = GetPrimaryKeyPropertyName();
            var foreignKeyProperty = GetForeignKeyPropertyName();
            ExcelImport errObj = new ExcelImport();

            if (primaryKeyProperty == null || foreignKeyProperty == null)
            {
                response.ValidationSuccess = false;
                response.StatusCode = "400";
                response.ErrorString = "Primary key or foreign key property not found.";
                return response;
            }

            // Get the primary key value from the object
            var primaryKeyValue = primaryKeyProperty.GetValue(obj[0])?.ToString();

            if (string.IsNullOrEmpty(primaryKeyValue))
            {
                response.ValidationSuccess = false;
                response.StatusCode = "400";
                response.ErrorString = "Primary key value is required.";
                return response;
            }

            // Generate the INSERT SQL statement for the header
            var insertHeaderColumns = GetColumnNames<T>(true).ToList();
            var insertHeaderValues = insertHeaderColumns.Select(c => $"@{c}").ToList();

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
        VALUES ({string.Join(",", insertHeaderValues)}); ";





            if (result == "N")
            {
                using (var conn = _dbConnectionProvider.CreateConnection())
                {

                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var item in obj)
                            {
                                response.ReturnCompleteRow = await conn.ExecuteAsync(insertHeaderSql, item, tran);
                            }
                            tran.Commit();

                        }
                        catch (Exception ex)
                        {

                            tran.Rollback();
                            response.ValidationSuccess = false;
                            response.StatusCode = "400";
                            response.ErrorString = ex.Message;
                        }


                        finally
                        {
                            conn.Close();

                        }
                    }
                }
            }
            else if (result == "Y")
            {

                int count = 1;
                int insertedcount = 0;
                int errorcount = 0;
                string error = "nothing";
                List<ImportErr> errorList = new List<ImportErr>();
                errObj.Total_rows = obj.Count;


                foreach (var item in obj)
                {
                    using (var conn = _dbConnectionProvider.CreateConnection())
                    {

                        try
                        {
                            using (var tran = conn.BeginTransaction())
                            {
                                response.ReturnCompleteRow = await conn.ExecuteAsync(insertHeaderSql, item, tran);
                                tran.Commit();
                                insertedcount++;
                            }



                        }
                        catch (Exception ex)
                        {
                            error = "Yes";
                            ImportErr err = new ImportErr { Error_rownum = count, ErrorMessage = ex.Message };
                            response.ValidationSuccess = false;
                            response.StatusCode = "400";
                            errorList.Add(err);
                            errorcount++;

                        }

                        count++;

                    }

                }
                errObj.errRaw = errorList;
                errObj.Error_rows = errorcount;
                errObj.Upload_rows = insertedcount;

                if (error == "Yes") response.ReturnCompleteRow = errObj;
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



    }
}
