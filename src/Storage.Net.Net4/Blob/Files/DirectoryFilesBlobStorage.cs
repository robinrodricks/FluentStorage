using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Storage.Net.Blob.Files
{
   /// <summary>
   /// Blob storage implementation which uses local file system directory
   /// </summary>
   public class DirectoryFilesBlobStorage : IBlobStorage
   {
      private readonly DirectoryInfo _directory;

      /// <summary>
      /// Creates an instance in a specific disk directory
      /// </summary>
      public DirectoryFilesBlobStorage(DirectoryInfo directory)
      {
         _directory = directory;
      }

      /// <summary>
      /// Returns the list of blob names in this storage, optionally filtered by prefix
      /// </summary>
      public IEnumerable<string> List(string prefix)
      {
         if(!_directory.Exists) return null;

         return _directory
            .GetFiles(prefix == null ? "*" : (prefix.SanitizePath() + "*"))
            .Select(f => f.Name);
      }

      /// <summary>
      /// Deletes blob file
      /// </summary>
      /// <param name="id"></param>
      public void Delete(string id)
      {
         if(id == null) throw new ArgumentNullException(nameof(id));

         string path = GetFilePath(id);
         if(File.Exists(path)) File.Delete(path);
      }

      /// <summary>
      /// Writes blob to file
      /// </summary>
      public void UploadFromStream(string id, Stream sourceStream)
      {
         if(id == null) throw new ArgumentNullException(nameof(id));
         if(sourceStream == null) throw new ArgumentNullException(nameof(sourceStream));

         using(Stream target = CreateStream(id))
         {
            sourceStream.CopyTo(target);
         }
      }

      /// <summary>
      /// Reads blob from file and writes to the target stream
      /// </summary>
      public void DownloadToStream(string id, Stream targetStream)
      {
         if(id == null) throw new ArgumentNullException(nameof(id));
         if(targetStream == null) throw new ArgumentNullException(nameof(targetStream));

         using(Stream source = OpenStream(id))
         {
            if(source == null) return;

            source.CopyTo(targetStream);
            targetStream.Flush();
         }
      }

      /// <summary>
      /// Opens the blob as a readable stream
      /// </summary>
      public Stream OpenStreamToRead(string id)
      {
         if(id == null) throw new ArgumentNullException(nameof(id));

         return OpenStream(id);
      }

      /// <summary>
      /// Checks if file exists
      /// </summary>
      public bool Exists(string id)
      {
         if(id == null) throw new ArgumentNullException(nameof(id));

         return File.Exists(GetFilePath(id));
      }

      private string GetFilePath(string id)
      {
         return Path.Combine(_directory.FullName, id.SanitizePath());
      }

      private Stream CreateStream(string id)
      {
         if(!_directory.Exists) _directory.Create();
         string path = GetFilePath(id);

         return File.Create(path);
      }

      private Stream OpenStream(string id)
      {
         string path = GetFilePath(id);
         if(!File.Exists(path)) return null;

         return File.OpenRead(path);
      }
   }
}
