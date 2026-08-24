using DigitalDistribution.Contracts;
using DigitalDistribution.Data;
using DigitalDistribution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DigitalDistribution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly AppDbContext _context;
        public GameController(AppDbContext context) { 
            _context = context;
        }
        // GET: api/<Game>
        [HttpGet]
        public async Task<ActionResult<List<GameResponse>>> Get()
        {
            var games = await _context.Games.Where(g => !g.IsDeleted).Select(g => new GameResponse(g.Name, g.Price)).ToListAsync();
            return Ok(games);
        }

        // GET api/<Game>/5
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GameResponse>> Get(Guid id)
        {
            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
            if(game == null)
                return NotFound();
            return Ok(new GameResponse(game.Name, game.Price));
        }

        // GET api/<Game>/search/name
        [HttpGet("search/{name}")]
        public async Task<ActionResult<List<GameResponse>>> Get(string name)
        {
            var games = await _context.Games
                .Where(g => EF.Functions.ILike(g.Name, $"%{name}%") && !g.IsDeleted)
                .Select(g => new GameResponse(g.Name, g.Price))
                .ToListAsync();

            return Ok(games);
        }

        // POST api/<Game>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GameRequest game)
        {
            Game newGame = new Game(game.name, game.price);
            _context.Games.Add(newGame);
            await _context.SaveChangesAsync();
            return Created();
        }

        // PUT api/<Game>/5
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] UpdateGameRequest game)
        {
            var curGame = await _context.Games.Where(g => !g.IsDeleted).FirstOrDefaultAsync(g => g.Id == id);
            if(curGame == null)
                return NotFound();
            if (!String.IsNullOrEmpty(game.name))
            {
                curGame.Name = game.name;
            }   
            if (game.price.HasValue)
                curGame.Price = game.price.Value;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/<Game>/5
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id.Equals(id));
            if (game == null)
                return NotFound();

            game.IsDeleted = true;
            await _context.Keys.Where(k => k.GameId == game.Id && k.Status == KeyStatus.Idle)
                .ExecuteUpdateAsync(setters => setters.SetProperty(k => k.Status, KeyStatus.Expired));
            
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
