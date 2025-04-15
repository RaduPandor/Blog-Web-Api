using BloggerWebApi.Dto;
using BloggerWebApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BloggerWebApi.Services;

public class PostService(AppDbContext context) : IPostService
{
    private readonly AppDbContext context = context;

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

    public async Task<Post> CreateAsync(Post post)
    {
        DateTime date = DateTime.UtcNow;
        post.CreatedDate = date;
        post.LastModifiedDate = date;
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        return post;
    }

    public async Task<Post?> UpdateAsync(int id, Post updatedPost)
    {
        var existingPost = await context.Posts.FindAsync(id);
        if (existingPost == null)
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



    public async Task<bool> DeleteAsync(int id)
    {
        var post = await context.Posts.FindAsync(id);
        if (post == null){
            return false;
        }
            
        context.Posts.Remove(post);
        await context.SaveChangesAsync();
        return true;
    }
}
