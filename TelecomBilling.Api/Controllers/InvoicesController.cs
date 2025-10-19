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
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        // Admin can create invoices, Users cannot directly create invoices
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> CreateInvoice([FromBody] InvoiceRequest request, [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var invoice = await _invoiceService.CreateInvoiceAsync(request);
                return CreatedAtAction(nameof(GetInvoice), new { userId = invoice.UserId, month = invoice.Month }, invoice);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the invoice.", details = ex.Message });
            }
        }

        // User can get their own invoices, Admin can get any
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<object>> GetInvoices(
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
                    // Users can only view their own invoices
                    if (userId.HasValue && userId.Value != currentUserId)
                    {
                        return Forbid("Users can only view their own invoices.");
                    }
                    userId = currentUserId; // Ensure user views their own data
                }
                // Admin can view any user's invoices or all if userId is null

                var result = await _invoiceService.GetInvoicesWithFormatAsync(userId, responseFormat, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving invoices.", details = ex.Message });
            }
        }

        // User can get their own specific invoice, Admin can get any
        [HttpGet("{userId}/{month}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<object>> GetInvoice(
            int userId,
            string month,
            [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userRole == UserRole.User.ToString() && userId != currentUserId)
                {
                    return Forbid("Users can only view their own invoices.");
                }

                var result = await _invoiceService.GetInvoiceWithFormatAsync(userId, month, responseFormat);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the invoice.", details = ex.Message });
            }
        }

        // Admin can update invoices
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> UpdateInvoice(int id, [FromBody] InvoiceRequest request, [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var updatedInvoice = await _invoiceService.UpdateInvoiceAsync(id, request);
                if (updatedInvoice == null)
                {
                    return NotFound(new { message = "Invoice not found." });
                }
                return Ok(updatedInvoice);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the invoice.", details = ex.Message });
            }
        }

        // Admin can delete invoices
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteInvoice(int id)
        {
            try
            {
                var deleted = await _invoiceService.DeleteInvoiceAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = "Invoice not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the invoice.", details = ex.Message });
            }
        }
    }
}