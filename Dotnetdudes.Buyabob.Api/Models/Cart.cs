namespace Dotnetdudes.Buyabob.Api.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        // status
        public int StatusId { get; set; } = 1;

        public List<CartItem> Items { get; set; } = [];

        // created
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        // deleted
        public DateTime? Deleted { get; set; }
    }
}
