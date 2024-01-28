namespace Dotnetdudes.Buyabob.Api.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Updated { get; set; }

        public DateTime? Deleted { get; set; }
    }
}
