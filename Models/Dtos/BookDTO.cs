using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Dtos
{
    public class BookDTO
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public float Price { get; set; } = 0;
        public bool Ordered { get; set; } = false;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set;} = string.Empty;

        public int UserID { get; set; }
    }
}
