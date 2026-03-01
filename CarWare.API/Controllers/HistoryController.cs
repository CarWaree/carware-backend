using CarWare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? status,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _historyService
                .GetAllAsync(userId, status, pageNumber, pageSize);

            if (!result.Success)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _historyService.GetByIdAsync(id);

            if (!result.Success)
                return NotFound(result.Error);

            return Ok(result.Data);
        }
    }
}