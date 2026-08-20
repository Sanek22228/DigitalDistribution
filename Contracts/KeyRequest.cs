using DigitalDistribution.Models;

namespace DigitalDistribution.Contracts
{
    public record KeyRequest(KeyStatus status, Guid gameId);
    public record UpdateKeyRequest(KeyStatus status);
}
