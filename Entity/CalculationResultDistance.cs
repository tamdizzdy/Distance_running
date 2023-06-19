namespace Running_DistanceCaltulate.Entity;

public class CalculationResultDistance
{
    public int Id { get; set; }
    public string Username { get; set; }
    public double Distance { get; set; }
    public double Speed { get; set; }
    public double Time { get; set; }

    public DateTime CreatedAt { get; set; }

    // public int UserId { get; set; } // Add UserId field to store the logged-in user's information
    public CalculationResultDistance()
    {
        CreatedAt = DateTime.UtcNow;
    }
}