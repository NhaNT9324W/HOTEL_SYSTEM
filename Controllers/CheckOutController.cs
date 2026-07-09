using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    [ApiController]
    [Route("api/checkout")]
    public class CheckOutController : ControllerBase
    {
        private readonly ICheckOutService _service;

        public CheckOutController(ICheckOutService service) => _service = service;

        // GET /api/checkout/{reservationId}/services
        [HttpGet("{reservationId}/services")]
        public async Task<IActionResult> GetServices(int reservationId)
        {
            var result = await _service.GetServiceUsagesAsync(reservationId);
            return Ok(result);
        }

        // POST /api/checkout/services
        [HttpPost("services")]
        public async Task<IActionResult> AddService([FromBody] AddServiceUsageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.AddServiceUsageAsync(dto);
                return Ok(new { message = "Service added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/checkout/services/{id}
        [HttpDelete("services/{id}")]
        public async Task<IActionResult> RemoveService(int id)
        {
            try
            {
                await _service.RemoveServiceUsageAsync(id);
                return Ok(new { message = "Service removed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/checkout/{reservationId}/preview
        [HttpGet("{reservationId}/preview")]
        public async Task<IActionResult> PreviewInvoice(int reservationId)
        {
            try
            {
                var result = await _service.PreviewInvoiceAsync(reservationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/checkout/{reservationId}/confirm
        [HttpPost("{reservationId}/confirm")]
        public async Task<IActionResult> CheckOut(int reservationId)
        {
            try
            {
                var result = await _service.CheckOutAsync(reservationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}