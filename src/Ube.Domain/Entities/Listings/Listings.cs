using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ube.Domain.Entities.Listings
{
    public class Listing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [Required]
        public string? Title { get; set; }

        // Foreign key
        public int CategoryId { get; set; }

        // Navigation property
        public Category? Category { get; set; }
    }
}