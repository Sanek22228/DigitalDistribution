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
            return await _context.Users.Where(u => !u.IsDeleted).Select(u => new UserResponse(
                u.Id,
                u.Login,
                u.Email,
                u.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o =>
                    new OrderResponse(o.Id, u.Login, o.Keys.Select(k =>
                        new KeyResponse(k.Id, k.Value, k.Status, k.GameId, k.Game.Name, k.Game.Price)).ToList(),
                    o.Status,
                    o.TotalPrice,
                    o.CreatedAt))
                .ToList()
            )).ToListAsync();
        }

        // GET api/<UserController>/5
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserResponse>> Get(Guid id)
        {
            UserResponse? user = await _context.Users
                .Where(u => u.Id == id && u.IsDeleted != true)
                .Select( u =>
                    new UserResponse(
                        u.Id,
                        u.Login,
                        u.Email,
                        u.Orders
                            .Where(o => o.Status == OrderStatus.Completed)
                            .OrderByDescending(o => o.CreatedAt)
                            .Select(o =>
                                new OrderResponse(o.Id, u.Login, o.Keys.Select(k =>
                                    new KeyResponse(k.Id, k.Value, k.Status, k.GameId, k.Game.Name, k.Game.Price)).ToList(),
                                    o.Status,
                                    o.TotalPrice,
                                    o.CreatedAt))
                            .ToList()))
                .FirstOrDefaultAsync();
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        // GET api/<UserController>/search/username123
        [HttpGet("search/{login}")]
        public async Task<ActionResult<List<SearchUserResponse>>> Get(string login)
        {
            var matches = await _context.Users
                .Where(u => EF.Functions.ILike(u.Login, $"{login}%") && u.IsDeleted != true)
                .Select(u => new SearchUserResponse(u.Id, u.Login))
                .ToListAsync();
            if (matches.Count == 0)
                return NotFound();
            return Ok(matches);
        }
        [HttpGet("{id:guid}/profile")]
        public async Task<ActionResult<PublicProfileUserResponse>> GetPublicProfile(Guid id)
        {
            PublicProfileUserResponse? user = await _context.Users
                .Where(u => u.Id == id && u.IsDeleted != true)
                .Select(u =>
                    new PublicProfileUserResponse(
                        u.Id,
                        u.Login,
                        u.Orders.Where(o => o.Status == OrderStatus.Completed)
                            .OrderByDescending(o => o.CreatedAt)
                            .SelectMany(o => o.Keys)
                            .Select(k => new GameResponse(k.Game.Id, k.Game.Name, k.Game.Price))
                            .Distinct() // если к игре куплены dlc или несколько ключей - отображается 1 раз
                            .ToList()
                    )
                )
                .FirstOrDefaultAsync();
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        // POST api/<UserController>
        [HttpPost("registration")]
        public async Task<ActionResult<UserResponse>> Registration([FromBody] CreateUserRequest value)
        {
            bool loginExists = await _context.Users.AnyAsync(u => u.Login == value.login);
            if (loginExists)
                return Conflict("Login already taken");
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == value.email);
            if (emailExists)
                return Conflict("Email already taken");
            var passwordHash = _hasher.HashPassword(value.password);
            var curUser = new User(value.login, value.email,  passwordHash);
            try
            {
                _context.Users.Add(curUser);
                await _context.SaveChangesAsync();
                return Ok(new UserResponse(curUser.Id, curUser.Login, curUser.Email, new List<OrderResponse>()));
            }
            catch (DbUpdateException)
            {
                return Conflict("Login or email already taken.");
            }
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserResponse>> Login([FromBody] LoginUserRequest value)
        {
            var user = await _context.Users.Where(u => !u.IsDeleted).FirstOrDefaultAsync(u => u.Login == value.userData || u.Email == value.userData);
            if (user == null)
                return NotFound("Login or email doesn't exist");
            if (_hasher.VerifyPassword(value.password, user.Password))
                return Ok(
                    new UserResponse(
                        user.Id,
                        user.Login,
                        user.Email,
                        user.Orders
                            .Where(o => o.Status == OrderStatus.Completed)
                            .OrderByDescending(o => o.CreatedAt)
                            .Select(o =>
                                new OrderResponse(o.Id, user.Login, o.Keys.Select(k =>
                                    new KeyResponse(k.Id, k.Value, k.Status, k.GameId, k.Game.Name, k.Game.Price)).ToList(),
                                    o.Status,
                                    o.TotalPrice,
                                    o.CreatedAt))
                            .ToList()
                    )
                );
            return NotFound("Password is incorrect");
        }

        // PUT api/<UserController>/5
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] UpdateUserRequest value)
        {
            User? curUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted != true);
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
            return Ok(
                new UserResponse(
                    curUser.Id,
                    curUser.Login,
                    curUser.Email,
                    curUser.Orders
                        .Where(o => o.Status == OrderStatus.Completed)
                        .OrderByDescending(o => o.CreatedAt)
                        .Select(o =>
                            new OrderResponse(o.Id, curUser.Login, o.Keys.Select(k =>
                                new KeyResponse(k.Id, k.Value, k.Status, k.GameId, k.Game.Name, k.Game.Price)).ToList(),
                                o.Status,
                                o.TotalPrice,
                                o.CreatedAt))
                        .ToList()
                ));
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            User? curUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted != true);
            if (curUser == null)
                return NotFound();
            if (curUser.Role == UserRole.Admin)
                return Conflict("User with admin role cannot be deleted");

            curUser.IsDeleted = true;
            curUser.Email = $"deleted_user_{curUser.Id.ToString()[..8]}@deleted.local";
            curUser.Login = $"deleted_user_{curUser.Id.ToString()[..8]}";
            curUser.Password = _hasher.HashPassword(Guid.NewGuid().ToString()[..8]);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
