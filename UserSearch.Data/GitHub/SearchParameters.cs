using System.Text.Json.Serialization;

namespace UserSearch.Data.GitHub
{
    public class SearchParameters
    {
        /// <summary>
        /// Page number of the results to fetch.
        /// Default: 1
        /// </summary>
        [JsonPropertyName("page")]
        public int PageNumber { get; set; }

        /// <summary>
        /// The number of results per page (max 100).
        /// Default: 30
        /// </summary>
        [JsonPropertyName("per_page")]
        public int ResultsPerPage { get; set; }
    }
}
