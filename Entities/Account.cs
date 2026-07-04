using Hotel_System.Entities.Enums;

namespace Hotel_System.Entities
{
    public class Account : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Role Role { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Active;
    }
}