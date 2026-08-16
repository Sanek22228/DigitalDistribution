namespace DigitalDistribution.Models
{
    public class Key
    {
        public Guid Id { get; set; }
        public string Value { get; set; } = null!;
        public int Status { get; set; }
        public Order? Order { get; set; }
        public Game Game { get; set; } = null!;
        public Guid GameId { get; set; }
    }
}
