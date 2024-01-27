namespace Dotnetdudes.Buyabob.Api.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string Identifier { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int AddressId { get; set; }
    }
}
