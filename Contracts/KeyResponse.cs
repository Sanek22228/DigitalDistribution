using DigitalDistribution.Models;

namespace DigitalDistribution.Contracts
{
    public record KeyResponse(string value, KeyStatus status, Guid gameId);
}
