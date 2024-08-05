using DapperAPI.EntityModel;
using DapperAPI.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DapperAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StkOutController : ControllerBase
    {
        private readonly IMasterDetailRepository<WT_STK_OUT_HEAD, WT_STK_OUT_ITEM> _stkOutRepositor;
        private readonly IUserValidationService _userValidationService;

        public StkOutController(IMasterDetailRepository<WT_STK_OUT_HEAD, WT_STK_OUT_ITEM> stkOutRepositor, IUserValidationService userValidationService)
        {
            _stkOutRepositor = stkOutRepositor;
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

        [HttpGet]
        public async Task<IActionResult> GetAll(string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            var userType = await _userValidationService.GetUserTypeAsync(user);

            // Call GetAll with the appropriate companyCode parameter
            var stkOut = await _stkOutRepositor.GetAll(userType == "CLIENT" ? companyCode : null, user);

            Log.Information("Get All = {@result}", stkOut);

            return Ok(stkOut);


        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetById(string id, string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            var userType = await _userValidationService.GetUserTypeAsync(user);
            var item = await _stkOutRepositor.GetById(id,userType == "CLIENT" ? companyCode : null, user);
            return Ok(item);            


        }

        [HttpPost]
        [Route("CreateByIdentity")]
        public async Task<IActionResult> Create([FromBody] WT_STK_OUT_HEAD stkOut, string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            if (stkOut == null)
            {
                return BadRequest("Stock Out is null.");
            }

            //var userType = await _userValidationService.GetUserTypeAsync(user);

            //if (userType == "CLIENT")
            //{
                var response = await _stkOutRepositor.InsertBySeq(stkOut, companyCode, user);
            Log.Information("STK INSERT BY SEQ = {@result}", response);
            return Ok(response);
            //}
           // else
            //{
                //var response = await _stkOutRepositor.InsertByIdentity(stkOut, companyCode, user);
                //return Ok(response);
            //}

            
        }

        [HttpPut]
        [Route("Update")]
        public async Task<IActionResult> Update([FromBody] WT_STK_OUT_HEAD stkOut, string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            if (stkOut == null)
            {
                return BadRequest("Item is null.");
            }

            var userType = await _userValidationService.GetUserTypeAsync(user);
            string companyCodeToUse = companyCode;
            if (userType == "OPERATOR" && companyCode == "ALL")
            {
                companyCodeToUse = null;
            }


            var response = await _stkOutRepositor.Update(stkOut, companyCodeToUse, user);
                return Ok(response);
            
             

        }

        [HttpPut("UpdateDetail")]
        
        public async Task<IActionResult> UpdateDetail([FromBody] WT_STK_OUT_ITEM detail, string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            var response = await _stkOutRepositor.UpdateDetail(detail, companyCode, user);
            if (response.ValidationSuccess)
            {
                return Ok(response.ReturnCompleteRow);
            }
            return BadRequest(response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
        {
            if (!await ValidateUserAndCompany(request.User, request.CompanyCode))
            {
                return Unauthorized("User validation failed.");
            }
            // Call the search method from your service layer
            var response = await _stkOutRepositor.Search<WT_STK_OUT_HEAD>(
                request.JsonModel,request.SortBy, request.PageNo, request.PageSize,
                request.CompanyCode, request.User, request.WhereClause, request.ShowDetail
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
    }
}
