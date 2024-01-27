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
        public DateTime DatePurchased { get; set; } = DateTime.UtcNow;
    }
}
