using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDistribution.Models
{
    public class Game
    {
        public Game() { }
        public Game(string name, decimal price) 
        {
            Name = name; 
            Price = price;
        }
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        public ICollection<Key> Keys { get; set; } = new List<Key>();
    }
}
