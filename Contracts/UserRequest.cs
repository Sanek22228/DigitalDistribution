namespace DigitalDistribution.Contracts
{
    public record class CreateUserRequest(string login, string email, string password);
    public record class LoginUserRequest(string userData, string password);
    public record class UpdateUserRequest(string? login, string? email, string? password);
}
