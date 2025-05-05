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
        return await userManager.CreateAsync(user, password);
    }

    public async Task<IdentityUser?> ValidateUserAsync(string username, string password)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
        {
            return null;
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
        return result.Succeeded ? user : null;
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

        user.UserName = newUsername;
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
}