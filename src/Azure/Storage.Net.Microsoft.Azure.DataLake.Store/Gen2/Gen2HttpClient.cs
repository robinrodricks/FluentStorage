using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Net.Microsoft.Azure.DataLake.Store.Gen2
{
   class Gen2HttpClient
   {
      private readonly AzureHttpClient _client;

      public Gen2HttpClient(string accountName)
      {
         _client = new AzureHttpClient($"https://{accountName}.dfs.core.windows.net/");
      }

      public async Task ListFilesystemsAsync()
      {
         await _client.HttpGetAsync("?resource=account");
      }
   }
}
