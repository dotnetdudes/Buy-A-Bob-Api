using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dotnetdudes.Buyabob.Api.Models.Auspost
{

    public class ShippingSizes
    {
        [JsonPropertyName("sizes"), JsonRequired]
        public Sizes? Sizes { get; set; }
    }

    public class Size
    {
        [JsonPropertyName("code"), JsonRequired]
        public string? Code { get; set; }
        [JsonPropertyName("name"), JsonRequired]
        public string? Name { get; set; }
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    public class Sizes
    {
        [JsonPropertyName("size"), JsonRequired]
        public List<Size>? Size { get; set; }
    }
}