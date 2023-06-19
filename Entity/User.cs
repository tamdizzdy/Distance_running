namespace Running_DistanceCaltulate.Entity;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    // public DateTime DOB { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }
}