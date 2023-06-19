namespace Running_DistanceCaltulate.IRepositore;

public interface IDistanceRepository
{
    Task<Object> CalculateDistanceSpeedTime(double latitude, double longitude);
    Task<Object> GetUserById();
}