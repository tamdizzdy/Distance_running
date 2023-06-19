using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Running_DistanceCaltulate.Data;
using Running_DistanceCaltulate.Entity;
using Running_DistanceCaltulate.IRepositore;
using Swashbuckle.AspNetCore.Annotations;

namespace Running_DistanceCaltulate.Controllers;

[Route("api/v1/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserRepository _repository;

    public UserController(IUserRepository repository)
    {
        _repository = repository;
    }

    [SwaggerOperation("GetAll List User")]
    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var listUser = await _repository.GetAllUser();
        return Ok(listUser);
    }

    [SwaggerOperation("Get user By Id")]
    [HttpGet("byUserId"), Authorize]
    public async Task<ActionResult<User>> GetById()
    {
        try
        {
            var user = await _repository.GetById();
            return Ok(user);
        }
        catch
        {
            return NoContent();
        }
    }

    [SwaggerOperation("Update User By Id")]
    [HttpPut("byUser"), Authorize]
    public async Task<ActionResult<User>> UpdateUser(UserUpdateDTO user)
    {
        try
        {
            var updateUser = await _repository.UpdateUser(user);
            return Ok(updateUser);
        }
        catch
        {
            return Conflict();
        }
    }

    [SwaggerOperation("Delete User By Id")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<User>> Delete(int id, bool? saveChangesError = false)
    {
        try
        {
            if (id == null)
            {
                throw new Exception("userId Not found");
            }

            var updateUser = _repository.DeleteUser(id);
            return Ok(updateUser);
        }
        catch
        {
            return NoContent();
        }
    }
}