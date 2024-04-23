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

            return group;
        }
    }
}