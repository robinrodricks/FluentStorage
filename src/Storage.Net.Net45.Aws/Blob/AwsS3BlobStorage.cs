using Storage.Net.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Storage.Net.Aws.Blob
{
   class AwsS3BlobStorage : IBlobStorage
   {
      public AwsS3BlobStorage()
      {
         //IAmazonS3 client;
      }

      public void Delete(string id)
      {
         throw new NotImplementedException();
      }

      public void DownloadToStream(string id, Stream targetStream)
      {
         throw new NotImplementedException();
      }

      public bool Exists(string id)
      {
         throw new NotImplementedException();
      }

      public IEnumerable<string> List(string prefix)
      {
         throw new NotImplementedException();
      }

      public Stream OpenStreamToRead(string id)
      {
         throw new NotImplementedException();
      }

      public void UploadFromStream(string id, Stream sourceStream)
      {
         throw new NotImplementedException();
      }
   }
}
