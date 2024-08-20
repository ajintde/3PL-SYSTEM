using DapperAPI.EntityModel;
using DapperAPI.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DapperAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemUomController : ControllerBase
    {
        private readonly ITwoRepository<OM_ITEM, OM_ITEM_UOM> _itemUomRepositor;
        private readonly IUserValidationService _userValidationService;

        public ItemUomController(ITwoRepository<OM_ITEM, OM_ITEM_UOM> itemUomRepositor, IUserValidationService userValidationService)
        {
            _itemUomRepositor = itemUomRepositor;
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

        [HttpPost("InsertDetailByModel")]
        public async Task<IActionResult> Create([FromBody] OM_ITEM_UOM detail, string companyCode, string user)
        {
            if (!await ValidateUserAndCompany(user, companyCode))
            {
                return Unauthorized("User validation failed.");
            }

            if (detail == null)
            {
                return BadRequest("Item is null.");
            }

            var userType = await _userValidationService.GetUserTypeAsync(user);
            string companyCodeToUse = companyCode;
            if (userType == "OPERATOR" && companyCode == "ALL")
            {
                companyCodeToUse = null;
            }
            var response = await _itemUomRepositor.InsertDetail(detail, companyCodeToUse, user);
            Log.Information("ITEM INSERT NORMAL = {@result}", response);
            return Ok(response);
        }

        [HttpPut("UpdateDetail")]
        public async Task<IActionResult> UpdateDetail([FromBody] OM_ITEM_UOM detail, string companyCode, string user)
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

            var response = await _itemUomRepositor.UpdateDetail(detail, null, user);
            if (response.ValidationSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpDelete]
        [Route("DeleteDetailByModel")]
        public async Task<IActionResult> DeleteDetailByModel([FromBody] OM_ITEM_UOM item, string companyCode, string user)
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

            var response = await _itemUomRepositor.DeleteDetail(item, companyCodeToUse, user);
            return Ok(response);
        }

        

       

        


    }
}
