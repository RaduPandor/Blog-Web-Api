using BloggerWebApi.Dto;
using BloggerWebApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace BloggerWebApi.Services
{
    public class PostService : IPostService
    {
        private readonly AppDbContext context;
        private readonly IUserService userService;

        public PostService(AppDbContext context, IUserService userService)
        {
            this.context = context;
            this.userService = userService;
        }

        public async Task<IEnumerable<PostPreviewDto>> GetAllAsync()
        {
            return await context.Posts
                .Include(post => post.AuthorUser)
                .OrderByDescending(post => post.CreatedDate)
                .Select(post => new PostPreviewDto
                {
                    Id = post.Id,
                    Title = post.Title,
                    Author = post.AuthorUser != null ? post.AuthorUser.DisplayName : post.Author,
                    ContentPreview = post.Content.Length > 150 ? post.Content.Substring(0, 150) + "..." : post.Content,
                    CreatedDate = post.CreatedDate,
                    LastModifiedDate = post.LastModifiedDate
                })
                .ToListAsync();
        }

        public async Task<FullPostDto?> GetByIdAsync(int id)
        {
            return await context.Posts
              .Where(p => p.Id == id)
              .Include(p => p.AuthorUser)
              .Select(p => new FullPostDto
              {
                  Id = p.Id,
                  Title = p.Title,
                  Content = p.Content,
                  Author = p.AuthorUser!.DisplayName,
                  AuthorId = p.AuthorUser.Id,
                  CreatedDate = p.CreatedDate,
                  LastModifiedDate = p.LastModifiedDate
              })
              .FirstOrDefaultAsync();
        }


        public async Task<Post> CreateAsync(Post post, string userId)
        {
            var date = DateTime.UtcNow;
            post.CreatedDate = date;
            post.LastModifiedDate = date;
            post.Author = userId;
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
            existingPost.Content = updatedPost.Content;
            existingPost.LastModifiedDate = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return existingPost;
        }

        public async Task<bool> DeleteAsync(int id)
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

        public async Task<bool> IsOwnerOrAdminAsync(int postId, string userId)
        {
            var post = await GetByIdAsync(postId);
            if (post == null)
            {
                return false;
            }
            return post.AuthorId == userId || await userService.IsUserAdminAsync(userId);
        }
    }
}