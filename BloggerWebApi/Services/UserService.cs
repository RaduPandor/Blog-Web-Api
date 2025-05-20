using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;

    public UserService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    public async Task<IdentityResult> RegisterUserAsync(string username, string password)
    {
        var user = new IdentityUser { UserName = username };
        var result = await userManager.CreateAsync(user, password);
        return result;
    }
    public async Task<IdentityUser?> ValidateUserAsync(string username, string password)
    {
        var result = await signInManager.PasswordSignInAsync(
            username,
            password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!result.Succeeded)
            return null;

        return await userManager.FindByNameAsync(username);
    }

    public async Task SignOutAsync()
    {
        await signInManager.SignOutAsync();
    }

    public async Task<IdentityResult> CreateUserAsync(string username, string password)
    {
        var user = new IdentityUser { UserName = username };
        return await userManager.CreateAsync(user, password);
    }

    public async Task<IdentityUser?> GetUserByIdAsync(string id)
    {
        return await userManager.FindByIdAsync(id);
    }

    public async Task<IEnumerable<IdentityUser>> GetAllUsersAsync()
    {
        return await userManager.Users.ToListAsync();
    }

    public async Task<IdentityResult> UpdateUserAsync(string id, string newUsername)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        }

        return await userManager.SetUserNameAsync(user, newUsername);
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
            if (!removeResult.Succeeded)
            {
                return removeResult;
            }
        }

        string roleToAssign = string.IsNullOrWhiteSpace(role) ? "User" : role;
        var addResult = await userManager.AddToRoleAsync(user, roleToAssign);
        return addResult;
    }

    public async Task<string> GetUserRoleAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found");
        var roles = await userManager.GetRolesAsync(user);
        return roles.FirstOrDefault() ?? "User";
    }

    public async Task<IdentityUser?> GetUserByUsernameAsync(string username)
    {
        return await userManager.FindByNameAsync(username);
    }

    public async Task<IdentityResult> AssignRoleAsync(IdentityUser user, string role)
    {
        return await userManager.AddToRoleAsync(user, role);
    }

}