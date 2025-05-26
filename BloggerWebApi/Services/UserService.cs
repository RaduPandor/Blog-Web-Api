using BloggerWebApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;

    public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    public async Task<IdentityResult> RegisterUserAsync(string username, string password, string? displayName = null)
    {
        var user = new ApplicationUser
        {
            UserName = username,
            DisplayName = displayName ?? username
        };
        return await userManager.CreateAsync(user, password);
    }

    public async Task<ApplicationUser?> ValidateUserAsync(string username, string password)
    {
        var result = await signInManager.PasswordSignInAsync(username, password, false, false);
        if (!result.Succeeded)
        {
            return null;
        }

        var user = await userManager.FindByNameAsync(username);
        if (user != null)
        {
            await userManager.UpdateAsync(user);
        }

        return user;
    }

    public async Task SignOutAsync()
    {
        await signInManager.SignOutAsync();
    }

    public async Task<IdentityResult> CreateUserAsync(string username, string password, string? displayName = null)
    {
        var user = new ApplicationUser
        {
            UserName = username,
            DisplayName = displayName ?? username
        };
        return await userManager.CreateAsync(user, password);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string id)
    {
        return await userManager.FindByIdAsync(id);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
    {
        return await userManager.Users.ToListAsync();
    }

    public async Task<IdentityResult> UpdateUserAsync(string id, string newUsername, string? displayName = null)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        }

        var result = await userManager.SetUserNameAsync(user, newUsername);
        if (!result.Succeeded)
        {
            return result;
        }

        if (displayName != null)
        {
            user.DisplayName = displayName;
            return await userManager.UpdateAsync(user);
        }

        return result;
    }

    public async Task<IdentityResult> UpdateDisplayNameAsync(string id, string displayName)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        }

        user.DisplayName = displayName;
        return await userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteUserAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        }
        return await userManager.DeleteAsync(user);
    }

    public async Task<IdentityResult> UpdateUserRoleAsync(string id, string? role)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return removeResult;
        }

        var roleToAssign = string.IsNullOrWhiteSpace(role) ? "User" : role;
        return await userManager.AddToRoleAsync(user, roleToAssign);
    }

    public async Task<string> GetUserRoleAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found");
        var roles = await userManager.GetRolesAsync(user);
        return roles.FirstOrDefault() ?? "User";
    }

    public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
    {
        return await userManager.FindByNameAsync(username);
    }

    public async Task<IdentityResult> AssignRoleAsync(ApplicationUser user, string role)
    {
        return await userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> ChangePasswordAsync(string userId, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserNotFound",
                Description = "User not found."
            });
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return await userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<IdentityResult> UpdateUsernameAsync(string userId, string newUsername)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        }

        var existingUser = await userManager.FindByNameAsync(newUsername);
        if (existingUser != null && existingUser.Id != userId)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Username already taken." });
        }

        user.UserName = newUsername;
        user.NormalizedUserName = userManager.NormalizeName(newUsername);
        return await userManager.UpdateAsync(user);
    }

}