using Dotnetdudes.Buyabob.Api.Models.Auspost;

namespace Dotnetdudes.Buyabob.Api.Services
{
    public sealed class AuspostService : IAuspostService
    {
        public readonly HttpClient _httpClient;

        public AuspostService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ShippingSizes> GetShippingSizesAsync()
        {
            var sizes = await _httpClient.GetFromJsonAsync<ShippingSizes>("/postage/parcel/domestic/size.json");
            return sizes ?? throw new Exception("Failed to get shipping sizes");
        }

        public async Task<ShippingServices> GetShippingServicesAsync(string from_postcode, string to_postcode, decimal weight, decimal width, decimal height, decimal length)
        {
            var services = await _httpClient.GetFromJsonAsync<ShippingServices>($"/postage/parcel/domestic/service.json?length={length}&width={width}&height={height}&weight={weight}&from_postcode={from_postcode}&to_postcode={to_postcode}");
            return services ?? throw new Exception("Failed to get shipping services");
        }
    }
}