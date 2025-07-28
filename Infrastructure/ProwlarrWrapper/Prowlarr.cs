using System.Net.Http.Json;
using System.Web;
using Infrastructure.ProwlarrWrapper.Models;

namespace Infrastructure.ProwlarrWrapper
{
    public class Prowlarr
    {
        private readonly HttpClient _baseHttpClient = new HttpClient();
        private readonly string _apiKey;
        private readonly string _apiUrl;

        /*********************************
        ******** Constructor     *********
        *********************************/

        public Prowlarr(string apiUrl, string apiKey)
        {
            _apiUrl = apiUrl;
            _apiKey = apiKey;

            _baseHttpClient.BaseAddress = new Uri(_apiUrl.EndsWith('/') ? _apiUrl : _apiUrl + "/");
            _baseHttpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
        }

        /*********************************
        ******** Private Methods *********
        *********************************/

        private static string BuildSearchQuery(SearchOptions options)
        {
            var queryParams = HttpUtility.ParseQueryString(string.Empty);

            if (!string.IsNullOrWhiteSpace(options.Query))
                queryParams["query"] = options.Query;

            if (!string.IsNullOrWhiteSpace(options.Type))
                queryParams["type"] = options.Type;

            foreach (var indexerId in options.IndexerIds ?? Enumerable.Empty<int>())
                queryParams.Add("indexerIds", indexerId.ToString());

            foreach (var categoryId in options.CategoryIds ?? Enumerable.Empty<int>())
                queryParams.Add("categories", categoryId.ToString());

            queryParams["limit"] = options.Limit.ToString();
            queryParams["offset"] = options.Offset.ToString();

            return queryParams.ToString();
        }

        private async Task<T> GetResponse<T>(string subUrl)
        {
            var response = await _baseHttpClient.GetAsync(subUrl);
            response.EnsureSuccessStatusCode();

            var indexers = await response.Content.ReadFromJsonAsync<T>();

            if (indexers == null)
            {
                throw new InvalidOperationException("Expected JSON but received null.");
            }

            return indexers;
        }

        /*********************************
        ******** Public  Methods *********
        *********************************/

        public async Task<List<Indexer>> GetIndexersAsync()
        {
            return await GetResponse<List<Indexer>>("/api/v1/indexer");
        }

        public async Task<List<SearchResult>> Search(SearchOptions options)
        {
            string query = "/api/v1/search?" + BuildSearchQuery(options);

            System.Console.WriteLine(query);

            return await GetResponse<List<SearchResult>>(query);
        }
    }
}