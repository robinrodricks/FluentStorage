using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Storage.Net.Microsoft.Azure
{
   /// <summary>
   /// General Azure methods
   /// </summary>
   public class AzureStorage
   {
      /// <summary>
      /// Creates an instance
      /// </summary>
      protected AzureStorage(string accountName, string key)
      {
         Account = new CloudStorageAccount(
            new StorageCredentials(accountName, key),
            true);

      }

      /// <summary>
      /// Gets account
      /// </summary>
      protected CloudStorageAccount Account { get; private set; }

      /// <summary>
      /// Checks if blob name is valid
      /// </summary>
      /// <param name="id"></param>
      /// <returns></returns>
      public static bool IsValidBlobName(string id)
      {
         /*
          * A blob name must conforming to the following naming rules:
          * - A blob name can contain any combination of characters.
          * - A blob name must be at least one character long and cannot be more than 1,024 characters long.
          * - Blob names are case-sensitive.
          * - Reserved URL characters must be properly escaped.
          * - The number of path segments comprising the blob name cannot exceed 254. A path segment is the string between consecutive delimiter characters (e.g., the forward slash '/') that corresponds to the name of a virtual directory.
          */

         if (string.IsNullOrEmpty(id)) return false;
         if (id.Length == 0) return false;
         if (id.Length > 1024) return false;
         if (HttpUtility.UrlEncode(id) != id) return false;

         return true;
      }
   }
}
