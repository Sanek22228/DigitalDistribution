using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDistribution.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public User User { get; set; } = null!;
        public Guid UserId { get; set; }
        public Key Key { get; set; } = null!;
        public Guid KeyId { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Status { get; set; }
    }
}
