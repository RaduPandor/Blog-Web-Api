using BloggerWebApi.Dto;
namespace BloggerWebApi.Interfaces;

public interface IPostService
{
    Task<IEnumerable<PostPreviewDto>> GetAllAsync();
    Task<FullPostDto?> GetByIdAsync(int id);
    Task<Post> CreateAsync(Post post, string userId);
    Task<Post?> UpdateAsync(int id, Post updatedPost);
    Task<bool> DeleteAsync(int id);
    Task<bool> IsOwnerOrAdminAsync(int id, string currentUserId);
}
