using DigitalDistribution.Contracts;
using DigitalDistribution.Data;
using DigitalDistribution.Models;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace DigitalDistribution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher _hasher;
        public UserController(AppDbContext context, PasswordHasher hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // GET: api/<UserController>
        [HttpGet]
        public async Task<ActionResult<List<UserResponse>>> Get()
        {
            return await _context.Users.Select(u => new UserResponse(u.Login, u.Email)).ToListAsync();
        }

        // GET api/<UserController>/5
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserResponse>> Get(Guid id)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound();
            return Ok(new UserResponse(user.Login, user.Email));
        }
        
        // GET api/<UserController>/username123
        [HttpGet("{login}")]
        public async Task<ActionResult<List<UserResponse>>> Get(string login)
        {
            List<UserResponse> matches = await _context.Users
                .Where(u => EF.Functions.ILike(u.Login, $"%{login}%"))
                .Select(u => new UserResponse(u.Login, u.Email))
                .ToListAsync();
            if (matches.Count == 0)
                return NotFound();
            return Ok(matches);
        }

        // POST api/<UserController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateUserRequest value)
        {
            bool loginExists = await _context.Users.AnyAsync(u => u.Login == value.login);
            if (loginExists)
                return Conflict("Login already exists");
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == value.email);
            if (emailExists)
                return Conflict("Email already exists");
            var passwordHash = _hasher.HashPassword(value.password);
            User curUser = new User(value.login, value.email,  passwordHash);
            _context.Users.Add(curUser);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // PUT api/<UserController>/5
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] UpdateUserRequest value)
        {
            User? curUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (curUser == null)
                return NotFound();
            if(!String.IsNullOrEmpty(value.email))
            {
                if (await _context.Users.AnyAsync(u => u.Email == value.email && u.Id != id))
                    return Conflict("email is already taken");
                curUser.Email = value.email;
            }
            if (!String.IsNullOrEmpty(value.login))
            {
                if (await _context.Users.AnyAsync(u => u.Login == value.login && u.Id != id))
                    return Conflict("login is already taken");
                curUser.Login = value.login;
            }
            if (!String.IsNullOrEmpty(value.password))
                curUser.Password = _hasher.HashPassword(value.password);    
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            User? curUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (curUser == null)
                return NotFound();
            _context.Users.Remove(curUser);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
