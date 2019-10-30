using System.Collections.Generic;
using System.Threading.Tasks;
using Storage.Net.Amazon.Aws;
using Storage.Net.Blobs;

namespace Storage.Net.ConsoleRunner
{
   class Program
   {
      static async Task Main(string[] args)
      {
         IReadOnlyCollection<string> profiles = AwsCliCredentials.EnumerateProfiles();

         IBlobStorage bs = StorageFactory.Blobs.AwsS3("***", "***", "eu-west-1");
         IReadOnlyCollection<Blob> all = await bs.ListAsync();

         //var ibs = StorageFactory.Blobs.AwsS3()
      }
   }
}
