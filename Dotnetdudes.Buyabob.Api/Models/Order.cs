namespace Dotnetdudes.Buyabob.Api.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int StatusId { get; set; }

        // total price of the order
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }

        public int ShippingTypeId { get; set; }

        public decimal Shipping { get; set; }

        public int ShippingAddressId { get; set; }

        public string? ContactName { get; set; }

        public string? ContactPhone { get; set; }

        // total price of the order
        public decimal Total { get; set; }

        // date and time of purchase
        public DateTime DatePurchased { get; set; } = DateTime.UtcNow;

        public DateTime? DateShipped { get; set; }

        // updated
        public DateTime? Updated { get; set; }

        // deleted
        public DateTime? Deleted { get; set; }
    }
}
