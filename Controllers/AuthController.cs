using Microsoft.AspNetCore.Mvc;
using BloggerWebApi.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BloggerWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public AuthController(IUserService userService, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.userService = userService;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            var user = await userService.ValidateUserAsync(loginDto.Username, loginDto.Password);

            if (user == null)
            {
                return Unauthorized("Invalid login attempt.");
            }

            await signInManager.SignInAsync(user, isPersistent: false);
            return Ok(new
            {
                Message = "Login successful!",
                User = new
                {
                    user.UserName,
                    user.Id
                }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto)
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return BadRequest("Passwords do not match.");
            }

            var identityResult = await userManager.CreateAsync(new IdentityUser { UserName = registerDto.Username }, registerDto.Password);

            if (!identityResult.Succeeded)
            {
                return BadRequest(identityResult.Errors);
            }

            return Ok(new
            {
                Message = "Registration successful!",
                User = new
                {
                    Username = registerDto.Username
                }
            });
        }
    }
}
