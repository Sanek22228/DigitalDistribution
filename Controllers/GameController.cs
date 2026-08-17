using DigitalDistribution.Contracts;
using DigitalDistribution.Data;
using DigitalDistribution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var games = await _context.Games.Select(g => new GameResponse(g.Name, g.Price)).ToListAsync();
            return Ok(games);
        }

        // GET api/<Game>/5
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GameResponse>> Get(Guid id)
        {
            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);
            if(game == null)
                return NotFound();
            return Ok(new GameResponse(game.Name, game.Price));
        }

        // GET api/<Game>/name
        [HttpGet("{name}")]
        public async Task<ActionResult<List<GameResponse>>> Get(string name)
        {
            var games = await _context.Games
                .Where(g => EF.Functions.ILike(g.Name, $"%{name}%"))
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
            return Ok();
        }

        // PUT api/<Game>/5
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Put(Guid id, [FromBody] UpdateGameRequest game)
        {
            var curGame = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);
            if(curGame == null)
                return NotFound();
            if (!String.IsNullOrEmpty(game.name))
            {
                if (await _context.Games.AnyAsync(g => g.Name == game.name && g.Id != id))
                    return Conflict("Game name is already taken");

                curGame.Name = game.name;
            }
                
            if (game.price.HasValue)
                curGame.Price = game.price.Value;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE api/<Game>/5
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id.Equals(id));
            if (game == null)
                return NotFound();
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
