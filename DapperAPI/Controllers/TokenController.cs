using DapperAPI.Data;
using DapperAPI.EntityModel;
using DapperAPI.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;
using System.Linq;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using DapperAPI.Services;
using static Dapper.SqlMapper;
using System.Runtime.InteropServices.JavaScript;
using System.Transactions;
using System.Data.Common;
using System;
using Microsoft.AspNetCore.Cors;


namespace DapperAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    
    public class TokenController : ControllerBase
    {

        private readonly IDbConnectionProvider _dbConnectionProvider;
        private readonly IUserValidationService _userValidationService;
        private readonly IConfiguration _configuration;

        

        public TokenController(IConfiguration configuration, IDbConnectionProvider dbConnectionProvider, IUserValidationService userValidationService)
        {
            _dbConnectionProvider = dbConnectionProvider;
            _configuration = configuration;
            _userValidationService= userValidationService;


        }

        
        [HttpPost]
        [Route("authenticate-user")]
        
      // [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<CommonResponse<object>> GetTokenController([FromBody] UserLogin objLogin) 
        {
            CommonResponse<object> response = new CommonResponse<object>();
            if (!ModelState.IsValid)
            {
                HttpContext.Response.ContentType = "application/json";
                HttpContext.Response.StatusCode = 400;
                response.ValidationSuccess = false;
                response.StatusCode = "400";
                response.ErrorString = "Invalid entry.";
                return response;
            }

            //  var sql = "SELECT * FROM OM_ITEM order by ITEM_CODE offset " + (pagenum - 1) * pagesize + " rows fetch next " + pagesize + " rows only";
            var sql = "SELECT  user_id,user_pwd from adm_user  INNER JOIN ADM_USER_COMP  ON  user_id = uc_user_id WHERE user_id='" + objLogin.userId + "' and user_pwd='" + objLogin.Password + "'";
           
            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {


                try
                {

                    response.ReturnCompleteRow = await conn.QueryAsync<ADM_USER>(sql);
                    string? json = JsonConvert.SerializeObject(response.ReturnCompleteRow);
                    List<ADM_USER>? jsonObject = System.Text.Json.JsonSerializer.Deserialize<List<ADM_USER>>(json);
                    
                    conn.Close(); 

                    if (jsonObject.Count > 0)
                    {
                        if (jsonObject[0].USER_ID == objLogin.userId && jsonObject[0].USER_PWD == objLogin.Password)
                        {
                             sql = "SELECT  COMP_CODE,COMP_NAME from ADM_USER_COMP  INNER JOIN FM_COMPANY  ON  uc_comp_code = comp_code WHERE uc_user_id='" + objLogin.userId + "'";
                             response.ReturnCompleteRow = await conn.QueryAsync(sql);
                             json = JsonConvert.SerializeObject(response.ReturnCompleteRow);
                             List<COMP_ACCESS>? jsoncomplist = System.Text.Json.JsonSerializer.Deserialize<List<COMP_ACCESS>>(json);

                            var token = _userValidationService.CallAccessToken(jsonObject[0].USER_ID);
                            token.CompList = jsoncomplist;

                            if (token == null)
                            {
                                HttpContext.Response.ContentType = "application/json";
                                HttpContext.Response.StatusCode = 403;
                                response.ErrorString = "Permission denied";
                                response.StatusCode = "403";
                                response.ValidationSuccess = false;
                                return response;
                            }

                            //UserRefreshTokens obj = new UserRefreshTokens
                            //{
                            //    RefreshToken = token.RefreshToken,
                            //    UserName = jsonObject[0].USER_ID
                            //};

                            

                            if (conn.State == ConnectionState.Closed) conn.Open();

                            using (var trans = conn.BeginTransaction())
                            {
                                try
                                {

                                    sql = "SELECT  TK_USER_ID,TK_REF_TOKEN from TOKEN_REFRESH   WHERE TK_USER_ID='" + objLogin.userId + "'";
                                    response.ReturnCompleteRow = await conn.QueryAsync(sql, transaction: trans);
                                    string? jsonRec = JsonConvert.SerializeObject(response.ReturnCompleteRow);
                                    List<object>? jsonRecObj = System.Text.Json.JsonSerializer.Deserialize<List<object>>(jsonRec);
                                    if (jsonRecObj.Count > 0)
                                    {
                                        sql = "Delete from TOKEN_REFRESH where TK_USER_ID = '" + jsonObject[0].USER_ID + "' ";
                                        response.ReturnCompleteRow = await conn.ExecuteAsync(sql, transaction: trans);
                                    }
                                    sql = "Insert into TOKEN_REFRESH (TK_USER_ID,TK_REF_TOKEN,TK_CR_UID) VALUES ('" + jsonObject[0].USER_ID + "','" + token.RefreshToken + "','" + jsonObject[0].USER_ID + "')";
                                    response.ReturnCompleteRow = await conn.ExecuteAsync(sql, transaction: trans);
                                    trans.Commit();
                                    trans.Dispose();
                                }

                                catch (Exception ex)
                                {
                                    trans.Rollback();
                                    //LogException(ex);
                                    HttpContext.Response.ContentType = "application/json";
                                    HttpContext.Response.StatusCode = 400;
                                    response.StatusCode = "400";
                                    response.ValidationSuccess = false;
                                    response.ErrorString = ex.Message;
                                }
                            }
                         
                            response.ReturnCompleteRow = token;
                        }
                        else
                        {
                            HttpContext.Response.ContentType = "application/json";
                            HttpContext.Response.StatusCode = 403;
                            response.ValidationSuccess = false;
                            response.StatusCode = "403";
                            response.ErrorString = "Not a valid user";
                        }
                    }
                    else
                    {
                        HttpContext.Response.ContentType = "application/json";
                        HttpContext.Response.StatusCode = 403;
                        response.ValidationSuccess = false;
                        response.StatusCode = "403";
                        response.ErrorString = "Not a valid user";
                    }


                }
                catch (Exception ex)
                {
                    //LogException(ex);
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.StatusCode = "400";
                    response.ValidationSuccess = false;

                    response.ErrorString = ex.Message;
                }

                finally
                {
                   if(conn.State==ConnectionState.Open) conn.Close();
                   conn.Dispose();

                }

                return response;


            }


        }
       
        [HttpPost]
        [Route("refresh-token")]
       // [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<CommonResponse<object>> Refresh(Tokens token)
        {
            CommonResponse<object> response = new CommonResponse<object>();
            //if (!ModelState.IsValid)
            //{
            //    HttpContext.Response.ContentType = "application/json";
            //    HttpContext.Response.StatusCode = 400;
            //    response.ValidationSuccess = false;
            //    response.StatusCode = "400";
            //    response.ErrorString = "Invalid entry.";
            //    return response;
            //}
            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {
                
                
                    try
                    {
                        var principal = _userValidationService.GetPrincipalFromExpiredToken(token.RefreshToken);
                        var username = "";

                       

                        if (principal != null)
                        { 
                          username = principal.Identity?.Name;
                          if (principal.Identity?.IsAuthenticated == false)
                           {
                            HttpContext.Response.ContentType = "application/json";
                            HttpContext.Response.StatusCode = 401;
                            response.StatusCode = "401";
                            response.ValidationSuccess = false;
                            return response;
                        }
                        }
                        else
                        {
                        HttpContext.Response.ContentType = "application/json";
                        HttpContext.Response.StatusCode = 401;
                        response.StatusCode = "401";
                        response.ValidationSuccess = false;
                        return response; }

                        List<TOCKEN_REFRESH>? jsonObject;

                        var sql = "select TK_USER_ID,TK_REF_TOKEN from TOKEN_REFRESH  where TK_USER_ID = ('" + username + "');";
                        response.ReturnCompleteRow = await conn.QueryAsync(sql);
                        string? json = JsonConvert.SerializeObject(response.ReturnCompleteRow);
                        jsonObject = System.Text.Json.JsonSerializer.Deserialize<List<TOCKEN_REFRESH>>(json);
                        if (conn.State == ConnectionState.Open) conn.Close();

                      

                        if (jsonObject[0].TK_REF_TOKEN != token.RefreshToken)
                        {
                            HttpContext.Response.ContentType = "application/json";
                            HttpContext.Response.StatusCode = 401;
                            response.ErrorString = "Permission denied";
                            response.ValidationSuccess = false;
                            response.StatusCode = "401";
                            response.ReturnCompleteRow = null;
                            return response;
                        }

                        var newJwtToken = _userValidationService.CallAccessToken(username);
                        response.ReturnCompleteRow = newJwtToken;

                    if (conn.State == ConnectionState.Closed) conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            sql = "update  TOKEN_REFRESH set TK_REF_TOKEN = '" + newJwtToken.RefreshToken + "'  where TK_USER_ID = ('" + username + "');";
                            var result = await conn.ExecuteAsync(sql, transaction: trans);
                            trans.Commit();
                            trans.Dispose();
                            response.ReturnCompleteRow = newJwtToken;

                            if (newJwtToken == null)
                            {
                                HttpContext.Response.ContentType = "application/json";
                                HttpContext.Response.StatusCode = 401;
                                response.ErrorString += "Permission denied";
                                response.ValidationSuccess = false;
                                response.StatusCode = "400";
                                response.ReturnCompleteRow = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            HttpContext.Response.ContentType = "application/json";
                            HttpContext.Response.StatusCode = 400;
                            response.ErrorString += ex.Message;
                            response.ValidationSuccess = false;
                            response.StatusCode = "400";
                            response.ReturnCompleteRow = null;
                        }
                    }


                }
                catch (Exception ex)
                    {
                        HttpContext.Response.ContentType = "application/json";
                        HttpContext.Response.StatusCode = 400;
                        response.ErrorString += ex.Message;
                        response.ValidationSuccess = false;  
                        response.StatusCode = "400";
                        response.ReturnCompleteRow = null;

                }

                    finally
                    {
                        if (conn.State == ConnectionState.Open) conn.Close();

                    }
                
                return response;
            }

        }




        [HttpPost]
        [Route("passwordReset")]
        [Authorize]
        // [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<CommonResponse<object>> User([FromBody] UserLogin objLogin)
        {
            CommonResponse<object> response = new CommonResponse<object>();
            if (!ModelState.IsValid)
            {
                HttpContext.Response.ContentType = "application/json";
                HttpContext.Response.StatusCode = 400;
                response.ValidationSuccess = false;
                response.StatusCode = "400";
                response.ErrorString = "Invalid entry.";
                return response;
            }

            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {

                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        var sql = "update adm_user set user_pwd='" + objLogin.Password +  "' where USER_ID = '" + objLogin.userId + "';";
                        response.ReturnCompleteRow = await conn.ExecuteAsync(sql, transaction: trans);
                        trans.Commit();
                        trans.Dispose();
                    }

                    catch (Exception ex)
                    {
                        trans.Rollback();
                        //LogException(ex);
                        HttpContext.Response.ContentType = "application/json";
                        HttpContext.Response.StatusCode = 400;
                        response.StatusCode = "400";
                        response.ValidationSuccess = false;
                        response.ErrorString = ex.Message;
                    }
                    finally
                    {
                        if (conn.State == ConnectionState.Open) conn.Close();
                        conn.Dispose();

                    }
                }
            }

                return response;

        }




        [HttpPost]
        [Route("UserMenu")]
        [Authorize]
        //[ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<CommonResponse<object>> GetMenuController(String userid, String companycode)
        {
            CommonResponse<object> response = new CommonResponse<object>();
            //if (!ModelState.IsValid)
            //{
            //    HttpContext.Response.ContentType = "application/json";
            //    HttpContext.Response.StatusCode = 400;
            //    response.ValidationSuccess = false;
            //    response.StatusCode = "400";
            //    response.ErrorString = "Invalid entry.";
            //    return response;
            //}


            var sql = " WITH UserGroup AS ( SELECT USER_GROUP_ID FROM ADM_USER WHERE USER_ID = '" + userid + "'), CTE_CONNECT_BY AS (SELECT 1 AS LEVEL, S.* FROM ADM_MENU S WHERE MENU_ACTION_TYPE NOT IN('R', 'N') " +
                     " AND MENU_ID IN(SELECT GM_MENU_ID FROM ADM_GROUP_MENU GM INNER JOIN ADM_GROUP_MENU_COMP GMC ON GM.GM_GROUP_ID = GMC.GMC_GROUP_ID WHERE GM.GM_MENU_ID = GMC.GMC_MENU_ID AND GMC.GMC_COMP_CODE = '" + companycode +
                     "' AND GM.GM_GROUP_ID = (SELECT USER_GROUP_ID FROM UserGroup)) UNION ALL SELECT LEVEL +1 AS LEVEL, S.* FROM CTE_CONNECT_BY R  INNER JOIN ADM_MENU S ON R.MENU_PARENT_ID = S.MENU_ID) " +
                     " SELECT DISTINCT  * FROM CTE_CONNECT_BY ORDER BY 1 ";

           
            using (IDbConnection conn = _dbConnectionProvider.CreateConnection())
            {


                try
                {
                    response.ReturnCompleteRow = await conn.QueryAsync<object>(sql);
                  
                }
                catch (Exception ex)
                {
                    //LogException(ex);
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.StatusCode = 400;
                    response.ValidationSuccess = false;
                    response.StatusCode = "400";
                    response.ErrorString += ex.Message;
                }

                finally
                {
                    if (conn.State == ConnectionState.Open) conn.Close();
                    conn.Dispose();

                }

                return response;


            }


        }





    }
}
