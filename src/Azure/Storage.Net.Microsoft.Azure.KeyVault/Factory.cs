using Storage.Net.Blobs;
using Storage.Net.Microsoft.Azure.KeyVault;
using Storage.Net.Microsoft.Azure.KeyVault.Blobs;
using System;
using System.Net;

namespace Storage.Net
{
   public static class Factory
   {
      public static IModulesFactory UseAzureKeyVault(this IModulesFactory factory)
      {
         return factory.Use(new ExternalModule());
      }

      /// <summary>
      /// Azure Key Vault secrets.
      /// </summary>
      /// <param name="factory">The factory.</param>
      /// <param name="vaultUri">The vault URI.</param>
      /// <param name="azureAadClientId">The azure aad client identifier.</param>
      /// <param name="azureAadClientSecret">The azure aad client secret.</param>
      /// <returns></returns>
      public static IBlobStorage AzureKeyVault(this IBlobStorageFactory factory,
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
      public static IBlobStorage AzureKeyVault(this IBlobStorageFactory factory,
         Uri vaultUri, NetworkCredential cred)
      {
         return new AzureKeyVaultBlobStorageProvider(vaultUri, cred.UserName, cred.Password);
      }

   }
}
