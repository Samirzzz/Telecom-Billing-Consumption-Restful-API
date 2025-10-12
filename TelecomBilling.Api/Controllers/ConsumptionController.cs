using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;
using TelecomBilling.Api.Services;

namespace TelecomBilling.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConsumptionController : ControllerBase
    {
        private readonly IConsumptionService _consumptionService;

        public ConsumptionController(IConsumptionService consumptionService)
        {
            _consumptionService = consumptionService;
        }

        // Admin can create usage records
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UsageRecordResponse>> CreateUsageRecord([FromBody] UsageRecordRequest request)
        {
            try
            {
                var usageRecord = await _consumptionService.CreateUsageRecordAsync(request);
                return CreatedAtAction(nameof(GetConsumption), new { userId = usageRecord.UserId }, usageRecord);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the usage record.", details = ex.Message });
            }
        }

        // User can get their own consumption, Admin can get any
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<object>> GetConsumption(
            [FromQuery] int? userId = null,
            [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userRole == UserRole.User.ToString())
                {
                    // Users can only view their own consumption
                    if (userId.HasValue && userId.Value != currentUserId)
                    {
                        return Forbid("Users can only view their own consumption records.");
                    }
                    userId = currentUserId; // Ensure user views their own data
                }
                // Admin can view any user's consumption or all if userId is null

                if (!userId.HasValue || userId.Value == 0)
                {
                    return BadRequest(new { message = "UserId is required for consumption retrieval." });
                }

                var result = await _consumptionService.GetUsageRecordsWithFormatAsync(userId.Value, responseFormat, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving consumption records.", details = ex.Message });
            }
        }

        // Admin can delete usage records
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUsageRecord(int id)
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
    }
}