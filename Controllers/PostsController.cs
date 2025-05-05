using BloggerWebApi.Dto;
using BloggerWebApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/posts")]
[ApiController]
public class PostsController : ControllerBase
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
    public async Task<ActionResult<Post>> CreatePost(Post post)
    {
        var created = await postService.CreateAsync(post);
        return CreatedAtAction(nameof(GetPost), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(int id, Post post)
    {
        var updatedPost = await postService.UpdateAsync(id, post);
        if (updatedPost == null)
        {
            return NotFound();
        }
        return Ok(updatedPost);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var result = await postService.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
