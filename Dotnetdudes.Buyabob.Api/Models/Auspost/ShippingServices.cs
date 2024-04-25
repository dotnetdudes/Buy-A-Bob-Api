using System.Text.Json.Serialization;

namespace Dotnetdudes.Buyabob.Api.Models.Auspost
{
    public class Option
    {
        [JsonPropertyName("code"), JsonRequired]
        public required string Code { get; set; }

        [JsonPropertyName("name"), JsonRequired]
        public required string Name { get; set; }

        [JsonPropertyName("suboptions")]
        public SubOptions? Suboptions { get; set; }

        [JsonPropertyName("max_extra_cover")]
        public int? MaxExtraCover { get; set; }
    }

    public class Options
    {
        [JsonPropertyName("option")]
        public List<Option>? SingleOption { get; set; }
    }
    public class ShippingServices
    {
        [JsonPropertyName("services"), JsonRequired]
        public required Services Services { get; set; }
    }

    public class Service
    {
        [JsonPropertyName("code"), JsonRequired]
        public required string Code { get; set; }
        [JsonPropertyName("name"), JsonRequired]
        public required string Name { get; set; }
        [JsonPropertyName("price"), JsonRequired]
        public required string Price { get; set; }
        [JsonPropertyName("max_extra_cover")]
        public int? MaxExtraCover { get; set; }
        [JsonPropertyName("options")]
        public Options? OptionsList { get; set; }
    }

    public class Services
    {
        [JsonPropertyName("service"), JsonRequired]
        public required List<Service> Service { get; set; }
    }

    public class SubOptions
    {
        [JsonPropertyName("option")]
        public Option? Suboption { get; set; }
    }

}