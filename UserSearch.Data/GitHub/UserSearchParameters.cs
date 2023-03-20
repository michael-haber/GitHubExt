using System.Text.Json.Serialization;

namespace UserSearch.Data.GitHub
{
    public class UserSearchParameters : SearchParameters
    {
        /// <summary>
        /// Query contains one or more search keywords and qualifiers
        /// </summary>
        [JsonPropertyName("q")]
        public string Term { get; set; }

        /// <summary>
        /// Sorts the results of the query by number of followers, repositories or when the user joined
        /// Can be one of: followers, repositories, joined
        /// </summary>
        [JsonPropertyName("sort")]
        public string Sort { get; set; }

        /// <summary>
        /// Ordered by highest number of matches (desc) or lowest number of matches (asc)
        /// Can be one of: asc, desc
        /// Default: desc
        /// </summary>
        [JsonPropertyName("order")]
        public string Order { get; set; }
    }
}
