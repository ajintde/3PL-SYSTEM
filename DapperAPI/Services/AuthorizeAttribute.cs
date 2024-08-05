using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using DapperAPI.EntityModel;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace DapperAPI.Services
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            
            if (context.HttpContext.User.Identity.IsAuthenticated == false)
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = 401;
                CommonResponse<object> response = new CommonResponse<object>();
                response.ValidationSuccess = false;
                response.StatusCode = "401";
                response.ErrorString = "Unauthorized";
                context.Result = new JsonResult(response);
            }
        }
    }
}
