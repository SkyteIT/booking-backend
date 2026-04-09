namespace Ube.Application.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public int ListingCount { get; set; }
    }
}