using Microsoft.AspNetCore.Diagnostics;

namespace DapperAPI.Services
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IExceptionHandler _exceptionHandler;

        public ExceptionHandlerMiddleware(RequestDelegate next, IExceptionHandler exceptionHandler)
        {
            _next = next;
            _exceptionHandler = exceptionHandler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var handled = await _exceptionHandler.TryHandleAsync(context, ex, context.RequestAborted);
                if (!handled)
                {
                    throw; // Re-throw the exception if it wasn't handled
                }
            }
        }
    }
}
