using Azure;
using DapperAPI.EntityModel;
using DapperAPI.Interface;
using DapperAPI.Repository;
using DapperAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DapperAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly ITwoRepository<OM_ITEM,OM_ITEM_UOM> _itemRepositor;
        private readonly IUserValidationService _userValidationService;
        public ItemController(ITwoRepository<OM_ITEM,OM_ITEM_UOM> itemRepositor, IUserValidationService userValidationService) 
        {
            _itemRepositor = itemRepositor;
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
        private async Task<IActionResult> GetAll(string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            var userType = await _userValidationService.GetUserTypeAsync(user);

            if (userType == "CLIENT")
            {
                var response = await _itemRepositor.GetAll(companyCode, user);
                return Ok(response);
            }
            else
            {
                var response = await _itemRepositor.GetAll(companyCode, user);
                return Ok(response);
            }

                
        }
        [HttpGet("GetById")]
        private async Task<IActionResult> GetById(string id, string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            var userType = await _userValidationService.GetUserTypeAsync(user);

            if (userType == "CLIENT")
            {
                var response = await _itemRepositor.GetById(id, companyCode, user);
                if (response == null)
                {
                    return NotFound();
                }
                return Ok(response);
            }
            else
            {
                var response = await _itemRepositor.GetById(id, companyCode, user);
                if (response == null)
                {
                    return NotFound();
                }
                return Ok(response);
            }

                
        }

        [HttpPost("InsertByModel")]
        public async Task<IActionResult> Create([FromBody] OM_ITEM item, string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            if (item == null)
            {
                return BadRequest("Item is null.");
            }

            var response = await _itemRepositor.Insert(item, companyCode, user);
            Log.Information("ITEM INSERT NORMAL = {@result}", response);

            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            else if (response.StatusCode == "404")
            {
                return NotFound(response);
            }
            else if (response.StatusCode == "400")
            {
                return BadRequest(response);
            }
            else
            {
                return StatusCode(500, response);
                //return StatusCode(int.Parse(response.StatusCode), response); // Returns the exact status code
            }
        }

        [HttpPut("UpdateByModel")]
        public async Task<IActionResult> Update([FromBody] OM_ITEM item, string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            if (item == null)
            {
                return BadRequest("Item is null.");
            }

            var userType = await _userValidationService.GetUserTypeAsync(user);
            string companyCodeToUse = companyCode;
            if (userType == "OPERATOR" && companyCode == "ALL")
            {
                companyCodeToUse = null;
            }
            var response =await _itemRepositor.Update(item, companyCodeToUse, user);

            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            else if (response.StatusCode == "404")
            {
                return NotFound(response);
            }
            else if (response.StatusCode == "400")
            {
                return BadRequest(response);
            }
            else
            {
                return StatusCode(500, response);
                //return StatusCode(int.Parse(response.StatusCode), response); // Returns the exact status code
            }
        }

        [HttpDelete]
        [Route("DeleteByModel")]
        public async Task<IActionResult> Delete([FromBody] OM_ITEM item, string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            var userType = await _userValidationService.GetUserTypeAsync(user);
            string companyCodeToUse = companyCode;
            if (userType == "OPERATOR" && companyCode == "ALL")
            {
                companyCodeToUse = null;
            }

            var response = await _itemRepositor.Delete(item, companyCodeToUse, user);

            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            else if (response.StatusCode == "404")
            {
                return NotFound(response);
            }
            else if (response.StatusCode == "400")
            {
                return BadRequest(response);
            }
            else
            {
                return StatusCode(500, response);
                //return StatusCode(int.Parse(response.StatusCode), response); // Returns the exact status code
            }
        }

        
        

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
        {
            
            if (!await ValidateUserAndCompany(request.User, request.CompanyCode))
            {
                return Unauthorized("User validation failed.");
            }
            var userType = await _userValidationService.GetUserTypeAsync(request.User);
            string companyCodeToUse = request.CompanyCode;
            if (userType == "OPERATOR" && request.CompanyCode == "ALL")
            {
                companyCodeToUse = null;
            }
            // Call the search method from your service layer
            var response = await _itemRepositor.Search<OM_ITEM>(
                request.JsonModel, request.SortBy, request.PageNo, request.PageSize,
                companyCodeToUse, request.User, request.WhereClause, request.ShowDetail
            );

            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            else if (response.StatusCode == "404")
            {
                return NotFound(response);
            }
            else if (response.StatusCode == "400")
            {
                return BadRequest(response);
            }
            else
            {
                return StatusCode(500,response);
                //return StatusCode(int.Parse(response.StatusCode), response); // Returns the exact status code
            }
        }

        [HttpGet("searchcount")]
        public async Task<IActionResult> SearchCount([FromBody] SearchRequest request)
        {

            if (!await ValidateUserAndCompany(request.User, request.CompanyCode))
            {
                return Unauthorized("User validation failed.");
            }
            var userType = await _userValidationService.GetUserTypeAsync(request.User);
            string companyCodeToUse = request.CompanyCode;
            if (userType == "OPERATOR" && request.CompanyCode == "ALL")
            {
                companyCodeToUse = null;
            }
            // Call the search method from your service layer
            var response = await _itemRepositor.SearchCount(
                request.JsonModel,companyCodeToUse, request.User, request.WhereClause
            );

            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            else if (response.StatusCode == "404")
            {
                return NotFound(response);
            }
            else if (response.StatusCode == "400")
            {
                return BadRequest(response);
            }
            else
            {
                return StatusCode(500, response);
                //return StatusCode(int.Parse(response.StatusCode), response); // Returns the exact status code
            }
        }



        [HttpPost("ImportItemMaster")]
        public async Task<CommonResponse<object>> Import([FromBody] List<OM_ITEM> item, string companyCode, string user, string result)
        {
            CommonResponse<object> response = new CommonResponse<object>();

            if (!await ValidateUserAndCompany(user, companyCode))
            {
                response.ErrorString = "User validation failed.";
                response.StatusCode = "400";
                response.ValidationSuccess = false;
                return response;
            }

            if (item == null)
            {
                response.ErrorString = "Item is null.";
                response.StatusCode = "400";
                response.ValidationSuccess = false;
                return response;
            }

            response = await _itemRepositor.Import(item, companyCode, user, result);
            return response;
        }



        ////[HttpPut("UpdateDetail")]

        ////public async Task<IActionResult> UpdateDetail([FromBody] OM_ITEM detail, string companyCode, string user)
        ////{
        ////    if (!await ValidateUserAndCompany(user, companyCode))
        ////    {
        ////        return Unauthorized("User validation failed.");
        ////    }

        ////    var response = await _itemRepositor.UpdateDetail(detail, companyCode, user);
        ////    if (response.ValidationSuccess)
        ////    {
        ////        return Ok(response.ReturnCompleteRow);
        ////    }
        ////    return BadRequest(response);
        ////}
    }
}
