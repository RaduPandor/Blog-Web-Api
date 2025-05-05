using Microsoft.AspNetCore.Identity;
public interface IUserService
{
    Task<IdentityResult> RegisterUserAsync(string username, string password);
    Task<IdentityUser?> ValidateUserAsync(string username, string password);
    Task SignOutAsync();
    Task<IdentityResult> CreateUserAsync(string username, string password);
    Task<IdentityUser?> GetUserByIdAsync(string id);
    Task<IEnumerable<IdentityUser>> GetAllUsersAsync();
    Task<IdentityResult> UpdateUserAsync(string id, string newUsername);
    Task<IdentityResult> DeleteUserAsync(string id);
}