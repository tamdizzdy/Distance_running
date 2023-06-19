using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Running_DistanceCaltulate.Data;
using Running_DistanceCaltulate.IRepositore;
using Swashbuckle.AspNetCore.Annotations;

namespace Running_DistanceCaltulate.Controllers;

[Route("api/v1/distance-calculate")]
[ApiController]
public class DistanceController : ControllerBase
{
    private readonly WebAPIContext _context;
    private readonly IDistanceRepository _repository;

    public DistanceController(IDistanceRepository repository, WebAPIContext context)
    {
        _repository = repository;
        _context = context;
    }

    [SwaggerOperation("Get calculate-distance-speed-time by longitude,latitude")]
    [HttpGet("calculate-distance-time/byUser"), Authorize]
    public async Task<IActionResult> CalculateDistance2(double latitude, double longitude)
    {
        try
        {
            var calculateResult = await _repository.CalculateDistanceSpeedTime(latitude, longitude);
            return Ok(calculateResult);
        }
        catch (Exception ex)
        {
            return BadRequest("UserId notfound or User not start");
        }
    }

    [SwaggerOperation("Get user description by Id")]
    [HttpGet("byUser"), Authorize]
    public async Task<IActionResult> GetUserById()
    {
        try
        {
            var calculateResult = await _repository.GetUserById();
            return Ok(calculateResult);
        }
        catch (Exception ex)
        {
            return BadRequest("UserId notfound or User not start");
        }
    }
}