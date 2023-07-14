using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models.Dtos
{
    public class OrderDTO
    {
        [Key]
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("Book")]
        public int BookID { get; set; }

    }
}
