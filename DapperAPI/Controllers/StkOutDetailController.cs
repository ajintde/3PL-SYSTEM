using DapperAPI.EntityModel;
using DapperAPI.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DapperAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StkOutDetailController : ControllerBase
    {
        private readonly IMasterDetailRepository<WT_STK_OUT_ITEM, WT_STK_OUT_ITEM> _stkOutDetailRepositor;
        private readonly IUserValidationService _userValidationService;
        public StkOutDetailController(IMasterDetailRepository<WT_STK_OUT_ITEM, WT_STK_OUT_ITEM> stkOutDetailRepositor, IUserValidationService userValidationService) 
        {
            _stkOutDetailRepositor = stkOutDetailRepositor;
            _userValidationService = userValidationService;
        }
        private async Task<bool> ValidateUserAndCompany(string userId, string companyCode)
        {
            var isUserValid = await _userValidationService.IsUserValidAsync(userId);
            if (!isUserValid)
            {
                return false;
            }

            var hasCompanyAccess = await _userValidationService.HasCompanyAccessAsync(userId, companyCode);
            if (!hasCompanyAccess)
            {
                return false;
            }

            var isCompanyValid = await _userValidationService.IsCompanyValidAsync(companyCode);
            if (!isCompanyValid)
            {
                return false;
            }

            return true;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
        {
            if (!await ValidateUserAndCompany(request.User, request.CompanyCode))
            {
                return Unauthorized("User validation failed.");
            }
            // Call the search method from your service layer
            var response = await _stkOutDetailRepositor.Search<WT_STK_OUT_ITEM>(
                request.JsonModel, request.SortBy, request.PageNo, request.PageSize,
                request.CompanyCode, request.User, request.WhereClause,request.ShowDetail
            );

            if (response.ValidationSuccess)
            {
                return Ok(response.ReturnCompleteRow);
            }
            else
            {
                return BadRequest(response.ErrorString);
            }
        }

        [HttpPost("InsertDetailBySeq")]
        public async Task<IActionResult> CreateDeatilBySeq([FromBody] WT_STK_OUT_ITEM detail, string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            if (detail == null)
            {
                return BadRequest("Item is null.");
            }

            var response = await _stkOutDetailRepositor.InsertDetailBySeq(detail, companyCode, user);
            Log.Information("ITEM INSERT NORMAL = {@result}", response);
            return Ok(response);
        }

        [HttpPut("UpdateDetail")]
        public async Task<IActionResult> UpdateDetail([FromBody] WT_STK_OUT_ITEM detail, string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            var response = await _stkOutDetailRepositor.UpdateDetailByIdentity(detail, companyCode, user);
            if (response.ValidationSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

    }
}
