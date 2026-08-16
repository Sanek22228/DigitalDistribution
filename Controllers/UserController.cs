using DigitalDistribution.Contracts;
using DigitalDistribution.Data;
using DigitalDistribution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

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
        public List<User> Get()
        {
            return (_context.Users).ToList();
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
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
            List<UserResponse> matches = await _context.Users.Where(u => u.Login.Contains(login)).Select(u => new UserResponse(u.Login, u.Email)).ToListAsync();
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
            _context.SaveChanges();
            return Ok();
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] UpdateUserRequest value)
        {
            User? curUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (curUser == null)
                return NotFound();
            if(!String.IsNullOrEmpty(value.email))
                curUser.Email = value.email;
            if (!String.IsNullOrEmpty(value.login))
                curUser.Login = value.login;
            if (!String.IsNullOrEmpty(value.password))
                curUser.Password = _hasher.HashPassword(value.password);    
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
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
