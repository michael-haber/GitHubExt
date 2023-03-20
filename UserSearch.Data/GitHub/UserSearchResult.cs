using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UserSearch.Data.GitHub
{
    public class UserSearchResult
    {
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }

        [JsonPropertyName("incomplete_results")]
        public bool IncompleteResults { get; set; }

        [JsonPropertyName("items")]
        public IEnumerable<UserSearchModel> Items { get; set; }
    }
}
