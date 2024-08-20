using Azure;
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

        [HttpPost("InsertByModel")]
        public async Task<IActionResult> Create([FromBody] OM_UOM obj, string companyCode, string user)
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

            if (obj == null)
            {
                return BadRequest();
            }
            var response = await _uomreposotory.Insert(obj, companyCodeToUse, user);
           
            if (response.StatusCode == "201")
            {
                return StatusCode(int.Parse(response.StatusCode), response); // Returns the exact status code
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

        [HttpPut("{id}")]
        [Route("UpdateByModel")]
        public async Task<IActionResult> UpdateUom(string id,[FromBody] OM_UOM uom, string companyCode, string user)
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

            try
            {
                ////var existingUom = await _uomreposotory.GetById(id, companyCode, user);
                ////if (existingUom == null)
                ////{
                ////    return NotFound($"UOM with ID: {id} not found");
                ////}

                var response= await _uomreposotory.Update(uom, companyCodeToUse, user);
                ////return NoContent(); // Successful update with no content to return
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
            catch (Exception ex)
            {
                
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteByModel")]
        public async Task<IActionResult> Delete([FromBody] OM_UOM uom, string companyCode, string user)
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

            try
            {
                var response = await _uomreposotory.Delete(uom, companyCodeToUse, user);

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
            

            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);
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
            var response = await _uomreposotory.Search<OM_UOM>(
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
                return StatusCode(500, response);
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
            var response = await _uomreposotory.SearchCount(
                request.JsonModel, companyCodeToUse, request.User, request.WhereClause
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

        //Done by Aji on 02 Jul 4:38
    }
}
