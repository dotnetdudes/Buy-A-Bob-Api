using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dotnetdudes.Buyabob.Api.Models.Auspost
{

    public class ShippingSizes
    {
        [JsonPropertyName("sizes"), JsonRequired]
        public required Sizes Sizes { get; set; }
    }

    public class Size
    {
        [JsonPropertyName("code"), JsonRequired]
        public required string Code { get; set; }
        [JsonPropertyName("name"), JsonRequired]
        public required string Name { get; set; }
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    public class Sizes
    {
        [JsonPropertyName("size"), JsonRequired]
        public required List<Size> Size { get; set; }
    }
}