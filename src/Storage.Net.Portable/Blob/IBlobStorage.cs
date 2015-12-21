using System.Collections.Generic;
using System.IO;

namespace Storage.Net.Blob
{
   public interface IBlobStorage
   {
      /// <summary>
      /// Returns the list of available blobs
      /// </summary>
      /// <param name="prefix">Blob prefix to filter by. When null returns all blobs.</param>
      /// <returns>List of blob IDs</returns>
      IEnumerable<string> List(string prefix);

      void Delete(string id);

      /// <summary>
      /// Uploads a new blob. When a blob with identical name already exists overwrites it.
      /// </summary>
      /// <param name="id">Blob ID.</param>
      /// <param name="sourceStream">Source stream to copy from.</param>
      void UploadFromStream(string id, Stream sourceStream);

      /// <summary>
      /// Downloads blob to a stream
      /// </summary>
      /// <param name="id">Blob ID</param>
      /// <param name="targetStream">Target stream to copy to</param>
      /// <exception cref="FileNotFoundException">Thrown when blob does not exist</exception>
      void DownloadToStream(string id, Stream targetStream);

      /// <summary>
      /// Opens the stream asynchronously to read on demand.
      /// </summary>
      /// <param name="id">Blob ID</param>
      /// <returns></returns>
      Stream OpenStreamToRead(string id);

      /// <summary>
      /// Checks if a blob exists
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      bool Exists(string id);
   }
}
