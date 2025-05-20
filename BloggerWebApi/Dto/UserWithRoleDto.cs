namespace BloggerWebApi.Dto
{
    public class UserWithRoleDto
    {
        public string Id { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string? Role { get; set; }
    }

}
