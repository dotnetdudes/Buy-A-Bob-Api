namespace Dotnetdudes.Buyabob.Api.Models
{
    public class ShippingAddress
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }

        public int AddressId { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Updated { get; set; }

        public DateTime? Deleted { get; set; }
    }
}
