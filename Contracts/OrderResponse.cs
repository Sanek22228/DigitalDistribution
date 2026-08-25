using DigitalDistribution.Models;

namespace DigitalDistribution.Contracts
{
    public record OrderResponse(Guid orderId, string userLogin, List<KeyResponse> keys, OrderStatus orderStatus, decimal totalPrice, DateTime createdAt);
}
