using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OneDrive.Sdk;
using Storage.Net.Blob;

namespace Storage.Net.OneDrive.Blob
{
   //this is in progress and higly experimental, see https://github.com/onedrive/onedrive-sdk-csharp
   //to continue
   public class OneDriveBlobStorage : IBlobStorage
   {
      private OneDriveClient _client;

      public OneDriveBlobStorage(string clientId, string returnUrl)
      {
         //_client = OneDriveClient.GetMicrosoftAccountClient(clientId, returnUrl, )
      }

      public IEnumerable<string> List(string prefix)
      {
         throw new NotImplementedException();
      }

      public void Delete(string id)
      {
         throw new NotImplementedException();
      }

      public Stream OpenStreamToRead(string id)
      {
         throw new NotImplementedException();
      }

      public bool Exists(string id)
      {
         throw new NotImplementedException();
      }

      public void DownloadToStream(string id, Stream targetStream)
      {
         throw new NotImplementedException();
      }

      public void UploadFromStream(string id, Stream sourceStream)
      {
         throw new NotImplementedException();
      }
   }
}
