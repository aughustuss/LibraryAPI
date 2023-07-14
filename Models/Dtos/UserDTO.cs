using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Dtos
{
    public class UserDTO
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Blocked { get; set; } = false;
        public bool Active { get; set; } = true;
        public float Fine { get; set; } = 0;
        public string UserRole { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string? ConfirmEmailToken { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public DateTime TokenExpiration { get; set; }
        public ICollection<Book> Books { get; } = new List<Book>();
    }
}
