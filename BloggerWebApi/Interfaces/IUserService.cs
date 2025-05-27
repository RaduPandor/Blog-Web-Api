using BloggerWebApi.Entities;
using Microsoft.AspNetCore.Identity;
public interface IUserService
{
    Task<IdentityResult> RegisterUserAsync(string username, string password, string? displayName = null);
    Task<ApplicationUser?> ValidateUserAsync(string username, string password);
    Task SignOutAsync();
    Task<IdentityResult> CreateUserAsync(string username, string password, string? displayName = null);
    Task<ApplicationUser?> GetUserByIdAsync(string id);
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    Task<IdentityResult> UpdateUserAsync(string id, string? username = null, string? displayName = null);
    Task<IdentityResult> UpdateDisplayNameAsync(string id, string displayName);
    Task<IdentityResult> DeleteUserAsync(string id);
    Task<IdentityResult> UpdateUserRoleAsync(string id, string? role);
    Task<string> GetUserRoleAsync(string userId);
    Task<ApplicationUser?> GetUserByUsernameAsync(string username);
    Task<IdentityResult> AssignRoleAsync(ApplicationUser user, string role);
    Task<IdentityResult> ChangePasswordAsync(string userId, string newPassword);
    Task<IdentityResult> UpdateUsernameAsync(string userId, string newUsername);
    Task<bool> IsUserAdminAsync(string userId);
}