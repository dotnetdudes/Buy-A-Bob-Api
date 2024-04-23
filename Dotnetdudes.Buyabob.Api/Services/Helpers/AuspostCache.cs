
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;

namespace Dotnetdudes.Buyabob.Api.Services.Helpers
{
    public class AuspostCache : DelegatingHandler
    {
        private readonly IMemoryCache _cache;

        public AuspostCache(IMemoryCache cache)
        {
            _cache = cache;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Check if the request is a GET request
            // if it is, check if the request URI contains the word "size"
            // if it does, check if the response is already in the cache
            // if it is, return the response from the cache
            // if it is not, send the request to the server and cache the response
            // if the request is not a GET request, send the request to the server
            // use the in memory cache to store the response
            if (request.Method == HttpMethod.Get)
            {
                if (request.RequestUri.ToString().Contains("size"))
                {
                    if (_cache != null && _cache.TryGetValue("shippingSizes", out HttpResponseMessage response))
                    {
                        return response;
                    }
                    response = await base.SendAsync(request, cancellationToken);
                    if (_cache != null)
                    {
                        _cache.Set("shippingSizes", response, TimeSpan.FromHours(12));
                    }
                    return response;
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}