using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/posts")]
[ApiController]
public class PostsController : ControllerBase
{
    private readonly AppDbContext context;

    public PostsController(AppDbContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
    {
        return await context.Posts.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Post>> CreatePost(Post post)
    {
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPosts), new { id = post.Id }, post);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id){
        var post = await context.Posts.FindAsync(id);
        if(post == null){
            return NotFound();
        }
        context.Posts.Remove(post); 
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(int id, Post updatedPost)
    {
        var existingPost = await context.Posts.FindAsync(id);
        if (existingPost == null)
        {
            return NotFound();
        }

        existingPost.Title = updatedPost.Title;
        existingPost.Author = updatedPost.Author;
        existingPost.Content = updatedPost.Content;
        existingPost.LastModifiedDate = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return NoContent();
    }
}
