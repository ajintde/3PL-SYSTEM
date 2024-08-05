using DapperAPI.EntityModel;
using System.Security.Claims;

namespace DapperAPI.Interface
{
    public interface IUserValidationService
    {
        Task<string> GetUserTypeAsync(string userId);
        Task<bool> IsUserValidAsync(string userId);
        Task<bool> HasCompanyAccessAsync(string userId, string companyCode);
        Task<bool> IsCompanyValidAsync(string companyCode);
        Tokens CallAccessToken(string userid);
        bool accessTokenValidity();
        string GenerateRefreshToken(string userid);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
