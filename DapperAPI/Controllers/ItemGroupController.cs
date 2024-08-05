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

    public class ItemGroupController : ControllerBase
    {
        private readonly IOneRepository<OM_ITEM_GROUP> _itemGroupReposotory;
        private readonly IUserValidationService _userValidationService;
        public ItemGroupController(IOneRepository<OM_ITEM_GROUP> itemgrouprepository, IUserValidationService userValidationService)
        {
            _itemGroupReposotory = itemgrouprepository;
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
            var suppitem = await _itemGroupReposotory.GetAll(companyCode, user);
            return Ok(suppitem);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, string companyCode, string user)
        {
            var entity = await _itemGroupReposotory.GetById(id, companyCode, user);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OM_ITEM_GROUP obj, string companyCode, string user)
        {
            if (obj == null)
            {
                return BadRequest();
            }
            var suppitem = await _itemGroupReposotory.Insert(obj, companyCode, user);
            return Ok(suppitem);
        }

        [HttpPut]
        public async Task<IActionResult> Update(string id, [FromBody] OM_ITEM_GROUP itemgroup, string companyCode, string user)
        {
            if (id != itemgroup.IG_CODE)
            {
                return BadRequest("ID mismatch in request");
            }

            try
            {
                var existingitem = await _itemGroupReposotory.GetById(id, companyCode, user);
                if (existingitem == null)
                {
                    return NotFound($"Item with ID: {id} not found");
                }

                await _itemGroupReposotory.Update(itemgroup, companyCode, user);
                return NoContent(); // Successful update with no content to return
            }
            catch (Exception ex)
            {

                return StatusCode(400, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, string companyCode, string user)
        {
            try
            {
                if (!await ValidateUserAndCompany(user, companyCode))
                {
                    return Unauthorized("User validation failed.");
                }
                var itemgroup = await _itemGroupReposotory.GetById(id, companyCode, user);
                if (itemgroup == null)
                {
                    return NotFound($"Item with ID: {id} not found");
                }

                await _itemGroupReposotory.Delete(id, companyCode, user);
                return NoContent(); // Successful deletion with no content to return
            }
            catch (Exception ex)
            {

                return StatusCode(400, ex.Message);
            }
        }


    }
}
