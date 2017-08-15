using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Storage.Net.Blob
{
   /// <summary>
   /// Generic interface for blob storage implementations
   /// </summary>
   public interface IBlobStorage : IDisposable
   {
      /// <summary>
      /// Returns the list of available blobs
      /// </summary>
      /// <param name="folderPath">Path to the folder. When null works with a root folder.</param>
      /// <param name="prefix">Blob prefix to filter by. When null returns all blobs.
      /// Cannot be longer than 50 characters.</param>
      /// <param name="recurse">When true returns files recursively</param>
      /// <returns>List of blob IDs</returns>
      IEnumerable<BlobItem> List(string folderPath, string prefix, bool recurse);

      /// <summary>
      /// Returns the list of available blobs
      /// </summary>
      /// <param name="folderPath">Path to the folder. When null works with a root folder.</param>
      /// <param name="prefix">Blob prefix to filter by. When null returns all blobs.
      /// Cannot be longer than 50 characters.</param>
      /// <param name="recurse">When true returns files recursively</param>
      /// <returns>List of blob IDs</returns>
      Task<IEnumerable<BlobItem>> ListAsync(string folderPath, string prefix, bool recurse);

      /// <summary>
      /// Deletes a blob by id
      /// </summary>
      /// <param name="id">Blob ID, required.</param>
      /// <exception cref="System.ArgumentNullException">Thrown when ID is null.</exception>
      /// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      void Delete(string id);

      /// <summary>
      /// Deletes a blob by id
      /// </summary>
      /// <param name="id">Blob ID, required.</param>
      /// <exception cref="System.ArgumentNullException">Thrown when ID is null.</exception>
      /// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      Task DeleteAsync(string id);

      /// <summary>
      /// Checks if a blob exists
      /// </summary>
      /// <param name="id">Blob ID, required</param>
      /// <returns>True if blob exists, false otherwise</returns>
      bool Exists(string id);

      /// <summary>
      /// Checks if a blob exists
      /// </summary>
      /// <param name="id">Blob ID, required</param>
      /// <returns>True if blob exists, false otherwise</returns>
      Task<bool> ExistsAsync(string id);

      /// <summary>
      /// Gets basic blob metadata
      /// </summary>
      /// <param name="id">Blob id</param>
      /// <returns>Blob metadata or null if blob doesn't exist</returns>
      BlobMeta GetMeta(string id);

      /// <summary>
      /// Gets basic blob metadata
      /// </summary>
      /// <param name="id">Blob id</param>
      /// <returns>Blob metadata or null if blob doesn't exist</returns>
      Task<BlobMeta> GetMetaAsync(string id);

      /// <summary>
      /// Creates a new blob and returns a writeable stream to it. If the blob already exists it will be
      /// overwritten.
      /// </summary>
      /// <param name="id">Blob ID</param>
      /// <param name="sourceStream">Source stream, must be readable and support Length</param>
      /// <returns>Writeable stream</returns>
      /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
      /// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      void Write(string id, Stream sourceStream);

      /// <summary>
      /// Creates a new blob and returns a writeable stream to it. If the blob already exists it will be
      /// overwritten.
      /// </summary>
      /// <param name="id">Blob ID</param>
      /// <param name="sourceStream">Source stream, must be readable and support Length</param>
      /// <returns>Writeable stream</returns>
      /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
      /// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      Task WriteAsync(string id, Stream sourceStream);

      /// <summary>
      /// Opens or creates a new blob for append operations. If the blob doesn't exist it will be created first.
      /// overwritten.
      /// </summary>
      /// <param name="id">Blob ID</param>
      /// <returns>Writeable stream</returns>
      /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
      /// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      void Append(string id, Stream sourceStream);

      /// <summary>
      /// Opens or creates a new blob for append operations. If the blob doesn't exist it will be created first.
      /// overwritten.
      /// </summary>
      /// <param name="id">Blob ID</param>
      /// <returns>Writeable stream</returns>
      /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
      /// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      Task AppendAsync(string id, Stream sourceStream);

      /// <summary>
      /// Opens the blob stream to read.
      /// </summary>
      /// <param name="id">Blob ID, required</param>
      /// <returns>Stream in an open state, or null if blob doesn't exist by this ID. It is your responsibility to close and dispose this
      /// stream after use.</returns>
      /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
      /// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      Stream OpenRead(string id);

      /// <summary>
      /// Opens the blob stream to read.
      /// </summary>
      /// <param name="id">Blob ID, required</param>
      /// <returns>Stream in an open state, or null if blob doesn't exist by this ID. It is your responsibility to close and dispose this
      /// stream after use.</returns>
      /// <exception cref="System.ArgumentNullException">Thrown when any parameter is null</exception>
      /// <exception cref="System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
      Task<Stream> OpenReadAsync(string id);
   }
}
