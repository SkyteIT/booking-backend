namespace Ube.Domain.Entities.Listings
{
    public class ListingImage
    {
        public Guid Id { get; set; }

        public Guid ListingId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;

        //  Navigation Property
        public Listing Listing { get; set; }
    }
}