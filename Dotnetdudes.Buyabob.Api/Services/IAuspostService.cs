// Interface for AuspostService

using System.Threading.Tasks;
using Dotnetdudes.Buyabob.Api.Models.Auspost;

namespace Dotnetdudes.Buyabob.Api.Services
{
    public interface IAuspostService
    {
        Task<ShippingSizes> GetShippingSizesAsync();
        Task<ShippingServices> GetShippingServicesAsync(string from_postcode, string to_postcode, decimal weight, decimal width, decimal height, decimal length);
        Task<ShippingCost> GetShippingCostAsync(string from_postcode, string to_postcode, decimal weight, decimal width, decimal height, decimal length, string service_code);
    }
}