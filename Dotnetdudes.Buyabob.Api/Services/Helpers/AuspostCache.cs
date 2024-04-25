
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;

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

            ArgumentNullException.ThrowIfNull(request);

            if (request.Method == HttpMethod.Get)
            {
                if (request.RequestUri!.ToString().Contains("size"))
                {
                    if(_cache.TryGetValue(request.RequestUri.ToString(), out string? originalContent))
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(originalContent!, Encoding.UTF8, "application/json")
                        };
                    }
                    else
                    {
                        var response = await base.SendAsync(request, cancellationToken);
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                        _cache.Set(request.RequestUri.ToString(), content, TimeSpan.FromHours(1));
                        return response;
                    }
                }

                if (request.RequestUri!.ToString().Contains("countries"))
                {
                    if (_cache.TryGetValue("countries", out string? originalContent))
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(originalContent!, Encoding.UTF8, "application/json")
                        };
                    }
                    else
                    {
                        var response = await base.SendAsync(request, cancellationToken);
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                        _cache.Set("countries", content, TimeSpan.FromHours(12));
                        return response;
                    }
                }
            }
            
            return await base.SendAsync(request, cancellationToken);
        }
    }
}