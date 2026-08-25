using DigitalDistribution.Models;

namespace DigitalDistribution.Contracts
{
    public record KeyResponse(Guid id, string value, KeyStatus status, Guid gameId, string gameName, decimal price);
}
