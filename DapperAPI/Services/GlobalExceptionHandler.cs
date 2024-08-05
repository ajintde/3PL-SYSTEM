using DapperAPI.EntityModel;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DapperAPI.Services
{


    public class BaseException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public BaseException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            CommonResponse<object> response = new CommonResponse<object>();
            
            response.ValidationSuccess = false;
            response.StatusCode= Convert.ToString(httpContext.Response.StatusCode);   
            if (exception is BaseException e)
            {
                response.ErrorString = e.Message;
            }
            else
            {
                response.ErrorString = exception.Message;
            }

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = httpContext.Response.StatusCode;

           // await httpContext.Response.WriteAsJsonAsync(response).ConfigureAwait(false);
            return true;
        }
    }
}
