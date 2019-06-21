using System.Net.Http;
using System.Threading.Tasks;

namespace Storage.Net.Microsoft.Azure.DataLake.Store.Gen2
{
   class AzureHttpClient
   {
      private readonly HttpClient _http;
      private readonly string _baseUri;

      public AzureHttpClient(string baseUri)
      {
         _baseUri = baseUri.Trim('/');
         _http = new HttpClient();
      }

      public async Task HttpGetAsync(string url)
      {
         HttpResponseMessage response = await _http.GetAsync($"{_baseUri}/{url}");
      }
   }
}
