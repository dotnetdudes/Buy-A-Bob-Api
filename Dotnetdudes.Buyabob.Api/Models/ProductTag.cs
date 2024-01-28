namespace Dotnetdudes.Buyabob.Api.Models
{
    public class ProductTag
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int TagId { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Deleted { get; set; }
    }
}
