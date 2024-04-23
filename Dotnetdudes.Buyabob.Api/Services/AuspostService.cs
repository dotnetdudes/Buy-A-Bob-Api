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
    }
}