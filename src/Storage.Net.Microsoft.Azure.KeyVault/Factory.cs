using Storage.Net.Blob;
using Storage.Net.Microsoft.Azure.KeyVault.Blob;
using System;
using System.Net;

namespace Storage.Net
{
   public static class Factory
   {
      public static IBlobStorage AzureKeyVault(this IBlobStorageFactory factory,
         Uri vaultUri, string azureAadClientId, string azureAadClientSecret)
      {
         return new AzureKeyVaultBlobStorage(vaultUri, azureAadClientId, azureAadClientSecret);
      }

      public static IBlobStorage AzureKeyVault(this IBlobStorageFactory factory,
         Uri vaultUri, NetworkCredential cred)
      {
         return new AzureKeyVaultBlobStorage(vaultUri, cred.UserName, cred.Password);
      }

   }
}
