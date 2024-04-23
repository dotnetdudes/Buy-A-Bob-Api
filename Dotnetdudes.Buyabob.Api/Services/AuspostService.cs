using Dotnetdudes.Buyabob.Api.Models.Auspost;
using Polly;
using Polly.Retry;

namespace Dotnetdudes.Buyabob.Api.Services
{
    public sealed class AuspostService : IAuspostService
    {
        public readonly HttpClient _httpClient;
        private readonly ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions()) // Add retry using the default options
        .AddTimeout(TimeSpan.FromSeconds(10)) // Add 10 seconds timeout
        .Build();

        public AuspostService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ShippingSizes> GetShippingSizesAsync()
        {
            // var sizes = await _httpClient.GetFromJsonAsync<ShippingSizes>("/postage/parcel/domestic/size.json");
            var sizes = await pipeline.ExecuteAsync(async (token) => await _httpClient.GetFromJsonAsync<ShippingSizes>("/postage/parcel/domestic/size.json"));
            return sizes ?? throw new Exception("Failed to get shipping sizes");
        }

        public async Task<ShippingServices> GetShippingServicesAsync(string from_postcode, string to_postcode, decimal weight, decimal width, decimal height, decimal length)
        {
            // var shippingservices = await _httpClient.GetFromJsonAsync<ShippingServices>($"/postage/parcel/domestic/service.json?length={length}&width={width}&height={height}&weight={weight}&from_postcode={from_postcode}&to_postcode={to_postcode}");
            var shippingservices = await pipeline.ExecuteAsync(async (token) => await _httpClient.GetFromJsonAsync<ShippingServices>($"/postage/parcel/domestic/service.json?length={length}&width={width}&height={height}&weight={weight}&from_postcode={from_postcode}&to_postcode={to_postcode}"));
            return shippingservices ?? throw new Exception("Failed to get shipping services");
        }
    }
}