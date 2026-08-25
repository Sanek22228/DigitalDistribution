using DigitalDistribution.Contracts;
using DigitalDistribution.Data;
using DigitalDistribution.Models;
using DigitalDistribution.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DigitalDistribution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeyController : ControllerBase
    {
        private  readonly AppDbContext _context;
        public KeyController(AppDbContext context) 
        {
            _context = context;
        }
        // GET: api/<KeyController>
        [HttpGet]
        public async Task<ActionResult<KeyResponse>> Get()
        {
            var keys = await _context.Keys.OrderBy(k => k.GameId).Select(k => new KeyResponse(k.Id, k.Value, k.Status, k.GameId, k.Game.Name, k.Game.Price)).ToListAsync();
            return Ok(keys);
        }

        // GET api/<KeyController>/5
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<List<KeyResponse>>> Get(Guid id)
        {
            var key = await _context.Keys.Include(k => k.Game).FirstOrDefaultAsync(k => k.Id == id);
            if (key == null)
                return NotFound();
            return Ok(new KeyResponse(key.Id, key.Value, key.Status, key.GameId, key.Game.Name, key.Game.Price));
        }
        // нужно ли?
        //[HttpGet("search/{gameName}")]
        //public async Task<ActionResult<List<KeyResponse>>> Get(string gameName)
        //{
        //    var keys = await _context.Keys.Include(k => k.Game).Where(k => k.Game.Name == gameName).OrderBy(k => k.GameId).Select(k => new KeyResponse(k.Id, k.Value, k.Status, k.GameId, k.Game.Name, k.Game.Price)).ToListAsync();
        //    return Ok(keys);
        //}
        // POST api/<KeyController>
        [HttpPost]
        public async Task<ActionResult<KeyResponse>> Post([FromBody] KeyRequest value)
        {
            var game = await _context.Games.Where(g => !g.IsDeleted).FirstOrDefaultAsync(g => g.Id == value.gameId);
            if (game == null)
                return NotFound();
            const int maxAttempts = 10;
            for (int i = 0; i < maxAttempts; i++)
            {
                // as the key value is unique we have to handle collision exception (DbUpdateConcurrencyException)
                var keyValue = KeyGenerator.GenerateKey();
                var key = new Key(keyValue, value.status, value.gameId);
                try
                {
                    _context.Keys.Add(key);
                    await _context.SaveChangesAsync();
                    return Ok(new KeyResponse(key.Id, key.Value, key.Status, key.GameId, game.Name, game.Price));
                }
                // when a DbUpdateException appears entity which caused it stays in ef core emory as added, so we have to detach it and try again in the while cicle
                catch (DbUpdateException ex)
                {
                    _context.Entry(key).State = EntityState.Detached;
                    if (ex.InnerException is PostgresException pgEx)
                    {
                        // 23505: duplicate key value violates unique constraint "value"
                        if (pgEx.SqlState == "23505")
                            continue;
                    }
                    throw; // any other exception
                }
            }
            return BadRequest("Failed to generate a key value. Try again later");
        }

        // PUT api/<KeyController>/5
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] UpdateKeyRequest value)
        {
            var key = await _context.Keys.FirstOrDefaultAsync(k => k.Id == id);

            if (key == null)
                return NotFound();

            key.Status = value.status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/<KeyController>/5
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var key = await _context.Keys.FirstOrDefaultAsync(k => k.Id == id);

            if (key == null)
                return NotFound();
            if (key.Status != KeyStatus.Idle && key.Status != KeyStatus.Expired)
                    return Conflict("Only keys in 'Idle' or 'Expired' state can be deleted.");

            _context.Remove(key);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
