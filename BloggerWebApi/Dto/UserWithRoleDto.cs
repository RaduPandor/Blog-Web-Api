namespace BloggerWebApi.Dto
{
    public class UserWithRoleDto
    {
        public required string Id { get; set; }
        public required string Username { get; set; }
        public required string Role { get; set; }
        public required string DisplayName { get; set; }
    }

}
