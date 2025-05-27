using BloggerWebApi.Dto;
using BloggerWebApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloggerWebApi.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class PostsController : BaseController
    {
        private readonly IPostService postService;

        public PostsController(IPostService postService)
        {
            this.postService = postService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostPreviewDto>>> GetPosts()
        {
            var posts = await postService.GetAllAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await postService.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Post>> CreatePost(Post post)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var created = await postService.CreateAsync(post, userId);
            return CreatedAtAction(nameof(GetPost), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost(int id, Post post)
        {
            if (!await postService.IsOwnerOrAdminAsync(id, CurrentUserId))
            {
                return Forbid();
            }

            var updated = await postService.UpdateAsync(id, post);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id)
        {
            if (!await postService.IsOwnerOrAdminAsync(id, CurrentUserId))
            {
                return Forbid();
            }

            var success = await postService.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }

    }
}