using System.Text.Json.Serialization;

namespace Dotnetdudes.Buyabob.Api.Models.Auspost
{
    public class Cost
    {
        [JsonPropertyName("item"), JsonRequired]
        public required string Item { get; set; }

        [JsonPropertyName("cost"), JsonRequired]
        public required string price { get; set; }
    }

    public class Costs
    {
        [JsonPropertyName("cost"), JsonRequired]
        public required Cost PostageCost { get; set; }
    }

    public class PostageResult
    {
        [JsonPropertyName("service"), JsonRequired]
        public required string Service { get; set; }

        [JsonPropertyName("delivery_time")]
        public string? DeliveryTime { get; set; }

        [JsonPropertyName("total_cost"), JsonRequired]
        public required string TotalCost { get; set; }

        [JsonPropertyName("costs"), JsonRequired]
        public required Costs ShipingCosts { get; set; }
    }

    public class ShippingCost
    {
        [JsonPropertyName("postage_result"), JsonRequired]
        public required PostageResult PostageResult { get; set; }
    }

}