using Microsoft.AspNetCore.Mvc;
using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;

namespace Hotel_System.Controllers
{
    [ApiController]
    [Route("api/roomtypes")]
    public class RoomTypesController : ControllerBase
    {
        private readonly IRoomTypeService _service;
        public RoomTypesController(IRoomTypeService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoomTypeDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomTypeDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.SoftDeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}