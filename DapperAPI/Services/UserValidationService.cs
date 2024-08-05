using Dapper;
using DapperAPI.Data;
using DapperAPI.EntityModel;
using DapperAPI.Interface;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DapperAPI.Services
{
    public class UserValidationService : IUserValidationService
    {

        private readonly IDbConnectionProvider _dbConnectionProvider;
        private readonly IConfiguration _configuration;
        public string key, refreshKey;
        public int tokenlifetime, refreshlife;
        private authToken objToken;
        public CommonResponse<object> objModelError;
        public UserValidationService(IConfiguration configuration, IDbConnectionProvider dbConnectionProvider)
        {
            _dbConnectionProvider = dbConnectionProvider;
            _configuration = configuration;
            key = _configuration.GetSection("JWT").GetSection("Key").Value;
            refreshKey = _configuration.GetSection("JWT").GetSection("RefreshKey").Value;
            tokenlifetime = Convert.ToInt32(_configuration.GetSection("JWT").GetSection("TockenLife").Value);
            refreshlife = Convert.ToInt32(_configuration.GetSection("JWT").GetSection("RefreshLife").Value);

        }


            public async Task<string> GetUserTypeAsync(string userId)
        {
            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                var user = await conn.QueryFirstOrDefaultAsync<ADM_USER>(
            "SELECT USER_TYPE FROM ADM_USER WHERE USER_ID = @UserId AND USER_FRZ_FLAG = 'N'",
            new { UserId = userId });

                return user?.USER_TYPE;
            }
        }

        public async Task<bool> IsUserValidAsync(string userId)
        {
            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                var user = await conn.QueryFirstOrDefaultAsync<ADM_USER>(
            "SELECT 1 FROM ADM_USER WHERE USER_ID = @UserId AND USER_FRZ_FLAG = 'N'",
            new { UserId = userId });

                return user != null;
            }
        }

        public async Task<bool> HasCompanyAccessAsync(string userId, string companyCode)
        {
            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                var userComp = await conn.QueryFirstOrDefaultAsync<ADM_USER_COMP>(
            "SELECT 1 FROM ADM_USER_COMP WHERE UC_USER_ID = @UserId AND UC_COMP_CODE = @CompanyCode",
            new { UserId = userId, CompanyCode = companyCode });

                return userComp != null;
            }
        }

        public async Task<bool> IsCompanyValidAsync(string companyCode)
        {
            using (var conn = _dbConnectionProvider.CreateConnection())
            {
                var company = await conn.QueryFirstOrDefaultAsync<FM_COMPANY>(
            "SELECT 1 FROM FM_COMPANY WHERE COMP_CODE = @CompanyCode AND COMP_FRZ_FLAG = 'N'",
            new { CompanyCode = companyCode });

                return company != null;
            }
        }

        public Tokens CallAccessToken(string userid)
        {
            try
            {
                var issuer = userid;
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                     {
                  new Claim(ClaimTypes.Name, userid)
                     }),
                    Expires = DateTime.Now.AddMinutes(tokenlifetime),
                    SigningCredentials = new SigningCredentials
                        (new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                var refreshToken = GenerateRefreshToken(userid);
                objToken = new authToken { accessToken = jwtToken, RefreshToken = refreshToken };
                return new Tokens { AccessToken = jwtToken, RefreshToken = refreshToken };
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public string GenerateRefreshToken(string userid)
        {
            try
            {
                var issuer = userid;
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                     {
                  new Claim(ClaimTypes.Name, userid)
                     }),
                    Expires = DateTime.Now.AddMinutes(tokenlifetime),
                    SigningCredentials = new SigningCredentials
                        (new SymmetricSecurityKey(Encoding.ASCII.GetBytes(refreshKey)),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtRefreshToken = tokenHandler.WriteToken(token);

                return jwtRefreshToken;
            }
            catch (Exception ex)
            {
                return null;
            }
        }




        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(refreshKey)),
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch (Exception ex)
            {
                return null;
            }
        }




        public bool accessTokenValidity()
        {
            bool result = false;
            try
            {
                var principal = GetPrincipalFromExpiredToken(objToken.accessToken);
                var username = "";



                if (principal != null)
                {
                    username = principal.Identity?.Name;
                    if (principal.Identity?.IsAuthenticated == false)
                    {
                        result = false;
                    }
                }
                else
                {

                    result = true;
                }

            }
            catch (Exception ex)
            {

            }

            return result;

        }
    }
}
