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

        public async Task<ShippingSizes> GetShippingSizesDomestic()
        {
            var sizes = await pipeline.ExecuteAsync(async (token) => await _httpClient.GetFromJsonAsync<ShippingSizes>("/postage/parcel/domestic/size.json"));
            return sizes ?? throw new Exception("Failed to get shipping sizes");
        }

        public async Task<ShippingServices> GetShippingServicesDomestic(string from_postcode, string to_postcode, decimal weight, decimal width, decimal height, decimal length)
        {
            var shippingservices = await pipeline.ExecuteAsync(async (token) => await _httpClient.GetFromJsonAsync<ShippingServices>($"/postage/parcel/domestic/service.json?length={length}&width={width}&height={height}&weight={weight}&from_postcode={from_postcode}&to_postcode={to_postcode}"));
            return shippingservices ?? throw new Exception("Failed to get shipping services");
        }

        public async Task<ShippingCost> GetShippingCostDomestic(string from_postcode, string to_postcode, decimal weight, decimal width, decimal height, decimal length, string service_code)
        {
            var shippingcost = await pipeline.ExecuteAsync(async (token) => await _httpClient.GetFromJsonAsync<ShippingCost>($"/postage/parcel/domestic/calculate.json?length={length}&width={width}&height={height}&weight={weight}&from_postcode={from_postcode}&to_postcode={to_postcode}&service_code={service_code}"));
            return shippingcost ?? throw new Exception("Failed to get shipping cost");
        }

        public async Task<ValidCountries> GetValidShippingCountries()
        {
            var countries = await pipeline.ExecuteAsync(async (token) => await _httpClient.GetFromJsonAsync<ValidCountries>("/postage/country.json"));
            return countries ?? throw new Exception("Failed to get valid shipping countries");
        }

        public async Task<ShippingServices> GetShippingServicesInternational(string countryCode, decimal weight)
        {
            var shippingservices = await pipeline.ExecuteAsync(async (token) => await _httpClient.GetFromJsonAsync<ShippingServices>($"/postage/parcel/international/service.json?country_code={countryCode}&weight={weight}"));
            return shippingservices ?? throw new Exception("Failed to get international shipping services");
        }

        public async Task<ShippingCost> GetShippingCostInternational(string countryCode, decimal weight, string serviceCode)
        {
            var shippingcost = await pipeline.ExecuteAsync(async (token) => await _httpClient.GetFromJsonAsync<ShippingCost>($"/postage/parcel/international/calculate.json?country_code={countryCode}&weight={weight}&service_code={serviceCode}"));
            return shippingcost ?? throw new Exception("Failed to get international shipping cost");
        }
    }
}