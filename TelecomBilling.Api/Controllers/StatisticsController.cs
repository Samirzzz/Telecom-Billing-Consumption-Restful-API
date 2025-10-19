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
    public class StatisticsController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IConsumptionService _consumptionService;

        public StatisticsController(IInvoiceService invoiceService, IConsumptionService consumptionService)
        {
            _invoiceService = invoiceService;
            _consumptionService = consumptionService;
        }

        /// <summary>
        /// Get top consumers by usage (Admin only)
        /// </summary>
        /// <param name="month">Month in YYYY-MM format (optional, defaults to current month)</param>
        /// <param name="limit">Number of top consumers to return (default: 10)</param>
        /// <param name="sortBy">Sort criteria: 'voice', 'data', 'sms', 'total' (default: 'total')</param>
        /// <returns>List of top consumers</returns>
        [HttpGet("top-consumers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetTopConsumers(
            [FromQuery] string? month = null,
            [FromQuery] int limit = 10,
            [FromQuery] string sortBy = "total",
            [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                if (limit <= 0 || limit > 100)
                {
                    return BadRequest(new { message = "Limit must be between 1 and 100" });
                }

                var validSortOptions = new[] { "voice", "data", "sms", "total" };
                if (!validSortOptions.Contains(sortBy.ToLower()))
                {
                    return BadRequest(new { message = "SortBy must be one of: voice, data, sms, total" });
                }

                var result = await _consumptionService.GetTopConsumersAsync(month, limit, sortBy);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving top consumers.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get revenue statistics (Admin only)
        /// </summary>
        /// <param name="month">Month in YYYY-MM format (optional, defaults to current month)</param>
        /// <param name="year">Year (optional, for yearly statistics)</param>
        /// <returns>Revenue statistics</returns>
        [HttpGet("revenue")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetRevenueStatistics(
            [FromQuery] string? month = null,
            [FromQuery] int? year = null,
            [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var result = await _invoiceService.GetRevenueStatisticsAsync(month, year);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving revenue statistics.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get usage statistics summary (Admin only)
        /// </summary>
        /// <param name="month">Month in YYYY-MM format (optional, defaults to current month)</param>
        /// <returns>Usage statistics summary</returns>
        [HttpGet("usage")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetUsageStatistics([FromQuery] string? month = null, [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var result = await _consumptionService.GetUsageStatisticsAsync(month);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving usage statistics.", details = ex.Message });
            }
        }
    }
}
