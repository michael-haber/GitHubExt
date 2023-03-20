using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using UserSearch.Data.GitHub;

namespace UserSearch.App.Pages
{
    public class SearchUserModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly INotyfService _notyf;

        public UserSearchResult UserSearchResult { get; set; } = new UserSearchResult();        
        public UserInfoModel UserInfoModel { get; set; } = new UserInfoModel();

        #region Search Properties
        [BindProperty]
        public string UserSearchTerm { get; set; } = string.Empty;
        
        [BindProperty]
        public int PageNumber { get; set; }

        [BindProperty]
        public int TotalResults { get; set; }

        [BindProperty]
        public string QueryElapsedTime { get; set; } = string.Empty;
        #endregion // Search Properties

        #region Pagination Properties
        [BindProperty]
        public string FirstPage { get; set; } = string.Empty;

        [BindProperty]
        public string PrevPage { get; set; } = string.Empty;

        [BindProperty]
        public string NextPage { get; set; } = string.Empty;

        [BindProperty]
        public string LastPage { get; set; } = string.Empty;
        #endregion // Pagination Properties


        public SearchUserModel(ILogger<IndexModel> logger, INotyfService toastNotification)
        {
            _logger = logger;
            _notyf = toastNotification;
        }

        #region Handlers
        /// <summary>
        /// Used for paginating search results
        /// </summary>
        /// <param name="CurrentPage">The page within the list of paginated results to display</param>
        /// <param name="Term">The search term from which results originate</param>
        /// <returns>Returns the page</returns>
        public async Task<IActionResult> OnGetNavigateAsync(string CurrentPage, string Term)
        {
            UserSearchTerm= Term;

            if (string.IsNullOrWhiteSpace(UserSearchTerm))
            {
                string msg = "Search term missing";

                _logger.LogError(msg);

                return BadRequest(msg);
            }

            if (!int.TryParse(CurrentPage, out var pageNumber))
            {
                string msg = "Search component missing";

                _logger.LogError($"{msg} - CurrentPage");

                return BadRequest("Search component missing");
            }

            await InitSearch(pageNumber);

            return Page();
        }

