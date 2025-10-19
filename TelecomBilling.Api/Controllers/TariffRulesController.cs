using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Services;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TariffRulesController : ControllerBase
    {
        private readonly ITariffRuleService _tariffRuleService;

        public TariffRulesController(ITariffRuleService tariffRuleService)
        {
            _tariffRuleService = tariffRuleService;
        }

        /// <summary>
        /// Get list of all tariff rules
        /// </summary>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>List of tariff rules</returns>
        [HttpGet]
        public async Task<ActionResult<object>> GetTariffRules(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var result = await _tariffRuleService.GetTariffRulesAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get tariff rule by ID
        /// </summary>
        /// <param name="id">Tariff rule ID</param>
        /// <returns>Tariff rule details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTariffRule(int id, [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var tariffRule = await _tariffRuleService.GetTariffRuleAsync(id);
                
                if (tariffRule == null)
                {
                    return NotFound(new { message = "Tariff rule not found" });
                }

                return Ok(tariffRule);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get active tariff rule for a plan type
        /// </summary>
        /// <param name="planType">Plan type</param>
        /// <returns>Active tariff rule for the plan type</returns>
        [HttpGet("active/{planType}")]
        public async Task<ActionResult<object>> GetActiveTariffRule(string planType, [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var tariffRule = await _tariffRuleService.GetActiveTariffRuleAsync(planType);
                
                if (tariffRule == null)
                {
                    return NotFound(new { message = "No active tariff rule found for this plan type" });
                }

                return Ok(tariffRule);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new tariff rule (Admin only)
        /// </summary>
        /// <param name="request">Tariff rule creation request</param>
        /// <returns>Created tariff rule</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> CreateTariffRule([FromBody] TariffRuleRequest request, [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var result = await _tariffRuleService.CreateTariffRuleAsync(request);
                return CreatedAtAction(nameof(GetTariffRule), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing tariff rule (Admin only)
        /// </summary>
        /// <param name="id">Tariff rule ID</param>
        /// <param name="request">Updated tariff rule information</param>
        /// <returns>Updated tariff rule</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> UpdateTariffRule(int id, [FromBody] TariffRuleRequest request, [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var result = await _tariffRuleService.UpdateTariffRuleAsync(id, request);
                
                if (result == null)
                {
                    return NotFound(new { message = "Tariff rule not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a tariff rule (Admin only)
        /// </summary>
        /// <param name="id">Tariff rule ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteTariffRule(int id)
        {
            try
            {
                var success = await _tariffRuleService.DeleteTariffRuleAsync(id);
                
                if (!success)
                {
                    return NotFound(new { message = "Tariff rule not found" });
                }

                return Ok(new { message = "Tariff rule deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
