namespace Dotnetdudes.Buyabob.Api.Models
{
    public class ProductCategory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CategoryId { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Deleted { get; set; }

    }
}
