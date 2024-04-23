// Interface for AuspostService

using System.Threading.Tasks;
using Dotnetdudes.Buyabob.Api.Models.Auspost;

namespace Dotnetdudes.Buyabob.Api.Services
{
    public interface IAuspostService
    {
        Task<ShippingSizes> GetShippingSizesAsync();
    }
}