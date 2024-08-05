using Azure;
using Azure.Core;
using DapperAPI.EntityModel;
using DapperAPI.Interface;
using DapperAPI.Repository;
using DapperAPI.Services;
using Microsoft.AspNetCore.Cors;

//using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DapperAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class SupplierItemController : ControllerBase
    {
        private readonly IOneRepository<OM_SUPP_ITEM> _suppItemReposotory;
        private readonly IUserValidationService _userValidationService;
        public SupplierItemController(IOneRepository<OM_SUPP_ITEM> suppitemrepository, IUserValidationService userValidationService)
        {
            _suppItemReposotory = suppitemrepository;
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
        public async Task<IActionResult> Get(string companyCode, string user)
        {
            var suppitem = await _suppItemReposotory.GetAll(companyCode, user);
            return Ok(suppitem);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, string companyCode, string user)
        {
            var entity = await _suppItemReposotory.GetById(id, companyCode, user);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OM_SUPP_ITEM obj, string companyCode, string user)
        {
            if (obj == null)
            {
                return BadRequest();
            }
            var suppitem = await _suppItemReposotory.Insert(obj, companyCode, user);
            return Ok(suppitem);
        }

        [HttpPut]
        public async Task<IActionResult> Update(string id, [FromBody] OM_SUPP_ITEM suppitem, string companyCode, string user)
        {
            if (id != suppitem.SI_SUPP_CODE)
            {
                return BadRequest("ID mismatch in request");
            }

            try
            {
                var existingitem = await _suppItemReposotory.GetById(id, companyCode, user);
                if (existingitem == null)
                {
                    return NotFound($"Item with ID: {id} not found");
                }

                await _suppItemReposotory.Update(suppitem, companyCode, user);
                return NoContent(); // Successful update with no content to return
            }
            catch (Exception ex)
            {

                return StatusCode(400, ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteByModel")]
        public async Task<IActionResult> Delete([FromBody] OM_SUPP_ITEM item, string companyCode, string user)
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

            var response = await _suppItemReposotory.Delete(item, companyCodeToUse, user);
            return Ok(response);
        }


    }
}
