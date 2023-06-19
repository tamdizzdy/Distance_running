using Microsoft.EntityFrameworkCore;
using Running_DistanceCaltulate.Data;
using Running_DistanceCaltulate.Entity;
using Running_DistanceCaltulate.IRepositore;
using Running_DistanceCaltulate.IService;

namespace Running_DistanceCaltulate.Repository;

public class DistanceRepository : IDistanceRepository
{
    private readonly WebAPIContext _context;
    private DateTime previousUpdateTime;
    private double previousLatitude;
    private double previousLongitude;
    private bool canUpdatePosition = true;
    private readonly IUserService _userService;


    public DistanceRepository(WebAPIContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    private double CalculateDistance(double startLatitude, double startLongitude, double endLatitude,
        double endLongitude)
    {
        const double earthRadiusKm = 6371; // Bán kính Trái Đất tính theo kilômét

        // Chuyển đổi độ sang radianasas
        double lat1 = DegreesToRadians(startLatitude);
        double lon1 = DegreesToRadians(startLongitude);
        double lat2 = DegreesToRadians(endLatitude);
        double lon2 = DegreesToRadians(endLongitude);

        // Tính toán haversine
        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double distance = earthRadiusKm * c;

        return distance;
    }

    private double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    private double CalculateSpeed(double distance, double time)
    {
        // Tính toán vận tốc (kích cỡ và đơn vị vận tốc phụ thuộc vào yêu cầu của bạn)
        double speed = distance / time;

        return speed;
    }

    private double CalculateTime(double distance, double speed)
    {
        // Tính toán thời gian (đơn vị thời gian phụ thuộc vào yêu cầu của bạn)
        double time = distance / speed;

        return time;
    }

    public async Task<Object> CalculateDistanceSpeedTime(double latitude, double longitude)
    {
        try
        {
            // var user = await _context.Users.FindAsync(userId);
            var usercheck = _userService.GetMyName();

            // if (userId != user.Id)
            // {
            //     throw new Exception("UserId mismatch");
            // }
            //
            // if (user.Id == null)
            // {
            //     throw new Exception("UserId does not exist");
            // }

            // Kiểm tra nếu đã có vị trí trước đó và đã đủ 10 giây kể từ lần cập nhật trước đó
            if (previousUpdateTime != null && DateTime.UtcNow.Subtract(previousUpdateTime).TotalSeconds < 10)
            {
                throw new Exception("Chưa đủ thời gian để cập nhật vị trí mới.");
            }

            if (previousUpdateTime == null || DateTime.UtcNow.Subtract(previousUpdateTime).TotalSeconds >= 10)
            {
                // Lấy kết quả tính toán gần nhất
                CalculationResultDistance latestResult = await _context.CalculationResult
                    .Where(c => c.Username == usercheck)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (latestResult == null || DateTime.UtcNow.Subtract(latestResult.CreatedAt).TotalSeconds >= 10)
                {
                    // Nếu không có kết quả tính toán trước đó hoặc đã đủ thời gian để cập nhật vị trí mới, thực hiện cập nhật
                    if (canUpdatePosition)
                    {
                        double time;
                        double timeFromLastUpdate = latestResult != null
                            ? DateTime.UtcNow.Subtract(latestResult.CreatedAt).TotalSeconds
                            : 0;

                        // Tính toán khoảng cách và thời gian dựa trên vị trí mới và vị trí trước đó
                        double distance =
                            CalculateDistance(previousLatitude, previousLongitude, latitude, longitude);
                        double speed = CalculateSpeed(distance, 10); // Giả sử thời gian di chuyển là 10 giây
                        if (timeFromLastUpdate == 0)
                        {
                            time = CalculateTime(distance, speed);
                        }
                        else
                        {
                            time = timeFromLastUpdate + CalculateTime(distance, speed) - 10;
                        }

                        // Cập nhật vị trí trước đó và thời gian cập nhật trước đó
                        previousLatitude = latitude;
                        previousLongitude = longitude;
                        previousUpdateTime = DateTime.UtcNow;

                        // Không cho phép cập nhật trong khoảng thời gian 10 giây
                        canUpdatePosition = false;

                        // Lưu kết quả tính toán vào cơ sở dữ liệu
                        CalculationResultDistance result = new CalculationResultDistance
                        {
                            Distance = distance,
                            Speed = speed,
                            Time = time,
                            CreatedAt = DateTime.UtcNow,
                            Username = usercheck
                        };

                        _context.CalculationResult.Add(result);
                        await _context.SaveChangesAsync();

                        // // Tính trung bình cộng của các trường distance, speed và time
                        // double averageDistance = await _context.CalculationResult.AverageAsync(r => r.Distance);
                        // double averageSpeed = await _context.CalculationResult.AverageAsync(r => r.Speed);
                        // double averageTime = await _context.CalculationResult.AverageAsync(r => r.Time);
                        double totalDistance = await _context.CalculationResult.Where(c => c.Username == usercheck)
                            .SumAsync(r => r.Distance);

                        // Lấy tổng thời gian từ cơ sở dữ liệu
                        double totalTravelTime = await _context.CalculationResult.Where(c => c.Username == usercheck)
                            .SumAsync(r => r.Time);

                        // Tính vận tốc trung bình
                        double averageSpeed = totalDistance / totalTravelTime;

                        CalculationResultDistanceModel rs = new CalculationResultDistanceModel()
                        {
                            Distance = distance,
                            Speed = speed,
                            Time = time,
                            // AverageDistance = averageDistance,
                            AverageSpeed = averageSpeed,
                            // AverageTime = averageTime,
                            TotalDistanceFromStart = totalDistance,
                            TotalTimeFromStart = totalTravelTime
                        };
                        return rs;
                    }
                    else
                    {
                        throw new Exception("Chưa đủ thời gian để cập nhật vị trí mới.");
                    }
                }
                else
                {
                    // Kiểm tra thời gian để gửi vị trí tiếp theo
                    DateTime nextPositionUpdateAt = latestResult.CreatedAt.AddSeconds(10);
                    TimeSpan timeUntilNextUpdate = nextPositionUpdateAt.Subtract(DateTime.UtcNow);

                    if (timeUntilNextUpdate.TotalSeconds > 0)
                    {
                        // Trả về thời gian cần chờ để gửi vị trí tiếp theo
                        TimeUntilNextModel timeUntil = new TimeUntilNextModel()
                            { TimeUntilNextUpdate = timeUntilNextUpdate.TotalSeconds };
                        return timeUntil;
                    }
                    else
                    {
                        // Nếu đã đủ thời gian để cập nhật vị trí
                        if (canUpdatePosition)

                        {
                            double time;
                            double timeFromLastUpdate = latestResult != null
                                ? DateTime.UtcNow.Subtract(latestResult.CreatedAt).TotalSeconds
                                : 0;
                            // Tính toán khoảng cách và thời gian dựa trên vị trí mới và vị trí trước đó
                            double distance =
                                CalculateDistance(previousLatitude, previousLongitude, latitude, longitude);
                            double speed = CalculateSpeed(distance, 10); // Giả sử thời gian di chuyển là 10 giây
                            if (timeFromLastUpdate == 0)
                            {
                                time = CalculateTime(distance, speed);
                            }
                            else
                            {
                                time = timeFromLastUpdate + CalculateTime(distance, speed) - 10;
                            }

                            // Cập nhật vị trí trước đó và thời gian cập nhật trước đó
                            previousLatitude = latitude;
                            previousLongitude = longitude;
                            previousUpdateTime = DateTime.UtcNow;

                            // Không cho phép cập nhật trong khoảng thời gian 10 giây
                            canUpdatePosition = false;

                            // Lưu kết quả tính toán vào cơ sở dữ liệu
                            CalculationResultDistance result = new CalculationResultDistance
                            {
                                Distance = distance,
                                Speed = speed,
                                Time = time,
                                CreatedAt = DateTime.UtcNow,
                                Username = usercheck
                            };

                            _context.CalculationResult.Add(result);
                            await _context.SaveChangesAsync();

                            // Tính trung bình cộng của các trường distance, speed và time
                            double TotalDistanceFromStart = await _context.CalculationResult
                                .Where(c => c.Username == usercheck)
                                .AverageAsync(r => r.Distance);
                            double averageSpeed = await _context.CalculationResult.Where(c => c.Username == usercheck)
                                .AverageAsync(r => r.Speed);
                            double totalTravelTime = await _context.CalculationResult
                                .Where(c => c.Username == usercheck)
                                .AverageAsync(r => r.Time);

                            // Trả về kết quả
                            CalculationResultDistanceModel rs = new CalculationResultDistanceModel()
                            {
                                Distance = distance,
                                Speed = speed,
                                Time = time,
                                // AverageDistance = averageDistance,
                                AverageSpeed = averageSpeed,
                                // AverageTime = averageTime,
                                TotalDistanceFromStart = TotalDistanceFromStart,
                                TotalTimeFromStart = totalTravelTime
                            };
                            return rs;
                        }
                        else
                        {
                            throw new Exception("Chưa đủ thời gian để cập nhật vị trí mới.");
                        }
                    }
                }
            }

            throw new Exception("Không thể tính toán vị trí mới.");
        }
        catch (Exception ex)
        {
            return (new { message = ex.Message });
        }
    }

    public async Task<Object> GetUserById()
    {
        try
        {
            var usercheck = _userService.GetMyName();

            // var uu = await _context.Users.FindAsync(userId);
            // if (userId != uu.Id)
            // {
            //     throw new Exception("UserId mismatch");
            // }
            // if (uu.Id == null)
            // {
            //     throw new Exception("UserId does not exist");
            // }

            var user = await (from u in _context.Users
                    join c in _context.CalculationResult on u.Username equals c.Username
                    where usercheck == u.Username
                    select new UserModel()
                    {
                        userId = u.Id,
                        Username = u.Username,
                        PhoneNumber = u.PhoneNumber,
                        // DOB = u.DOB,
                    }
                ).FirstOrDefaultAsync();

            double totalDistance = await _context.CalculationResult.Where(c => c.Username == usercheck)
                .SumAsync(r => r.Distance);

            double totalTravelTime =
                await _context.CalculationResult.Where(c => c.Username == usercheck).SumAsync(r => r.Time);

            double averageSpeed = totalDistance / totalTravelTime;

            int count = await _context.CalculationResult.CountAsync(c => c.Username == usercheck);
            return new DistanceDto()
            {
                AverageSpeed = averageSpeed,
                TotalDistanceFromStart = totalDistance,
                TotalTimeFromStart = totalTravelTime,
                CountReq = count
            };
        }
        catch
        {
            return (new { message = "UserId notfound or User not start" });
        }
    }
}