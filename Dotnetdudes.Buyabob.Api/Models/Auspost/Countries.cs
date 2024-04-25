using System.Text.Json.Serialization;

namespace Dotnetdudes.Buyabob.Api.Models.Auspost
{
    public class Countries
    {
        [JsonPropertyName("country"), JsonRequired]
        public required List<Country> CountryList { get; set; }
    }

    public class Country
    {
        [JsonPropertyName("code"), JsonRequired]
        public required string Code { get; set; }

        [JsonPropertyName("name"), JsonRequired]
        public required string Name { get; set; }
    }

    public class ValidCountries
    {
        [JsonPropertyName("countries"), JsonRequired]
        public required Countries Countries { get; set; }
    }

}
