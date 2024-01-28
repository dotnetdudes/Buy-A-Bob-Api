namespace Dotnetdudes.Buyabob.Api.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }

        // price at time of purchase
        public decimal Price { get; set; }

        // quantity purchased
        public int Quantity { get; set; }

        // date and time of purchase
        public DateTime Created { get; set; } = DateTime.UtcNow;

        // date and time of last update
        public DateTime? Updated { get; set; }

        // date and time of deletion
        public DateTime? Deleted { get; set; }
    }
}
