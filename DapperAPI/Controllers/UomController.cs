using DapperAPI.EntityModel;
using DapperAPI.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DapperAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UomController : ControllerBase
    {
        private readonly IOneRepository<OM_UOM> _uomreposotory;
        private readonly IUserValidationService _userValidationService;
        public UomController(IOneRepository<OM_UOM> uomrepository, IUserValidationService userValidationService)
        {
            _uomreposotory = uomrepository;
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

            var response = await _uomreposotory.GetAll(companyCodeToUse, user);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, string companyCode, string user)
        {
            var entity = await _uomreposotory.GetById(id, companyCode, user);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OM_UOM obj, string companyCode, string user)
        {
            if (obj == null)
            {
                return BadRequest();
            }
            var uom = await _uomreposotory.Insert(obj, companyCode, user);
            return Ok(uom);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUom(string id, [FromBody] OM_UOM uom, string companyCode, string user)
        {
            if (id != uom.UOM_CODE)
            {
                return BadRequest("ID mismatch in request");
            }

            try
            {
                var existingUom = await _uomreposotory.GetById(id, companyCode, user);
                if (existingUom == null)
                {
                    return NotFound($"UOM with ID: {id} not found");
                }

                await _uomreposotory.Update(uom, companyCode, user);
                return NoContent(); // Successful update with no content to return
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, ex.Message);
            }
        }

        //Done by Aji on 02 Jul 4:38
    }
}
