using NetBox.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Storage.Net.Blob.Files
{
   /// <summary>
   /// Blob storage implementation which uses local file system directory
   /// </summary>
   public class DirectoryFilesBlobStorage : AsyncBlobStorage
   {
      private readonly DirectoryInfo _directory;
      private static readonly string FsPathSeparator = new string(Path.DirectorySeparatorChar, 1);

      /// <summary>
      /// Creates an instance in a specific disk directory
      /// <param name="directory">Root directory</param>
      /// </summary>
      public DirectoryFilesBlobStorage(DirectoryInfo directory)
      {
         _directory = directory;
      }

      /// <summary>
      /// Returns the list of blob names in this storage, optionally filtered by prefix
      /// </summary>
      public override IEnumerable<string> List(string prefix)
      {
         GenericValidation.CheckBlobPrefix(prefix);

         if(!_directory.Exists) return null;

         string[] allIds = _directory.GetFiles("*", SearchOption.AllDirectories).Select(ToId).ToArray();

         if (string.IsNullOrEmpty(prefix)) return allIds;

         string wildcard = prefix + "*";
         return allIds.Where(id => id.MatchesWildcard(wildcard));
      }

      private string ToId(FileInfo fi)
      {
         string name = fi.FullName.Substring(_directory.FullName.Length + 1);

         name = name.Replace(Path.DirectorySeparatorChar, StoragePath.PathSeparator);

         string[] parts = name.Split(StoragePath.PathSeparator);

         return string.Join(StoragePath.PathStrSeparator, parts.Select(DecodePathPart));
      }

      /// <summary>
      /// Deletes blob file
      /// </summary>
      /// <param name="id"></param>
      public override void Delete(string id)
      {
         GenericValidation.CheckBlobId(id);

         string path = GetFilePath(id);
         if(File.Exists(path)) File.Delete(path);
      }

      /// <summary>
      /// Writes blob to file
      /// </summary>
      public override Stream OpenWrite(string id)
      {
         GenericValidation.CheckBlobId(id);

         return CreateStream(id);
      }

      /// <summary>
      /// Append chunk to file
      /// </summary>
      public override Stream OpenAppend(string id)
      {
         GenericValidation.CheckBlobId(id);

         return CreateStream(id, false);
      }

      /// <summary>
      /// Opens the blob as a readable stream
      /// </summary>
      public override Stream OpenRead(string id)
      {
         GenericValidation.CheckBlobId(id);

         return OpenStream(id);
      }

      /// <summary>
      /// Checks if file exists
      /// </summary>
      public override bool Exists(string id)
      {
         GenericValidation.CheckBlobId(id);

         return File.Exists(GetFilePath(id));
      }

      /// <summary>
      /// Gets blob metadata
      /// </summary>
      public override BlobMeta GetMeta(string id)
      {
         GenericValidation.CheckBlobId(id);

         string path = GetFilePath(id);

         if (!File.Exists(path)) return null;

         var fi = new FileInfo(path);

         string md5;
         using (Stream fs = File.OpenRead(fi.FullName))
         {
            md5 = fs.GetHash(HashType.Md5);
         }

         return new BlobMeta(
            fi.Length,
            md5);
      }

      private string GetFilePath(string id)
      {
         GenericValidation.CheckBlobId(id);

         //id can contain path separators
         string[] parts = id.Split(StoragePath.PathSeparator).Select(EncodePathPart).ToArray();
         string name = parts[parts.Length - 1];
         DirectoryInfo dir;
         if(parts.Length == 1)
         {
            dir = _directory;
         }
         else
         {
            string extraPath = string.Join(FsPathSeparator, parts, 0, parts.Length - 1);

            string fullPath = Path.Combine(_directory.FullName, extraPath);

            dir = new DirectoryInfo(fullPath);
            if (!dir.Exists) dir.Create();
         }

         return Path.Combine(dir.FullName, name);
      }

      private Stream CreateStream(string id, bool overwrite = true)
      {
         GenericValidation.CheckBlobId(id);
         if (!_directory.Exists) _directory.Create();
         string path = GetFilePath(id);

         Stream s = overwrite ? File.Create(path) : File.OpenWrite(path);
         s.Seek(0, SeekOrigin.End);
         return s;
      }

      private Stream OpenStream(string id)
      {
         GenericValidation.CheckBlobId(id);
         string path = GetFilePath(id);
         if(!File.Exists(path)) return null;

         return File.OpenRead(path);
      }

      private static string EncodePathPart(string path)
      {
         return path.UrlEncode();
      }

      private static string DecodePathPart(string path)
      {
         return path.UrlDecode();
      }
   }
}
