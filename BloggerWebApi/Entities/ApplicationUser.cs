using Microsoft.AspNetCore.Identity;

namespace BloggerWebApi.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public required string DisplayName { get; set; }

        // public string? ProfilePictureUrl { get; set; }
    }
}