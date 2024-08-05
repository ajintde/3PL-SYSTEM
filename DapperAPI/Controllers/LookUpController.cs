using Dapper;
using DapperAPI.EntityModel;
using DapperAPI.Interface;
using DapperAPI.Services;
using Microsoft.AspNetCore.Cors;

//using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Reflection.Emit;
using static Dapper.SqlMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DapperAPI.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    
    public class LookUpController : ControllerBase
    {


        private readonly IDbConnectionProvider _dbConnectionProvider;
        public LookUpController(IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;

        }


        [HttpPost]
        public async Task<CommonResponse<object>> CompanyLookup(CompanyLookup compSearch)
        {

            var sql = "";
            string whereClause = " ";

            if (compSearch.COMP_CODE != null) whereClause = whereClause + "COMP_CODE LIKE '" + compSearch.COMP_CODE + "'";
            if (compSearch.COMP_NAME != null) whereClause = whereClause + "COMP_NAME LIKE '" + compSearch.COMP_NAME + "'";
            if (compSearch.strAdd != null) whereClause = whereClause + " AND " + compSearch.strAdd;
            if (compSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + compSearch.sortby.ToString();
            }
            else
            {
                if (compSearch.COMP_CODE != null)  whereClause = whereClause + " ORDER BY  COMP_CODE ";
                if (compSearch.COMP_NAME != null) whereClause = whereClause + " ORDER BY  COMP_NAME ";
            }

                sql = "SELECT COMP_CODE,COMP_NAME FROM FM_COMPANY,ADM_USER_COMP  WHERE COMP_CODE=UC_COMP_CODE AND UC_USER_ID = '" + compSearch.userId + "' AND COMP_FRZ_FLAG = 'N' AND "
                    + whereClause + " offset " + (compSearch.pageNum - 1) * compSearch.pageSize + " rows fetch next " + compSearch.pageSize + " rows only";
              
            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);
                    

                }
                catch (Exception ex)
                {

                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }

        [HttpPost]
        public async Task<CommonResponse<object>> ItemLookup(ItemLookup itemSearch)
        {

            
            //if(itemSearch.strAdd is null)
            // sql = "SELECT ITEM_CODE,ITEM_NAME FROM OM_ITEM   WHERE (ITEM_CODE LIKE '"  + itemSearch.strSearch + "' OR ITEM_NAME LIKE '"  + itemSearch.strSearch + "') AND ITEM_FRZ_FLAG = 'N' AND ITEM_COMP_CODE = '" + itemSearch.compcode + "' ORDER BY ITEM_CODE offset " + (itemSearch.pageNum - 1) * itemSearch.pageSize + " rows fetch next " + itemSearch.pageSize + " rows only";
            //else 
            // sql = "SELECT ITEM_CODE,ITEM_NAME FROM OM_ITEM   WHERE (ITEM_CODE LIKE '" + itemSearch.strSearch + "' OR ITEM_NAME LIKE '" + itemSearch.strSearch + "') AND ITEM_FRZ_FLAG = 'N' AND ITEM_COMP_CODE = '" + itemSearch.compcode + "' AND " + itemSearch.strAdd + " ORDER BY ITEM_CODE offset " + (itemSearch.pageNum - 1) * itemSearch.pageSize + " rows fetch next " + itemSearch.pageSize + " rows only";

            var sql = "";
            string whereClause = " ";

            if (itemSearch.ITEM_CODE != null) whereClause = whereClause + "ITEM_CODE LIKE '" + itemSearch.ITEM_CODE + "'";
            if (itemSearch.ITEM_NAME != null) whereClause = whereClause + "ITEM_NAME LIKE '" + itemSearch.ITEM_NAME + "'";
            if (itemSearch.strAdd != null) whereClause = whereClause + " AND " + itemSearch.strAdd;
            if (itemSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemSearch.sortby.ToString();
            }
            else
            {
                if (itemSearch.ITEM_CODE != null) whereClause = whereClause + " ORDER BY  ITEM_CODE ";
                if (itemSearch.ITEM_NAME != null) whereClause = whereClause + " ORDER BY  ITEM_NAME ";
            }

            sql = "SELECT ITEM_CODE,ITEM_NAME FROM OM_ITEM  WHERE  ITEM_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (itemSearch.pageNum - 1) * itemSearch.pageSize + " rows fetch next " + itemSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();



            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                        response.ReturnCompleteRow = await conn.QueryAsync(sql);                   

                }
                catch (Exception ex)
                {

                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }

        [HttpPost]
        //[ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<CommonResponse<object>> ItemGroupLookup(ItemGroupLookup itemGroupSearch)
        {

            
            //if (itemGroupSearch.strAdd is null)
            //    sql = "SELECT IG_CODE,IG_NAME FROM OM_ITEM_GROUP   WHERE (IG_CODE LIKE '" + itemGroupSearch.strSearch + "' OR IG_NAME LIKE '" + itemGroupSearch.strSearch + "') AND IG_FRZ_FLAG = 'N' AND IG_COMP_CODE = '" + itemGroupSearch.compcode + "' ORDER BY IG_CODE offset " + (itemGroupSearch.pageNum - 1) * itemGroupSearch.pageSize + " rows fetch next " + itemGroupSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT IG_CODE,IG_NAME FROM OM_ITEM_GROUP   WHERE (IG_CODE LIKE '" + itemGroupSearch.strSearch + "' OR IG_NAME LIKE '" + itemGroupSearch.strSearch + "') AND IG_FRZ_FLAG = 'N' AND IG_COMP_CODE = '" + itemGroupSearch.compcode + "' AND " + itemGroupSearch.strAdd + " ORDER BY IG_CODE offset " + (itemGroupSearch.pageNum - 1) * itemGroupSearch.pageSize + " rows fetch next " + itemGroupSearch.pageSize + " rows only";


            var sql = "";
            string whereClause = "";

            if (itemGroupSearch.IG_CODE != null) whereClause = whereClause + "IG_CODE LIKE '" + itemGroupSearch.IG_CODE + "'";
            if (itemGroupSearch.IG_NAME != null) whereClause = whereClause + "IG_NAME LIKE '" + itemGroupSearch.IG_NAME + "'";
            if (itemGroupSearch.strAdd != null) whereClause = whereClause + " AND " + itemGroupSearch.strAdd;
            if (itemGroupSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemGroupSearch.sortby.ToString();
            }
            else
            {
                if (itemGroupSearch.IG_CODE != null) whereClause = whereClause + " ORDER BY  IG_CODE ";
                if (itemGroupSearch.IG_NAME != null) whereClause = whereClause + " ORDER BY  IG_NAME ";
            }

            sql = "SELECT IG_CODE,IG_NAME FROM OM_ITEM_GROUP  WHERE   IG_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (itemGroupSearch.pageNum - 1) * itemGroupSearch.pageSize + " rows fetch next " + itemGroupSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();



            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }


        [HttpPost]
        public async Task<CommonResponse<object>> ItemSubGroupLookup(ItemSubGroupLookup itemSubGroupSearch)
        {


            //if (itemSubGroupSearch.strAdd is null)
            //    sql = "SELECT ISG_CODE,ISG_NAME FROM OM_ITEM_SUB_GROUP  INNER JOIN OM_ITEM_GROUP ON ISG_IG_CODE = IG_CODE   WHERE (ISG_CODE LIKE '" + itemSubGroupSearch.strSearch + "' OR ISG_NAME LIKE '" + itemSubGroupSearch.strSearch + "') AND ISG_IG_CODE=IG_CODE AND ISG_FRZ_FLAG = 'N' AND IG_COMP_CODE = '" + itemSubGroupSearch.compcode + "' ORDER BY ISG_CODE offset " + (itemSubGroupSearch.pageNum - 1) * itemSubGroupSearch.pageSize + " rows fetch next " + itemSubGroupSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT ISG_CODE,ISG_NAME FROM OM_ITEM_SUB_GROUP INNER JOIN OM_ITEM_GROUP ON ISG_IG_CODE = IG_CODE   WHERE (ISG_CODE LIKE '" + itemSubGroupSearch.strSearch + "' OR ISG_NAME LIKE '" + itemSubGroupSearch.strSearch + "') AND ISG_IG_CODE=IG_CODE AND ISG_FRZ_FLAG = 'N' AND IG_COMP_CODE = '" + itemSubGroupSearch.compcode + "' AND " + itemSubGroupSearch.strAdd + " ORDER BY ISG_CODE offset " + (itemSubGroupSearch.pageNum - 1) * itemSubGroupSearch.pageSize + " rows fetch next " + itemSubGroupSearch.pageSize + " rows only";



            var sql = "";
            string whereClause = " ";

            if (itemSubGroupSearch.ISG_CODE != null) whereClause = whereClause + "ISG_CODE LIKE '" + itemSubGroupSearch.ISG_CODE + "'";
            if (itemSubGroupSearch.ISG_NAME != null) whereClause = whereClause + "ISG_NAME LIKE '" + itemSubGroupSearch.ISG_NAME + "'";
            if (itemSubGroupSearch.strAdd != null) whereClause = whereClause + " AND " + itemSubGroupSearch.strAdd;
            if (itemSubGroupSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemSubGroupSearch.sortby.ToString();
            }
            else
            {
                if (itemSubGroupSearch.ISG_CODE != null) whereClause = whereClause + " ORDER BY  ISG_CODE ";
                if (itemSubGroupSearch.ISG_NAME != null) whereClause = whereClause + " ORDER BY  ISG_NAME ";
            }

            sql = "SELECT ISG_CODE,ISG_NAME FROM OM_ITEM_SUB_GROUP  INNER JOIN OM_ITEM_GROUP ON ISG_IG_CODE = IG_CODE   WHERE  ISG_FRZ_FLAG = 'N' AND "
                   + whereClause + " offset " + (itemSubGroupSearch.pageNum - 1) * itemSubGroupSearch.pageSize + " rows fetch next " + itemSubGroupSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }


        [HttpPost]
        public async Task<CommonResponse<object>> ItemTypeLookup(CommonLookup itemTypeSearch)
        {

            //if (itemTypeSearch.strAdd is null)
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_TYPE' AND  (CM_CODE LIKE '" + itemTypeSearch.strSearch + "' OR CM_NAME LIKE '" + itemTypeSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemTypeSearch.compcode + "' ORDER BY CM_CODE offset " + (itemTypeSearch.pageNum - 1) * itemTypeSearch.pageSize + " rows fetch next " + itemTypeSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_TYPE' AND  (CM_CODE LIKE '" + itemTypeSearch.strSearch + "' OR CM_NAME LIKE '" + itemTypeSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemTypeSearch.compcode + "' AND " + itemTypeSearch.strAdd + " ORDER BY CM_CODE offset " + (itemTypeSearch.pageNum - 1) * itemTypeSearch.pageSize + " rows fetch next " + itemTypeSearch.pageSize + " rows only";


            var sql = "";
            string whereClause = " ";

            if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + "CM_CODE LIKE '" + itemTypeSearch.CM_CODE + "'";
            if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + "CM_NAME LIKE '" + itemTypeSearch.CM_NAME + "'";
            if (itemTypeSearch.strAdd != null) whereClause = whereClause + " AND " + itemTypeSearch.strAdd;
            if (itemTypeSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemTypeSearch.sortby.ToString();
            }
            else
            {
                if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + " ORDER BY  CM_CODE ";
                if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + " ORDER BY  CM_NAME ";
            }

            sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON  WHERE  CM_TYPE = 'ITEM_TYPE' AND  CM_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (itemTypeSearch.pageNum - 1) * itemTypeSearch.pageSize + " rows fetch next " + itemTypeSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }


        [HttpPost]
        public async Task<CommonResponse<object>> ItemSubTypeLookup(CommonLookup itemTypeSearch)
        {

            //if (itemSubTypeSearch.strAdd is null)
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_SUB_TYPE' AND  (CM_CODE LIKE '" + itemSubTypeSearch.strSearch + "' OR CM_NAME LIKE '" + itemSubTypeSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemSubTypeSearch.compcode + "' ORDER BY CM_CODE offset " + (itemSubTypeSearch.pageNum - 1) * itemSubTypeSearch.pageSize + " rows fetch next " + itemSubTypeSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_SUB_TYPE' AND  (CM_CODE LIKE '" + itemSubTypeSearch.strSearch + "' OR CM_NAME LIKE '" + itemSubTypeSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemSubTypeSearch.compcode + "' AND " + itemSubTypeSearch.strAdd + " ORDER BY CM_CODE offset " + (itemSubTypeSearch.pageNum - 1) * itemSubTypeSearch.pageSize + " rows fetch next " + itemSubTypeSearch.pageSize + " rows only";

            var sql = "";
            string whereClause = " ";

            if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + "CM_CODE LIKE '" + itemTypeSearch.CM_CODE + "'";
            if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + "CM_NAME LIKE '" + itemTypeSearch.CM_NAME + "'";
            if (itemTypeSearch.strAdd != null) whereClause = whereClause + " AND " + itemTypeSearch.strAdd;
            if (itemTypeSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemTypeSearch.sortby.ToString();
            }
            else
            {
                if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + " ORDER BY  CM_CODE ";
                if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + " ORDER BY  CM_NAME ";
            }

            sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON  WHERE  CM_TYPE = 'ITEM_SUB_TYPE' AND  CM_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (itemTypeSearch.pageNum - 1) * itemTypeSearch.pageSize + " rows fetch next " + itemTypeSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }


        [HttpPost]
        public async Task<CommonResponse<object>> ItemEquipmentLookup(CommonLookup itemTypeSearch)
        {



            //if (itemEquipmentSearch.strAdd is null)
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_EQUIP' AND  (CM_CODE LIKE '" + itemEquipmentSearch.strSearch + "' OR CM_NAME LIKE '" + itemEquipmentSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemEquipmentSearch.compcode + "' ORDER BY CM_CODE offset " + (itemEquipmentSearch.pageNum - 1) * itemEquipmentSearch.pageSize + " rows fetch next " + itemEquipmentSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_EQUIP' AND  (CM_CODE LIKE '" + itemEquipmentSearch.strSearch + "' OR CM_NAME LIKE '" + itemEquipmentSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemEquipmentSearch.compcode + "' AND " + itemEquipmentSearch.strAdd + " ORDER BY CM_CODE offset " + (itemEquipmentSearch.pageNum - 1) * itemEquipmentSearch.pageSize + " rows fetch next " + itemEquipmentSearch.pageSize + " rows only";


            var sql = "";
            string whereClause = " ";

            if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + "CM_CODE LIKE '" + itemTypeSearch.CM_CODE + "'";
            if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + "CM_NAME LIKE '" + itemTypeSearch.CM_NAME + "'";
            if (itemTypeSearch.strAdd != null) whereClause = whereClause + " AND " + itemTypeSearch.strAdd;
            if (itemTypeSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemTypeSearch.sortby.ToString();
            }
            else
            {
                if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + " ORDER BY  CM_CODE ";
                if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + " ORDER BY  CM_NAME ";
            }

            sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON  WHERE  CM_TYPE = 'ITEM_EQUIP' AND  CM_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (itemTypeSearch.pageNum - 1) * itemTypeSearch.pageSize + " rows fetch next " + itemTypeSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }


        [HttpPost]
        public async Task<CommonResponse<object>> ItemSubEquipmentLookup(CommonLookup itemTypeSearch)
        {


            //if (itemSubEquipmentSearch.strAdd is null)
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_SUB_EQUIP' AND  (CM_CODE LIKE '" + itemSubEquipmentSearch.strSearch + "' OR CM_NAME LIKE '" + itemSubEquipmentSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemSubEquipmentSearch.compcode + "' ORDER BY CM_CODE offset " + (itemSubEquipmentSearch.pageNum - 1) * itemSubEquipmentSearch.pageSize + " rows fetch next " + itemSubEquipmentSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_SUB_EQUIP' AND  (CM_CODE LIKE '" + itemSubEquipmentSearch.strSearch + "' OR CM_NAME LIKE '" + itemSubEquipmentSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemSubEquipmentSearch.compcode + "' AND " + itemSubEquipmentSearch.strAdd + " ORDER BY CM_CODE offset " + (itemSubEquipmentSearch.pageNum - 1) * itemSubEquipmentSearch.pageSize + " rows fetch next " + itemSubEquipmentSearch.pageSize + " rows only";



            var sql = "";
            string whereClause = " ";

            if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + "CM_CODE LIKE '" + itemTypeSearch.CM_CODE + "'";
            if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + "CM_NAME LIKE '" + itemTypeSearch.CM_NAME + "'";
            if (itemTypeSearch.strAdd != null) whereClause = whereClause + " AND " + itemTypeSearch.strAdd;
            if (itemTypeSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemTypeSearch.sortby.ToString();
            }
            else
            {
                if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + " ORDER BY  CM_CODE ";
                if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + " ORDER BY  CM_NAME ";
            }

            sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON  WHERE  CM_TYPE = 'ITEM_SUB_EQUIP' AND  CM_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (itemTypeSearch.pageNum - 1) * itemTypeSearch.pageSize + " rows fetch next " + itemTypeSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }



        [HttpPost]
        public async Task<CommonResponse<object>> ItemMakeUpLookup(CommonLookup itemTypeSearch)
        {



            //if (itemMakeSearch.strAdd is null)
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_MAKE' AND  (CM_CODE LIKE '" + itemMakeSearch.strSearch + "' OR CM_NAME LIKE '" + itemMakeSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemMakeSearch.compcode + "' ORDER BY CM_CODE offset " + (itemMakeSearch.pageNum - 1) * itemMakeSearch.pageSize + " rows fetch next " + itemMakeSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_MAKE' AND  (CM_CODE LIKE '" + itemMakeSearch.strSearch + "' OR CM_NAME LIKE '" + itemMakeSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemMakeSearch.compcode + "' AND " + itemMakeSearch.strAdd + " ORDER BY CM_CODE offset " + (itemMakeSearch.pageNum - 1) * itemMakeSearch.pageSize + " rows fetch next " + itemMakeSearch.pageSize + " rows only";

            var sql = "";
            string whereClause = " ";

            if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + "CM_CODE LIKE '" + itemTypeSearch.CM_CODE + "'";
            if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + "CM_NAME LIKE '" + itemTypeSearch.CM_NAME + "'";
            if (itemTypeSearch.strAdd != null) whereClause = whereClause + " AND " + itemTypeSearch.strAdd;
            if (itemTypeSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemTypeSearch.sortby.ToString();
            }
            else
            {
                if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + " ORDER BY  CM_CODE ";
                if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + " ORDER BY  CM_NAME ";
            }

            sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON  WHERE  CM_TYPE = 'ITEM_MAKE' AND  CM_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (itemTypeSearch.pageNum - 1) * itemTypeSearch.pageSize + " rows fetch next " + itemTypeSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }


        [HttpPost]
        public async Task<CommonResponse<object>> ItemSellTypeLookup(CommonLookup itemTypeSearch)
        {


            //if (itemSellTypeSearch.strAdd is null)
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_SELL_TYPE' AND  (CM_CODE LIKE '" + itemSellTypeSearch.strSearch + "' OR CM_NAME LIKE '" + itemSellTypeSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemSellTypeSearch.compcode + "' ORDER BY CM_CODE offset " + (itemSellTypeSearch.pageNum - 1) * itemSellTypeSearch.pageSize + " rows fetch next " + itemSellTypeSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_SELL_TYPE' AND  (CM_CODE LIKE '" + itemSellTypeSearch.strSearch + "' OR CM_NAME LIKE '" + itemSellTypeSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemSellTypeSearch.compcode + "' AND " + itemSellTypeSearch.strAdd + " ORDER BY CM_CODE offset " + (itemSellTypeSearch.pageNum - 1) * itemSellTypeSearch.pageSize + " rows fetch next " + itemSellTypeSearch.pageSize + " rows only";

            var sql = "";
            string whereClause = " ";

            if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + "CM_CODE LIKE '" + itemTypeSearch.CM_CODE + "'";
            if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + "CM_NAME LIKE '" + itemTypeSearch.CM_NAME + "'";
            if (itemTypeSearch.strAdd != null) whereClause = whereClause + " AND " + itemTypeSearch.strAdd;
            if (itemTypeSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemTypeSearch.sortby.ToString();
            }
            else
            {
                if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + " ORDER BY  CM_CODE ";
                if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + " ORDER BY  CM_NAME ";
            }

            sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON  WHERE  CM_TYPE = 'ITEM_SELL_TYPE' AND  CM_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (itemTypeSearch.pageNum - 1) * itemTypeSearch.pageSize + " rows fetch next " + itemTypeSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }


        [HttpPost]
        public async Task<CommonResponse<object>> ItemClassLookup(CommonLookup itemTypeSearch)
        {

            //if (itemClassSearch.strAdd is null)
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_CLASS' AND  (CM_CODE LIKE '" + itemClassSearch.strSearch + "' OR CM_NAME LIKE '" + itemClassSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemClassSearch.compcode + "' ORDER BY CM_CODE offset " + (itemClassSearch.pageNum - 1) * itemClassSearch.pageSize + " rows fetch next " + itemClassSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_CLASS' AND  (CM_CODE LIKE '" + itemClassSearch.strSearch + "' OR CM_NAME LIKE '" + itemClassSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemClassSearch.compcode + "' AND " + itemClassSearch.strAdd + " ORDER BY CM_CODE offset " + (itemClassSearch.pageNum - 1) * itemClassSearch.pageSize + " rows fetch next " + itemClassSearch.pageSize + " rows only";

            var sql = "";
            string whereClause = " ";

            if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + "CM_CODE LIKE '" + itemTypeSearch.CM_CODE + "'";
            if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + "CM_NAME LIKE '" + itemTypeSearch.CM_NAME + "'";
            if (itemTypeSearch.strAdd != null) whereClause = whereClause + " AND " + itemTypeSearch.strAdd;
            if (itemTypeSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemTypeSearch.sortby.ToString();
            }
            else
            {
                if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + " ORDER BY  CM_CODE ";
                if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + " ORDER BY  CM_NAME ";
            }

            sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON  WHERE  CM_TYPE = 'ITEM_CLASS' AND  CM_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (itemTypeSearch.pageNum - 1) * itemTypeSearch.pageSize + " rows fetch next " + itemTypeSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }

        [HttpPost]
        public async Task<CommonResponse<object>> ItemTemparatureLookup(CommonLookup itemTypeSearch)
        {


            //if (itemTemparatureSearch.strAdd is null)
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_TEMPERATURE' AND  (CM_CODE LIKE '" + itemTemparatureSearch.strSearch + "' OR CM_NAME LIKE '" + itemTemparatureSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemTemparatureSearch.compcode + "' ORDER BY CM_CODE offset " + (itemTemparatureSearch.pageNum - 1) * itemTemparatureSearch.pageSize + " rows fetch next " + itemTemparatureSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_TEMPERATURE' AND  (CM_CODE LIKE '" + itemTemparatureSearch.strSearch + "' OR CM_NAME LIKE '" + itemTemparatureSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemTemparatureSearch.compcode + "' AND " + itemTemparatureSearch.strAdd + " ORDER BY CM_CODE offset " + (itemTemparatureSearch.pageNum - 1) * itemTemparatureSearch.pageSize + " rows fetch next " + itemTemparatureSearch.pageSize + " rows only";

            var sql = "";
            string whereClause = " ";

            if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + "CM_CODE LIKE '" + itemTypeSearch.CM_CODE + "'";
            if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + "CM_NAME LIKE '" + itemTypeSearch.CM_NAME + "'";
            if (itemTypeSearch.strAdd != null) whereClause = whereClause + " AND " + itemTypeSearch.strAdd;
            if (itemTypeSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemTypeSearch.sortby.ToString();
            }
            else
            {
                if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + " ORDER BY  CM_CODE ";
                if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + " ORDER BY  CM_NAME ";
            }

            sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON  WHERE  CM_TYPE = 'ITEM_TEMPERATURE' AND  CM_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (itemTypeSearch.pageNum - 1) * itemTypeSearch.pageSize + " rows fetch next " + itemTypeSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }




        [HttpPost]
        public async Task<CommonResponse<object>> ItemVEDLookup(CommonLookup itemTypeSearch)
        {


            //if (itemTemparatureSearch.strAdd is null)
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_TEMPERATURE' AND  (CM_CODE LIKE '" + itemTemparatureSearch.strSearch + "' OR CM_NAME LIKE '" + itemTemparatureSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemTemparatureSearch.compcode + "' ORDER BY CM_CODE offset " + (itemTemparatureSearch.pageNum - 1) * itemTemparatureSearch.pageSize + " rows fetch next " + itemTemparatureSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON   WHERE CM_TYPE = 'ITEM_TEMPERATURE' AND  (CM_CODE LIKE '" + itemTemparatureSearch.strSearch + "' OR CM_NAME LIKE '" + itemTemparatureSearch.strSearch + "')  AND CM_FRZ_FLAG = 'N' AND CM_COMP_CODE = '" + itemTemparatureSearch.compcode + "' AND " + itemTemparatureSearch.strAdd + " ORDER BY CM_CODE offset " + (itemTemparatureSearch.pageNum - 1) * itemTemparatureSearch.pageSize + " rows fetch next " + itemTemparatureSearch.pageSize + " rows only";

            var sql = "";
            string whereClause = " ";

            if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + "CM_CODE LIKE '" + itemTypeSearch.CM_CODE + "'";
            if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + "CM_NAME LIKE '" + itemTypeSearch.CM_NAME + "'";
            if (itemTypeSearch.strAdd != null) whereClause = whereClause + " AND " + itemTypeSearch.strAdd;
            if (itemTypeSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemTypeSearch.sortby.ToString();
            }
            else
            {
                if (itemTypeSearch.CM_CODE != null) whereClause = whereClause + " ORDER BY  CM_CODE ";
                if (itemTypeSearch.CM_NAME != null) whereClause = whereClause + " ORDER BY  CM_NAME ";
            }

            sql = "SELECT CM_CODE,CM_NAME FROM OM_COMMON  WHERE  CM_TYPE = 'ITEM_VED' AND  CM_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (itemTypeSearch.pageNum - 1) * itemTypeSearch.pageSize + " rows fetch next " + itemTypeSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }


        [HttpPost]
        public async Task<CommonResponse<object>> UomLookup(ItemUomLookup itemuomSearch)
        {



            //if (itemuomSearch.strAdd is null)
            //    sql = "SELECT UOM_CODE,UOM_NAME FROM OM_UOM WHERE UOM_FRZ_FLAG = 'N'  AND UOM_CODE LIKE '" + itemuomSearch.strSearch + "' OR UOM_NAME LIKE '" + itemuomSearch.strSearch + "'  ORDER BY UOM_CODE offset " + (itemuomSearch.pageNum - 1) * itemuomSearch.pageSize + " rows fetch next " + itemuomSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT UOM_CODE,UOM_NAME FROM OM_UOM WHERE UOM_FRZ_FLAG = 'N'  AND ( UOM_CODE LIKE '" + itemuomSearch.strSearch + "' OR UOM_NAME LIKE '" + itemuomSearch.strSearch + "' )  AND " + itemuomSearch.strAdd + "  ORDER BY UOM_CODE offset " + (itemuomSearch.pageNum - 1) * itemuomSearch.pageSize + " rows fetch next " + itemuomSearch.pageSize + " rows only";


            var sql = "";
            string whereClause = " ";

            if (itemuomSearch.UOM_CODE != null) whereClause = whereClause + "UOM_CODE LIKE '" + itemuomSearch.UOM_CODE + "'";
            if (itemuomSearch.UOM_NAME != null) whereClause = whereClause + "UOM_NAME LIKE '" + itemuomSearch.UOM_NAME + "'";
            if (itemuomSearch.strAdd != null) whereClause = whereClause + " AND " + itemuomSearch.strAdd;
            if (itemuomSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + itemuomSearch.sortby.ToString();
            }
            else
            {
                if (itemuomSearch.UOM_CODE != null) whereClause = whereClause + " ORDER BY  UOM_CODE ";
                if (itemuomSearch.UOM_NAME != null) whereClause = whereClause + " ORDER BY  UOM_NAME ";
            }

            sql = "SELECT UOM_CODE,UOM_NAME FROM OM_UOM  WHERE   UOM_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (itemuomSearch.pageNum - 1) * itemuomSearch.pageSize + " rows fetch next " + itemuomSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }


        [HttpPost]
        public async Task<CommonResponse<object>> SupplierLookup(SpplierLookup supplierSearch)
        {




            //if (supplierSearch.strAdd is null)
            //    sql = "SELECT SUPP_CODE,SUPP_NAME FROM OM_SUPPLIER WHERE SUPP_FRZ_FLAG = 'N'  AND SUPP_CODE LIKE '" + supplierSearch.strSearch + "' OR SUPP_NAME LIKE '" + supplierSearch.strSearch + "'  ORDER BY SUPP_CODE offset " + (supplierSearch.pageNum - 1) * supplierSearch.pageSize + " rows fetch next " + supplierSearch.pageSize + " rows only";
            //else
            //    sql = "SELECT SUPP_CODE,UOM_NAME FROM supplierSearch WHERE SUPP_FRZ_FLAG = 'N'  AND ( SUPP_CODE LIKE '" + supplierSearch.strSearch + "' OR SUPP_NAME LIKE '" + supplierSearch.strSearch + "' )  AND " + supplierSearch.strAdd + "  ORDER BY SUPP_CODE offset " + (supplierSearch.pageNum - 1) * supplierSearch.pageSize + " rows fetch next " + supplierSearch.pageSize + " rows only";

            var sql = "";
            string whereClause = " ";

            if (supplierSearch.SUPP_CODE != null) whereClause = whereClause + "SUPP_CODE LIKE '" + supplierSearch.SUPP_CODE + "'";
            if (supplierSearch.SUPP_NAME != null) whereClause = whereClause + "COMP_NAME LIKE '" + supplierSearch.SUPP_NAME + "'";
            if (supplierSearch.strAdd != null) whereClause = whereClause + " AND " + supplierSearch.strAdd;
            if (supplierSearch.sortby != null)
            {
                whereClause = whereClause + " ORDER BY " + supplierSearch.sortby.ToString();
            }
            else
            {
                if (supplierSearch.SUPP_CODE != null) whereClause = whereClause + " ORDER BY  SUPP_CODE ";
                if (supplierSearch.SUPP_NAME != null) whereClause = whereClause + " ORDER BY  SUPP_NAME ";
            }

            sql = "SELECT SUPP_CODE,SUPP_NAME FROM OM_SUPPLIER  WHERE  SUPP_FRZ_FLAG = 'N' AND "
                + whereClause + " offset " + (supplierSearch.pageNum - 1) * supplierSearch.pageSize + " rows fetch next " + supplierSearch.pageSize + " rows only";

            CommonResponse<object> response = new CommonResponse<object>();


            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync(sql);

                }
                catch (Exception ex)
                {
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;
                    response.ErrorString = ex.Message;
                }


                finally
                {
                    conn.Close();

                }


                return response;
            }

        }



    }
}
