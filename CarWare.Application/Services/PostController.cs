using BlogSystemAPI.DTO;
using BlogSystemAPI.Models;
using BlogSystemAPI.Services;
using BlogSystemAPI.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        PostService service;
        public PostController(UnitWork unit, PostService service)
        {
            this.service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<BlogPostDTO> Posts = service.GetAll();
            return Ok(Posts);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            BlogPostDTO post =  service.GetById(id);
            return Ok(post);
        }

        [HttpPost]
        public IActionResult Add([FromBody]BlogPostDTO PDTO)
        {
            if (PDTO == null) return BadRequest("Blog Post can not be null");
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var created =  service.Add(PDTO);
            if (created == null)
                return BadRequest(new { Message = "Failed to create blog post" });

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public IActionResult Update([FromBody]BlogPostDTO PDTO, int id)
        {;
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var Updated = service.Update(PDTO);
            if (Updated == "NotFound")
                return NotFound();
            if (Updated == "NoChanges")
                return Ok("No Changes were made to this post");

            return Ok("Post Updated Successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var deleted = await service.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}