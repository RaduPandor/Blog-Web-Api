using BloggerWebApi.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloggerWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService userService;

        public AuthController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto loginDto)
        {
            var user = await userService.ValidateUserAsync(loginDto.Username, loginDto.Password);
            if (user == null)
            {
                return Unauthorized("Invalid login attempt.");
            }

            return Ok(new
            {
                Message = "Login successful!",
                User = new { user.UserName, user.Id }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto)
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return BadRequest("Passwords do not match.");
            }

            var identityResult = await userService.RegisterUserAsync(registerDto.Username, registerDto.Password);
            if (!identityResult.Succeeded)
            {
                return BadRequest(identityResult.Errors);
            }

            var user = await userService.GetUserByUsernameAsync(registerDto.Username);
            if (user == null)
            {
                return StatusCode(500, "User was created but could not be retrieved.");
            }

            var roleResult = await userService.AssignRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                return StatusCode(500, new { Message = "User created but role assignment failed", Errors = roleResult.Errors });
            }

            return Ok(new { Message = "Registration successful!", Username = registerDto.Username });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await userService.SignOutAsync();
            return Ok(new { Message = "Logout successful!" });
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createDto)
        {
            var identityResult = await userService.CreateUserAsync(createDto.Username, createDto.Password);
            if (!identityResult.Succeeded)
            {
                return BadRequest(identityResult.Errors);
            }

            var user = await userService.GetUserByUsernameAsync(createDto.Username);
            if (user == null)
            {
                return StatusCode(500, "User was created but could not be retrieved.");
            }

            var role = createDto.IsAdmin ? "Admin" : "User";
            var roleResult = await userService.AssignRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                return StatusCode(500, new { Message = "User created but role assignment failed", Errors = roleResult.Errors });
            }

            return Ok(new { Message = $"User created successfully with role {role}", Username = createDto.Username, UserId = user.Id });
        }

        [HttpGet("getall")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userService.GetAllUsersAsync();
            var list = new List<UserWithRoleDto>(users.Count());

            foreach (var u in users)
            {
                var role = await userService.GetUserRoleAsync(u.Id);
                list.Add(new UserWithRoleDto
                {
                    Id = u.Id,
                    Username = u.UserName!,
                    Role = role
                });
            }

            return Ok(list);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && currentUserId != id)
            {
                return Forbid();
            }

            var u = await userService.GetUserByIdAsync(id);
            if (u == null)
            {
                return NotFound();
            }

            var role = await userService.GetUserRoleAsync(id);
            return Ok(new UserWithRoleDto
            {
                Id = u.Id,
                Username = u.UserName!,
                Role = role
            });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUsernameDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && currentUserId != id)
            {
                return Forbid();
            }

            var result = await userService.UpdateUserAsync(id, dto.Username);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && currentUserId != id)
            {
                return Forbid();
            }

            var identityResult = await userService.DeleteUserAsync(id);
            if (!identityResult.Succeeded)
            {
                return BadRequest(identityResult.Errors);
            }

            return Ok(new { Message = "User deleted successfully!" });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser([FromServices] UserManager<IdentityUser> userManager)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);

            return Ok(new
            {
                id = user.Id,
                userName = user.UserName,
                roles
            });
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(string id, [FromBody] RoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            string roleToAssign = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role;

            var result = await userService.UpdateUserRoleAsync(id, roleToAssign);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok();
        }
    }
}