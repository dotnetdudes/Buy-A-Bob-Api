namespace Dotnetdudes.Buyabob.Api.Models
{
    public class Address
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Street { get; set; } = string.Empty;

        public string Suburb { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Postcode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Updated { get; set; }
        public DateTime? Deleted { get; set; }
    }
}