        /// <summary>
        /// Initiate a search and display the first page of potentially paginated results
        /// </summary>
        /// <returns>Returns the page</returns>
        public async Task<IActionResult> OnPostSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(UserSearchTerm))
            {
                string msg = "Search term missing";

                _logger.LogError(msg);

                return BadRequest(msg);
            }

            await InitSearch(1);

            return Page();
        }

        /// <summary>
        /// Initiate the population of user details to be displayed in a modal dialog
        /// </summary>
        /// <param name="userLogin">User login from which to retrieve detailed information</param>
        /// <returns>Returns the partial view</returns>
        public async Task<IActionResult> OnGetRenderUserInfo(string userLogin)
        {
            UserInfoModel = await PopulateUserInfo(userLogin);

            return new PartialViewResult
            {
                ViewName = "_UserInfoPartial",
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<UserInfoModel>(
                    ViewData, UserInfoModel)
            };
        }
        #endregion // Handlers

        #region Helper Methods
        /// <summary>
        /// Method to perform Web API call to retrieve results, based on various parameters, from a user search of GitHub
        /// </summary>
        /// <param name="curPage">The current page of results to retrieve</param>
        /// <returns>N/A</returns>
        private async Task InitSearch(int curPage)
        {
            using (var httpClient = new HttpClient())
            {
                string url = Data.Constants.UserSearchWebApiUrl;

                // Construct search param object
                var searchParam = new UserSearchParameters()
                {
                    Term = HttpUtility.UrlEncode(UserSearchTerm),
                    ResultsPerPage = 30, // TODO: We can set this to be a customizable value, later
                    PageNumber = curPage
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(searchParam),
                    System.Text.Encoding.UTF8,
                    "application/json");

                // Keep track of search runtime
                Stopwatch queryWatch = new Stopwatch();
                queryWatch.Start();

                // Call to the web API to retrieve search data
                using (HttpResponseMessage response = await httpClient.PostAsync(url, content))
                {
                    string queryTime = $"{ queryWatch.ElapsedMilliseconds }ms";
                    QueryElapsedTime = $"User Search Web API Execution: {queryTime}";

                    if (response.IsSuccessStatusCode)
                    { // If the web API executed successfully
                        
                        UserSearchResult = await response.Content.ReadFromJsonAsync<UserSearchResult>() ?? new UserSearchResult();

                        TotalResults = UserSearchResult?.TotalCount ?? -1;

                        ParseLinkHeader(response.Headers);

                        _notyf.Success($"{UserSearchResult?.Items.Count()} users in {queryTime}");
                    }
                    else
                    { // If the web API encountered an error
                        response.Headers.TryGetValues("ErrorMessage", out var errorMesssageHeader);
                        if (!string.IsNullOrEmpty(errorMesssageHeader?.FirstOrDefault()))
                        { // If the web API published an error message, have notyf render error message
                            _notyf.Error(errorMesssageHeader.FirstOrDefault());
                        }
                        else
                        {
                            _logger.LogError("Call to {WebApiUrl} failed with no provided detail from Web API", url);
                        }

                        _logger.LogError("Call to {WebApiUrl} with {UserSearchParameters} failed. Error: {ErrorMessage} Response: {response}",
                            url, searchParam, errorMesssageHeader?.FirstOrDefault(), response);
                    }
                }

                // Keep track of the current page
                PageNumber = curPage;
            }
        }

        /// <summary>
        /// Method to perform Web API call to retrieve detailed information about a GitHub user
        /// </summary>
        /// <param name="userLogin">The user login on which to reetrieve details</param>
        /// <returns>Returns </returns>
        private async Task<UserInfoModel> PopulateUserInfo(string userLogin)
        {
            using (var httpClient = new HttpClient())
            {
                string url = $"{Data.Constants.UserInfoWebApiUrl}?userLogin={userLogin}";

                // Keep track of search runtime
                Stopwatch queryWatch = new Stopwatch();
                queryWatch.Start();

                // Call to the web API to retrieve detailed user information
                using (HttpResponseMessage response = await httpClient.GetAsync(url))
                {
                    UserInfoModel userInfo = new UserInfoModel();

                    string queryTime = $"{ queryWatch.ElapsedMilliseconds }ms";
                    QueryElapsedTime = $"User Detail Web API Execution: {queryTime}";

                    if (response.IsSuccessStatusCode)
                    { // If the web API executed successfully

                        userInfo = await response.Content.ReadFromJsonAsync<UserInfoModel>() ?? new UserInfoModel();
                        _notyf.Success($"User detail in {queryTime}");
                    }
                    else
                    { // If the web API encountered an error

                        response.Headers.TryGetValues("ErrorMessage", out var errorMesssageHeader);
                        if (!string.IsNullOrEmpty(errorMesssageHeader?.FirstOrDefault()))
                        { // If the web API published an error message, have notyf render error message

                            _notyf.Error(errorMesssageHeader.FirstOrDefault());
                        }
                        else
                        {
                            _logger.LogError("Call to {WebApiUrl} failed with no provided detail from Web API", url);
                        }

                        _logger.LogError("Call to {WebApiUrl} with {UserLogin} failed. Error: {ErrorMessage} Response: {response}",
                            url, userLogin, errorMesssageHeader?.FirstOrDefault(), response);
                    }

                    return userInfo;
                }
            }
        }

        /// <summary>
        /// This method is used to pull data from the Link http header passed back from GitHub and 
        /// sets local bound properties to corresponding values of pagination controls
        /// </summary>
        /// <param name="headers">Http headers passed back from GitHub API</param>
        /// <returns>N/A</returns>
        private void ParseLinkHeader(HttpResponseHeaders headers)
        {
            headers.TryGetValues("Link", out var linksString);
            string linkString = linksString?.FirstOrDefault() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(linkString)) return;

            var links = linkString.Split(", ").Reverse();

            if (links == null) return;

            FirstPage = FindAndTrim(links, ">; rel=\"first\"");
            PrevPage = FindAndTrim(links, ">; rel=\"prev\"");
            NextPage = FindAndTrim(links, ">; rel=\"next\"");
            LastPage = FindAndTrim(links, ">; rel=\"last\"");
        }

        /// <summary>
        /// This method searches the provided Link header value for a provided search term and 
        /// returns the corresponding page number. This is highly dependent upon the data returned
        /// through the GitHub API
        /// </summary>
        /// <param name="links">Link Header value split on ", " from http response</param>
        /// <param name="searchTerm">The portion of the string following the page number</param>
        /// <returns>Returns the page number value matching the requested search term</returns>
        private string FindAndTrim(IEnumerable<string> links, string searchTerm)
        {
            // Paginated response sample:
            // 
            // link: <https://api.github.com/repositories/1300192/issues?page=2>; rel="prev", <https://api.github.com/repositories/1300192/issues?page=4>; rel="next", <https://api.github.com/repositories/1300192/issues?page=515>; rel="last", <https://api.github.com/repositories/1300192/issues?page=1>; rel="first"
            //

            return links.FirstOrDefault(l => l.Contains(searchTerm, StringComparison.Ordinal))
                ?.Replace(searchTerm, string.Empty)
                ?.Split("=").LastOrDefault() ?? string.Empty;
        }
        #endregion // Helper Methods
    }
}
