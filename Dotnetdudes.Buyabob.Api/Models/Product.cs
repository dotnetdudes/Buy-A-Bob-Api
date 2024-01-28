namespace Dotnetdudes.Buyabob.Api.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        // product dimensions and weight
        public decimal Weight { get; set; }
        public decimal Width { get; set; }
        public decimal Depth { get; set; }
        public decimal Height { get; set; }
        public int Quantity { get; set; }

        // created and updated dates in utc
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; }

        // sold
        public bool IsSold { get; set; } = false;
        public DateTime? SoldDate { get; set; }
        public DateTime? Deleted { get; set; }
    }
}
