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
    public class UsageController : ControllerBase
    {
        private readonly IConsumptionService _consumptionService;

        public UsageController(IConsumptionService consumptionService)
        {
            _consumptionService = consumptionService;
        }

        /// <summary>
        /// Delete usage record by ID (Admin only)
        /// </summary>
        /// <param name="id">Usage record ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUsage(int id)
        {
            try
            {
                var deleted = await _consumptionService.DeleteUsageRecordAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = "Usage record not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the usage record.", details = ex.Message });
            }
        }

        /// <summary>
        /// Create bulk usage records (Admin only)
        /// </summary>
        /// <param name="request">Bulk usage record request</param>
        /// <returns>Bulk creation result</returns>
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> CreateBulkUsage([FromBody] BulkUsageRecordRequest request, [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var result = await _consumptionService.CreateBulkUsageRecordsAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating bulk usage records.", details = ex.Message });
            }
        }
    }
}
