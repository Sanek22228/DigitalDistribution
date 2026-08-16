namespace DigitalDistribution.Models
{
    public class User
    {
        public User() { }
        public User(string login, string email, string password) 
        {
            Login = login;
            Email = email;
            Password = password;
        }
        public Guid Id { get; set; }
        public string Login { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int Role { get; set; }
        public ICollection<Order>? Orders { get; set; } = new List<Order>();
    }
}
