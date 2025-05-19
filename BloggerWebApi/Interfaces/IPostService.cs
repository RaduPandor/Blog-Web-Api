using BloggerWebApi.Dto;
namespace BloggerWebApi.Interfaces;

public interface IPostService
{
    Task<IEnumerable<PostPreviewDto>> GetAllAsync();
    Task<Post?> GetByIdAsync(int id);
    Task<Post> CreateAsync(Post post, string userId);
    Task<Post?> UpdateAsync(int id, Post updatedPost, string userId);
    Task<bool> DeleteAsync(int id, string userId);
}
