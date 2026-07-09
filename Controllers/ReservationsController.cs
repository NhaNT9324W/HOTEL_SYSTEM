using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    [ApiController]
    [Route("api/reservations")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _service;

        public ReservationsController(IReservationService service) => _service = service;

        // GET /api/reservations
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // GET /api/reservations/search?keyword=...
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAll();

            var result = await _service.SearchAsync(keyword);
            return Ok(result);
        }

        // GET /api/reservations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Reservation not found" });
            return Ok(result);
        }

        // GET /api/reservations/available-rooms?checkIn=...&checkOut=...
        [HttpGet("available-rooms")]
        public async Task<IActionResult> GetAvailableRooms(
    [FromQuery] DateTime checkIn,
    [FromQuery] DateTime checkOut,
    [FromQuery] int? roomTypeId = null)
        {
            if (checkIn >= checkOut)
                return BadRequest(new { message = "Check-out date must be after check-in date" });

            var result = await _service.GetAvailableRoomsAsync(checkIn, checkOut, roomTypeId);
            return Ok(result);
        }

        // POST /api/reservations
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.CreateAsync(dto);
                return Ok(new { message = "Reservation created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/reservations/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new { message = "Reservation updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PATCH /api/reservations/{id}/cancel
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _service.CancelAsync(id);
                return Ok(new { message = "Reservation cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PATCH /api/reservations/{id}/checkin
        [HttpPatch("{id}/checkin")]
        public async Task<IActionResult> CheckIn(int id)
        {
            try
            {
                await _service.CheckInAsync(id);
                return Ok(new { message = "Check-in successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}