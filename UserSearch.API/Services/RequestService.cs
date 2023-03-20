using System.Collections.Immutable;
using UserSearch.Data.GitHub;

namespace UserSearch.Api.Services
{
    public class RequestService : IRequestService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public RequestService(ILogger<RequestService> logger, HttpClient client)
        {
            _logger = logger;
            _httpClient = client;
        }

        /// <summary>
        /// Make a request to the provided URL, setting any provided headers first
        /// </summary>
        /// <param name="url">URL to where request will be made</param>
        /// <param name="headers">Zero or more headers to include in request</param>
        /// <returns>Returns body of response as string</returns>
        public async Task<HttpResponseMessage> SendGetRequest(string url, ImmutableDictionary<string,string>? headers = null)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                using (_logger.BeginScope("Making an http request to {URL}", url))
                {
                    if (headers != null)
                    {
                        // Add any provided headers to the current request
                        foreach (var header in headers)
                        {
                            _logger.LogInformation("Adding Header {HeaderName}, {HeaderValue} to http request", header.Key, header.Value);
                            httpRequestMessage.Headers.Add(header.Key, header.Value);
                        }
                    }

                    httpRequestMessage.RequestUri = new Uri(url);

                    var response = await _httpClient.SendAsync(httpRequestMessage);
                    _logger.LogInformation("Http request complete");

                    return response;
                }
            }
        }
    }
}
