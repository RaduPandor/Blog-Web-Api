using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public interface IUserService
{
    Task<IdentityUser> RegisterUserAsync(string username, string password);
    Task<IdentityUser?> ValidateUserAsync(string username, string password);
}
