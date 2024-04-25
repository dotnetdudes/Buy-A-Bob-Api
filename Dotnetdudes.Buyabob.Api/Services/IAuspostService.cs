// Interface for AuspostService

using Dotnetdudes.Buyabob.Api.Models.Auspost;

namespace Dotnetdudes.Buyabob.Api.Services
{
    public interface IAuspostService
    {
        Task<ShippingSizes> GetShippingSizesDomestic();
        Task<ShippingServices> GetShippingServicesDomestic(string from_postcode, string to_postcode, decimal weight, decimal width, decimal height, decimal length);
        Task<ShippingCost> GetShippingCostDomestic(string from_postcode, string to_postcode, decimal weight, decimal width, decimal height, decimal length, string service_code);

        Task<ValidCountries> GetValidShippingCountries();
        Task<ShippingServices> GetShippingServicesInternational(string countryCode, decimal weight);
        Task<ShippingCost> GetShippingCostInternational(string countryCode, decimal weight, string serviceCode);

    }
}