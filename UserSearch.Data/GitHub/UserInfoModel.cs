using System.Text.Json.Serialization;

namespace UserSearch.Data.GitHub
{
    public class UserInfoModel : UserSearchModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("company")]
        public string Company { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("bio")]
        public string Bio { get; set; }

        [JsonPropertyName("followers")]
        public int Followers { get; set; }

        [JsonPropertyName("following")]
        public int Following { get; set; }

        [JsonPropertyName("created_at")]
        public System.DateTime Created { get; set; }

        [JsonPropertyName("public_repos")]
        public int PublicRepos { get; set; }

        //"blog": "",
        //"hireable": null,
        //"twitter_username": null,
        //"public_gists": 0,
        //"updated_at": "2021-02-05T06:03:44Z"
    }
}
