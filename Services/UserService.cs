using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public class UserService : IUserService
{
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;

    public UserService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    public async Task<IdentityUser> RegisterUserAsync(string username, string password)
    {
        var user = new IdentityUser { UserName = username };
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            return user;
        }
        return null;
    }

    public async Task<IdentityUser?> ValidateUserAsync(string username, string password)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user != null)
        {
            var result = await signInManager.CheckPasswordSignInAsync(user, password, false);
            if (result.Succeeded)
            {
                return user;
            }
        }
        return null;
    }
}
