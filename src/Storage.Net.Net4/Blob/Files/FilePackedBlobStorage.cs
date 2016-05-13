using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Net.Blob.Files
{
   /// <summary>
   /// File based blob storage packed into one single file.
   /// </summary>
   public class FilePackedBlobStorage : IBlobStorage
   {
      public FilePackedBlobStorage(Stream fileStream)
      {
         
      }

      public IEnumerable<string> List(string prefix)
      {
         throw new NotImplementedException();
      }

      public void Delete(string id)
      {
         throw new NotImplementedException();
      }

      public void UploadFromStream(string id, Stream sourceStream)
      {
         throw new NotImplementedException();
      }

      public void DownloadToStream(string id, Stream targetStream)
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
   }
}
