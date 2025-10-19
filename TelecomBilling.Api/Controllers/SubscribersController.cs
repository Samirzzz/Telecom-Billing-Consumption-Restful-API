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
    public class SubscribersController : ControllerBase
    {
        private readonly IAuthService _authService;

        public SubscribersController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Update subscriber plan (Admin only) - Users are subscribers in this system
        /// </summary>
        /// <param name="id">User/Subscriber ID</param>
        /// <param name="request">Plan update request</param>
        /// <returns>Updated subscriber information</returns>
        [HttpPut("{id}/plan")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> UpdateSubscriberPlan(int id, [FromBody] UpdatePlanRequest request, [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var result = await _authService.UpdateSubscriberPlanAsync(id, request);
                
                if (result == null)
                {
                    return NotFound(new { message = "Subscriber not found" });
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating subscriber plan.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get subscriber information (Users can view their own, Admins can view any)
        /// </summary>
        /// <param name="id">Subscriber ID</param>
        /// <returns>Subscriber information</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<object>> GetSubscriber(int id, [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userRole == UserRole.User.ToString() && id != currentUserId)
                {
                    return Forbid("Users can only view their own subscriber information.");
                }

                var result = await _authService.GetSubscriberAsync(id);
                
                if (result == null)
                {
                    return NotFound(new { message = "Subscriber not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving subscriber information.", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all subscribers (Admin only)
        /// </summary>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>List of subscribers</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetSubscribers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var result = await _authService.GetSubscribersAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving subscribers.", details = ex.Message });
            }
        }

        /// <summary>
        /// Update subscriber information (Admin only)
        /// </summary>
        /// <param name="id">Subscriber ID</param>
        /// <param name="request">Subscriber update request</param>
        /// <returns>Updated subscriber information</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> UpdateSubscriber(int id, [FromBody] UpdateSubscriberRequest request, [FromQuery] ResponseFormat responseFormat = ResponseFormat.Json)
        {
            try
            {
                var result = await _authService.UpdateSubscriberAsync(id, request);
                
                if (result == null)
                {
                    return NotFound(new { message = "Subscriber not found" });
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating subscriber.", details = ex.Message });
            }
        }

        /// <summary>
        /// Deactivate subscriber (Admin only)
        /// </summary>
        /// <param name="id">Subscriber ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeactivateSubscriber(int id)
        {
            try
            {
                var success = await _authService.DeactivateSubscriberAsync(id);
                
                if (!success)
                {
                    return NotFound(new { message = "Subscriber not found" });
                }

                return Ok(new { message = "Subscriber deactivated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deactivating subscriber.", details = ex.Message });
            }
        }
    }
}
