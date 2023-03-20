using Microsoft.AspNetCore.Mvc;
using System.Net;
using UserSearch.Api.Services;
using UserSearch.Data;
using UserSearch.Data.GitHub;

namespace UserSearch.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserSearchController : ControllerBase
    {
        private readonly ILogger<UserSearchController> _logger;
        private readonly IRequestService _requestService;

        public UserSearchController(ILogger<UserSearchController> logger, RequestService requestService)
        {
            _logger = logger;
            _requestService = requestService;
        }

        /// <summary>
        /// POST: api/UserSearch/SearchUsers
        /// Takes in a search term and uses the GitHub API to match on user names
        /// </summary>
        /// <param name="searchParams">Object whose properties will customize the search</param>
        /// <returns>Returns an IActionResult wrapping a collection of basic data for each user 
        /// matching on the provided search term 
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> SearchUsers(UserSearchParameters searchParams)
        {
            HttpResponseMessage? response = null;

            try
            {
                using (_logger.BeginScope(searchParams))
                {
                    if (string.IsNullOrWhiteSpace(searchParams.Term))
                    {
                        return BadRequest(response);
                    }

                    string term = $"q={searchParams.Term}";
                    string resultsPerPage = searchParams.ResultsPerPage == 0 ? string.Empty : $"per_page={searchParams.ResultsPerPage}";
                    string pageNumber = searchParams.PageNumber == 0 ? string.Empty : $"page={searchParams.PageNumber}";

                    // Construct url
                    string url = $"{Constants.UserSearchGitHubApiUrl}?{term}";

                    if (!string.IsNullOrWhiteSpace(resultsPerPage))
                    { // Conditionally include results per page query string param
                        url += $"&{resultsPerPage}";
                    }
                    if (!string.IsNullOrWhiteSpace(pageNumber))
                    { // Conditionally include page number query string param
                        url += $"&{pageNumber}";
                    }

                    // Send request to GitHub API
                    response = await _requestService.SendGetRequest(url, Constants.GitHubApiRequestHeaders);

                    // Make sure request was successful, throw exception if not
                    response.EnsureSuccessStatusCode();

                    // Convert returned data into UserSearchResult entity
                    UserSearchResult? result = await response.Content.ReadFromJsonAsync<UserSearchResult>();

                    if (response.Headers.Contains("Link"))
                    { // Pass through Link header to caller
                        HttpContext.Response.Headers.Add("Link", response.Headers.GetValues("Link").FirstOrDefault());
                    }

                    return Ok(result);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Exception encountered while searching users -- returned: {HttpStatusMessage} ({HttpStatusCode})",
                    response?.ReasonPhrase, response?.StatusCode);

                // Handle custom GitHub api rate limit exception 
                if (response?.StatusCode == HttpStatusCode.Forbidden && response?.ReasonPhrase?.CompareTo("rate limit exceeded") == 0)
                {
                    HttpContext.Response.Headers.Add("ErrorMessage", "Search limit exceeded. Wait one minute");
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception encountered while searching users");

                return BadRequest(response);
            }
        }

        /// <summary>
        /// GET api/UserSearch/UserDetail
        /// Takes in a user's login and uses the GitHub API to provide additional detail on that user.
        /// </summary>
        /// <param name="userLogin">GitHub login</param>
        /// <returns>
        /// Returns an IActionResult wrapping an object detailing information on provided user 
        /// </returns>
        [HttpGet("UserDetail")]
        public async Task<IActionResult> GetUserDetail(string userLogin)
        {
            HttpResponseMessage? response = null;

            try
            {
                using (_logger.BeginScope(userLogin))
                {
                    if (string.IsNullOrWhiteSpace(userLogin))
                    {
                        return BadRequest(response);
                    }

                    // Construct url
                    string url = $"{Constants.UserInfoGitHubApiUrl}/{userLogin}";

                    // Send request to GitHub API
                    response = await _requestService.SendGetRequest(url, Constants.GitHubApiRequestHeaders);

                    // Make sure request was successful, throw exception if not
                    response.EnsureSuccessStatusCode();

                    // Convert returned data into UserSearchResult entity
                    UserInfoModel? result = await response.Content.ReadFromJsonAsync<UserInfoModel>();

                    return Ok(result);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Exception encountered while populating user info -- returned: {HttpStatusMessage} ({HttpStatusCode})",
                    response?.ReasonPhrase, response?.StatusCode);

                // Handle custom GitHub api rate limit exception 
                if (response?.StatusCode == HttpStatusCode.Forbidden && response?.ReasonPhrase?.CompareTo("rate limit exceeded") == 0)
                {
                    HttpContext.Response.Headers.Add("ErrorMessage", "Search limit exceeded. Wait one minute");
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception encountered while populating user info");

                return BadRequest(response);
            }
        }
    }
}