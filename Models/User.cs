using Microsoft.EntityFrameworkCore;

namespace DigitalDistribution.Models
{
    [Index(nameof(Login), IsUnique = true), Index(nameof(Email), IsUnique = true)]
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
        public UserRole Role { get; set; } = UserRole.User;
        public bool IsDeleted { get; set; } = false;
        public ICollection<Order>? Orders { get; set; } = new List<Order>();
    }
    public enum UserRole
    {
        User,
        Admin
    }
}
