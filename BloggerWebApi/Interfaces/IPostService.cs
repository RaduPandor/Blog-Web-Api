using BloggerWebApi.Dto;
namespace BloggerWebApi.Interfaces;

public interface IPostService
{
    Task<IEnumerable<PostPreviewDto>> GetAllAsync();
    Task<Post?> GetByIdAsync(int id);
    Task<Post> CreateAsync(Post post);
    Task<Post?> UpdateAsync(int id, Post updatedPost);
    Task<bool> DeleteAsync(int id);
}
