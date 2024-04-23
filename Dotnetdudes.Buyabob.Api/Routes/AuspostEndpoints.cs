using Dotnetdudes.Buyabob.Api.Models.Auspost;
using Dotnetdudes.Buyabob.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;
using System.Text.Json;

namespace Dotnetdudes.Buyabob.Api.Routes
{
    public static class AuspostEndpoints
    {
        public static RouteGroupBuilder MapAuspostEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/shipping-sizes", async Task<Results<JsonHttpResult<ShippingSizes>, NotFound>> (AuspostService auspost) =>
            {
                var sizes = await auspost.GetShippingSizesAsync();
                return sizes is null ? TypedResults.NotFound() : TypedResults.Json(sizes);
            });

            group.MapGet("/shipping-services", async Task<Results<JsonHttpResult<ShippingServices>, NotFound>> (AuspostService auspost, string from_postcode, string to_postcode, decimal weight, decimal width, decimal height, decimal length) =>
            {
                var services = await auspost.GetShippingServicesAsync(from_postcode, to_postcode, weight, width, height, length);
                return services is null ? TypedResults.NotFound() : TypedResults.Json(services);
            });

            return group;
        }
    }
}