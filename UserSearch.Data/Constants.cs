using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserSearch.Data
{
    // TODO: Store these in something like AWS parameter store, config/appsettings file or database,
    // so updates do not become code changes
    public static class Constants
    {
#pragma warning disable S1075 // URIs should not be hardcoded
        public const string UserSearchGitHubApiUrl = "https://api.github.com/search/users";
        public const string UserInfoGitHubApiUrl = "https://api.github.com/users";
        public const string UserSearchWebApiUrl = "http://localhost:5188/api/UserSearch";
        public const string UserInfoWebApiUrl = "http://localhost:5188/api/UserSearch/UserDetail";
#pragma warning restore S1075 // URIs should not be hardcoded
        public static readonly ImmutableDictionary<string, string> GitHubApiRequestHeaders = new Dictionary<string, string>
        {
            { "Accept", "application/vnd.github+json" },
            { "X-GitHub-Api-Version", "2022-11-28" },
            { "User-Agent", "request" }
        }.ToImmutableDictionary();
    }
}
