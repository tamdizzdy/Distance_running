using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Running_DistanceCaltulate.Data;
using Running_DistanceCaltulate.Entity;
using Running_DistanceCaltulate.IRepositore;
using Running_DistanceCaltulate.IService;

namespace Running_DistanceCaltulate.Repository;

public class UserRepository : IUserRepository
{
    private readonly WebAPIContext _context;
    private readonly IUserService _userService;

    public UserRepository(WebAPIContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }


    public async Task<ActionResult<List<User>>> GetAllUser()
    {
        var listUser = await _context.Users.ToListAsync();
        return listUser;
    }

    public async Task<ActionResult<User>> GetById()
    {
        var usercheck = _userService.GetMyName();
        // var user = await _context.Users.FindAsync(id);
        User latestResult = await _context.Users.Where(c => c.Username == usercheck).FirstOrDefaultAsync();
        if (latestResult == null) throw new Exception("user Not found");
        return latestResult;
    }

    public async Task<ActionResult<User>> UpdateUser(UserUpdateDTO user)
    {
        var usercheck = _userService.GetMyName();

        if (usercheck == null)
        {
            throw new Exception("User Not found");
        }

        var userUp = await _context.Users.Where(c => c.Username == usercheck).FirstOrDefaultAsync();

        userUp.PhoneNumber = user.PhoneNumber;
        userUp.UpdatedDate = user.UpdatedDate;
        _context.Entry(userUp).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return userUp;
    }

    public async Task<ActionResult<User>> DeleteUser(int id, bool? saveChangesError)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            throw new Exception("userId Not found");
        }

        _context.Entry(user).State = EntityState.Deleted;
        await _context.SaveChangesAsync();

        return user;
    }
}