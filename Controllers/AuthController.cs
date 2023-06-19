using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Running_DistanceCaltulate.Data;
using Running_DistanceCaltulate.Entity;
using Running_DistanceCaltulate.IService;

namespace Running_DistanceCaltulate.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        WebAPIContext _webAPIContext;

        public AuthController(IConfiguration configuration, IUserService userService, WebAPIContext context)
        {
            _configuration = configuration;
            _userService = userService;
            _webAPIContext = context;
        }

        [HttpGet, Authorize]
        public ActionResult<string> GetMyName()
        {
            return Ok(_userService.GetMyName());

            //var userName = User?.Identity?.Name;
            //var roleClaims = User?.FindAll(ClaimTypes.Role);
            //var roles = roleClaims?.Select(c => c.Value).ToList();
            //var roles2 = User?.Claims
            //    .Where(c => c.Type == ClaimTypes.Role)
            //    .Select(c => c.Value)
            //    .ToList();
            //return Ok(new { userName, roles, roles2 });
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            string passwordHash
                = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var findUser = await _webAPIContext.Users.FirstOrDefaultAsync(p => p.Username == request.Username);
            if (findUser != null)
            {
                return BadRequest("Username anda suda terdaftar");
            }

            user.Username = request.Username;
            user.PasswordHash = passwordHash;
            user.CreatedDate = request.CreatedDate;

            _webAPIContext.Users.Add(user);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _webAPIContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Unable to save change. " +
                                             "Try Again, if you have problem persists, " +
                                             "Contact your system administrator");
            }

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(UserDto request)
        {
            try
            {
                var user = await _webAPIContext.Users.FirstOrDefaultAsync(x => x.Username == request.Username);
                if (user.Username != request.Username)
                {
                    return BadRequest("User not found.");
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return BadRequest("Wrong password.");
                }

                string token = CreateToken(user);

                _webAPIContext.SaveChanges();


                return Ok(token);
            }
            catch
            {
                return BadRequest();
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "User"),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            user.Token = jwt;
            return jwt;
        }
    }
}