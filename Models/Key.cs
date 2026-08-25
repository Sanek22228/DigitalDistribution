using Microsoft.EntityFrameworkCore;

namespace DigitalDistribution.Models
{
    [Index(nameof(Value), IsUnique = true)]
    public class Key
    {
        public Key() { }
        public Key(string value, KeyStatus status, Guid gameId) 
        {
            Value = value;
            Status = status;
            GameId = gameId;
        }
        public Guid Id { get; set; }
        public string Value { get; set; } = null!;
        public KeyStatus Status { get; set; } = KeyStatus.Idle;
        public Order? Order { get; set; }
        public Guid? OrderId { get; set; }
        public Game Game { get; set; } = null!;
        public Guid GameId { get; set; }
    }
    public enum KeyStatus
    {
        Idle = 0, //неактивен
        Pending = 1, // ожидает оплаты
        Sold = 2, // продан
        Redeemed = 3, // активирован
        Expired = 4 //просрочен
    }
}
