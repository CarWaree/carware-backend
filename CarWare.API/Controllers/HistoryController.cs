using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarWare.API.Controllers
{
    [Route("api/history")]
    [ApiController]
    [Authorize]
    public class HistoryController : ControllerBase
    {
        private readonly IServiceRequestService _service;

        public HistoryController(IServiceRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _service.GetUserHistoryAsync(userId);

            if (!result.Success)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _service.GetUserHistoryDetailsAsync(id, userId);

            if (!result.Success)
                return NotFound(result.Error);

            return Ok(result.Data);
        }
    }
}