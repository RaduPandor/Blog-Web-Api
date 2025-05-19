using BloggerWebApi.Dto;
using BloggerWebApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace BloggerWebApi.Services;

public class PostService : IPostService
{
    private readonly AppDbContext context;
    private readonly IHttpContextAccessor httpContextAccessor;

    public PostService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        this.context = context;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<PostPreviewDto>> GetAllAsync()
    {
        return await context.Posts
            .OrderByDescending(post => post.CreatedDate)
            .Select(post => new PostPreviewDto
            {
                Id = post.Id,
                Title = post.Title,
                Author = post.Author,
                ContentPreview = post.Content.Length > 20 ? post.Content.Substring(0, 20) + "..." : post.Content,
                CreatedDate = post.CreatedDate,
                LastModifiedDate = post.LastModifiedDate
            })
            .ToListAsync();
    }

    public async Task<Post?> GetByIdAsync(int id)
    {
        return await context.Posts.FindAsync(id);
    }

    public async Task<Post> CreateAsync(Post post, string userId)
    {
        DateTime date = DateTime.UtcNow;
        post.CreatedDate = date;
        post.LastModifiedDate = date;
        post.UserId = userId;
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        return post;
    }

    public async Task<Post?> UpdateAsync(int id, Post updatedPost, string userId)
    {
        var existingPost = await context.Posts.FindAsync(id);
        if (existingPost == null)
        {
            return null;
        }

        if (existingPost.UserId != userId && !httpContextAccessor.HttpContext.User.IsInRole("Admin"))
        {
            return null;
        }

        existingPost.Title = updatedPost.Title;
        existingPost.Author = updatedPost.Author;
        existingPost.Content = updatedPost.Content;
        existingPost.LastModifiedDate = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return existingPost;
    }



    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var post = await context.Posts.FindAsync(id);
        if (post == null)
        {
            return false;
        }

        var currentUser = httpContextAccessor.HttpContext.User;
        var isAdmin = currentUser.IsInRole("Admin");
        if (post.UserId != userId && !isAdmin)
        {
            return false;
        }        

        context.Posts.Remove(post);
        await context.SaveChangesAsync();
        return true;
    }
}
