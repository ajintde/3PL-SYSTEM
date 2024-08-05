using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServiceStack.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace DapperAPI.Services
{
    
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
       // private readonly AppSettings _appSettings;

        public JwtMiddleware(RequestDelegate next )
        {
            _next = next;
           // _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                await attachUserToContext(context, token);

            await _next(context);
        }

        private async Task attachUserToContext(HttpContext context,  string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                
            }
            catch
            {
                //Do nothing if JWT validation fails
                // user is not attached to context so the request won't have access to secure routes
            }
        }
    }
}
