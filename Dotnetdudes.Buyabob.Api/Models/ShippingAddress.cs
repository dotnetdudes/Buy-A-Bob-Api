namespace Dotnetdudes.Buyabob.Api.Models
{
    public class ShippingAddress
    {
        public int Id { get; set; }
        public int OrderId { get; set; }

        public int AddressId { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Deleted { get; set; }
    }
}
