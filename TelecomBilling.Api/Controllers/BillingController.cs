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
    public class BillingController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public BillingController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Get billing information for a specific subscriber and month
        /// </summary>
        /// <param name="subscriberId">Subscriber ID</param>
        /// <param name="month">Month in YYYY-MM format</param>
        /// <param name="responseFormat">Response format (JSON or SOAP)</param>
        /// <returns>Billing information</returns>
        [HttpGet("{subscriberId}/{month}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<object>> GetBilling(
            int subscriberId,
            string month,
            [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userRole == UserRole.User.ToString() && subscriberId != currentUserId)
                {
                    return Forbid("Users can only view their own billing information.");
                }

                var result = await _invoiceService.GetInvoiceWithFormatAsync(subscriberId, month, responseFormat);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving billing information.", details = ex.Message });
            }
        }
    }
}
