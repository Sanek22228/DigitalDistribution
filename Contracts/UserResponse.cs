using DigitalDistribution.Models;

namespace DigitalDistribution.Contracts
{
    public record class UserResponse(Guid id, string login, string email, List<OrderResponse> orders);
    public record class SearchUserResponse(Guid id, string login);
    public record class PublicProfileUserResponse(Guid id, string login, List<GameResponse>games);
}
