using BloggerWebApi.Dto;
using BloggerWebApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BloggerWebApi.Services
{
    public class PostService : IPostService
    {
        private readonly AppDbContext context;

        public PostService(AppDbContext context)
        {
            this.context = context;
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
            var date = DateTime.UtcNow;
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

            context.Posts.Remove(post);
            await context.SaveChangesAsync();
            return true;
        }
    }
}