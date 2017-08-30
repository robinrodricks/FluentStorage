using Storage.Net.Blob;
using Storage.Net.Microsoft.Azure.KeyVault.Blob;
using System;
using System.Net;

namespace Storage.Net
{
   public static class Factory
   {
      /// <summary>
      /// Azure Key Vault secrets.
      /// </summary>
      /// <param name="factory">The factory.</param>
      /// <param name="vaultUri">The vault URI.</param>
      /// <param name="azureAadClientId">The azure aad client identifier.</param>
      /// <param name="azureAadClientSecret">The azure aad client secret.</param>
      /// <returns></returns>
      public static IBlobStorageProvider AzureKeyVault(this IBlobStorageFactory factory,
         Uri vaultUri, string azureAadClientId, string azureAadClientSecret)
      {
         return new AzureKeyVaultBlobStorageProvider(vaultUri, azureAadClientId, azureAadClientSecret);
      }

      /// <summary>
      /// Azure Key Vault secrets.
      /// </summary>
      /// <param name="factory">The factory.</param>
      /// <param name="vaultUri">The vault URI.</param>
      /// <param name="cred">The cred.</param>
      /// <returns></returns>
      public static IBlobStorageProvider AzureKeyVault(this IBlobStorageFactory factory,
         Uri vaultUri, NetworkCredential cred)
      {
         return new AzureKeyVaultBlobStorageProvider(vaultUri, cred.UserName, cred.Password);
      }

   }
}
