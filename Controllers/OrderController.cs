using DigitalDistribution.Contracts;
using DigitalDistribution.Data;
using DigitalDistribution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DigitalDistribution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrderController(AppDbContext context) 
        {
            _context = context;
        }
        // GET: api/<OrderController>
        [HttpGet]
        public async Task<ActionResult<List<OrderResponse>>> Get()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o =>
                new OrderResponse(o.Id, o.User.Login, o.Keys.Select(k =>
                                    new KeyResponse(k.Id, k.Value, k.Status, k.GameId, k.Game.Name, k.Game.Price)).ToList(),
                                    o.Status,
                                    o.TotalPrice,
                                    o.CreatedAt)
                ).ToListAsync();
            return orders;
        }

        // GET api/<OrderController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<OrderController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<OrderController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<OrderController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
