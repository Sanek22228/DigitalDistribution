namespace DigitalDistribution.Contracts
{
    public record GameRequest(string name, decimal price);
    public record UpdateGameRequest(string? name, decimal? price);
}
