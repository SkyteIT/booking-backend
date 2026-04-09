using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ube.Domain.Entities.Listings; // Needed to reference Listing

namespace Ube.Domain.Entities.Listings // Same namespace as Listing
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 🔥 IMPORTANT
        public int Id { get; set; }

        [Required] // Optional but good practice
        public string? Name { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    }
}