using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDistribution.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public User User { get; set; } = null!;
        public Guid UserId { get; set; }
        public ICollection<Key> Keys { get; set; } = new List<Key>();
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
    }
    public enum OrderStatus
    {
        Pending = 0, // ожидает оплаты
        Completed = 1, // оплата прошла, ключи выданы
        Cancelled = 2, // заказ отменен до оплаты (истек таймаут, пользователь отменил)
        Refunded = 3, // деньги возвращены
        Failed = 4 //произошел сбой во время оплаты (нехватка средств, отказ банка)
    }
}
