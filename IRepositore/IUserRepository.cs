using Microsoft.AspNetCore.Mvc;
using Running_DistanceCaltulate.Data;
using Running_DistanceCaltulate.Entity;

namespace Running_DistanceCaltulate.IRepositore;

public interface IUserRepository
{
    Task<ActionResult<List<User>>> GetAllUser();
    Task<ActionResult<User>> GetById();
    Task<ActionResult<User>> UpdateUser(UserUpdateDTO user);
    Task<ActionResult<User>> DeleteUser(int id, bool? saveChangesError = false);
}