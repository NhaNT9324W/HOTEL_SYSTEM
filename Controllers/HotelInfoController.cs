using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    [ApiController]
    [Route("api/hotelinfo")]
    public class HotelInfoController : ControllerBase
    {
        private readonly IHotelInfoService _service;

        public HotelInfoController(IHotelInfoService service) => _service = service;

        // GET /api/hotelinfo
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var info = await _service.GetAsync();
            if (info == null) return NotFound(new { message = "Hotel information not found" });
            return Ok(info);
        }

        // PUT /api/hotelinfo
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateHotelInfoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.UpdateAsync(dto);
                return Ok(new { message = "Hotel information updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}