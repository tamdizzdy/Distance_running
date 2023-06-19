namespace Running_DistanceCaltulate.Data;

public class UserDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public DateTime CreatedDate { get; set; }
}