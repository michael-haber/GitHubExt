using System.Collections.Immutable;

namespace UserSearch.Api.Services
{
    public interface IRequestService
    {
        Task<HttpResponseMessage> SendGetRequest(string url, ImmutableDictionary<string, string>? headers = null);
    }
}
