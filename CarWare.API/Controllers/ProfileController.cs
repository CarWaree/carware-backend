using CarWare.API.Errors;
using CarWare.API.Errors.NonGeneric;
using CarWare.Application.DTOs.Profile;
using CarWare.Application.Interfaces;
using CarWare.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarWare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // GET api/profile/{userId}
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            var result = await _profileService.GetProfileAsync(userId);

            if (!result.Success)
                return NotFound(ApiResponse.Fail(result.Error));

            return Ok(ApiResponseGeneric<EditProfileResponseDto>
                .Success(result.Data, "Profile retrieved successfully"));
        }

        // PUT api/profile/{userId}
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateProfile(string userId, [FromBody] UpdateProfileDto dto)
        {
            var result = await _profileService.UpdateProfileAsync(userId, dto);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(ApiResponse.Success(result.Data));
        }

        // POST api/profile/upload-image/{userId}
        [HttpPost("upload-image/{userId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(string userId, [FromForm] UploadImageDto dto)
        {
            var result = await _profileService.UploadImageAsync(userId, dto.File);

            if (!result.Success)
                return BadRequest(ApiResponse.Fail(result.Error));

            return Ok(ApiResponse.Success(result.Data));
        }
    }
}