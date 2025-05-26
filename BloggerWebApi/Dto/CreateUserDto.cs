public class CreateUserDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public bool IsAdmin { get; set; }
    public required string DisplayName { get; set; }
}
