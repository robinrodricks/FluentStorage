using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetBox;
using Xunit;
using Storage.Net.Aws.Blob;
using Storage.Net.Blob;
using Storage.Net.Blob.Files;
using Storage.Net.Microsoft.Azure.Storage.Blob;
using System.Threading.Tasks;

namespace Storage.Net.Tests.Integration
{
   #region [ Test Variations ]

   public class AzureBlobStorageProviderTest : BlobStorageProviderTest
   {
      public AzureBlobStorageProviderTest() : base("azure") { }
   }

   public class AzureDataLakeBlobStorageProviderTest : BlobStorageProviderTest
   {
      public AzureDataLakeBlobStorageProviderTest() : base("azure-datalakestore") { }
   }

   public class DiskDirectoryBlobStorageProviderTest : BlobStorageProviderTest
   {
      public DiskDirectoryBlobStorageProviderTest() : base("disk-directory") { }
   }

   public class AwsS3BlobStorageProviderTest : BlobStorageProviderTest
   {
      public AwsS3BlobStorageProviderTest() : base("aws-s3") { }
   }

   public class InMemboryBlobStorageProviderTest : BlobStorageProviderTest
   {
      public InMemboryBlobStorageProviderTest() : base("inmemory") { }
   }

   public class AzureKeyVaultBlobStorageProviderTest : BlobStorageProviderTest
   {
      public AzureKeyVaultBlobStorageProviderTest() : base("azurekeyvault") { }
   }


   #endregion

   public abstract class BlobStorageProviderTest : AbstractTestFixture
   {
      private readonly string _type;
      private IBlobStorageProvider _provider;

      public BlobStorageProviderTest(string type)
      {
         _type = type;

         switch (_type)
         {
            case "azure":
               _provider = new AzureBlobStorageProvider(
                  TestSettings.Instance.AzureStorageName,
                  TestSettings.Instance.AzureStorageKey,
                  "blobstoragetest");
               break;
            case "azure-datalakestore":
               _provider = StorageFactory.Blobs.AzureDataLakeStoreByClientSecret(
                  TestSettings.Instance.AzureDataLakeStoreAccountName,
                  TestSettings.Instance.AzureDataLakeCredential);
               break;
            case "disk-directory":
               _provider = new DirectoryFilesBlobStorage(TestDir);
               break;
            //break;
            case "aws-s3":
               _provider = new AwsS3BlobStorageProvider(
                  TestSettings.Instance.AwsAccessKeyId,
                  TestSettings.Instance.AwsSecretAccessKey,
                  TestSettings.Instance.AwsTestBucketName);
               break;
            case "inmemory":
               _provider = StorageFactory.Blobs.InMemory();
               break;
            case "azurekeyvault":
               _provider = StorageFactory.Blobs.AzureKeyVault(
                  TestSettings.Instance.KeyVaultUri,
                  TestSettings.Instance.KeyVaultCreds);
               break;
         }
      }

      private async Task<string> GetRandomStreamId(string prefix = null)
      {
         string id = Guid.NewGuid().ToString();
         if (prefix != null) id = prefix + "/" + id;

         using (Stream s = "kjhlkhlkhlkhlkh".ToMemoryStream())
         {
            await _provider.WriteAsync(id, s);
         }

         return id;
      }

      [Fact]
      public async Task List_All_DoesntCrash()
      {
         List<BlobId> allBlobNames = (await _provider.ListAsync(null, null, true)).ToList();
      }

      [Fact]
      public async Task List_ByFlatPrefix_Filtered()
      {
         string prefix = Generator.RandomString;
         string id1 = prefix + Generator.RandomString;
         string id2 = prefix + Generator.RandomString;
         string id3 = Generator.RandomString;

         _provider.WriteText(id1, Generator.RandomString);
         _provider.WriteText(id2, Generator.RandomString);
         _provider.WriteText(id3, Generator.RandomString);

         List<BlobId> items = _provider.List(null, prefix, false).ToList();
         Assert.Equal(2, items.Count);
      }

      [Fact]
      public async Task List_FilesInFolder_NonRecursive()
      {
         string id = Generator.RandomString;

         _provider.WriteText(id, Generator.RandomString);

         var items = _provider.List(null, null, true);
      }

      [Fact]
      public async Task List_FilesInFolder_Recursive()
      {
         string id1 = Generator.RandomString;
         string id2 = StoragePath.Combine(Generator.RandomString, Generator.RandomString);
         string id3 = StoragePath.Combine(Generator.RandomString, Generator.RandomString, Generator.RandomString);

         _provider.WriteText(id1, Generator.RandomString);
         _provider.WriteText(id2, Generator.RandomString);
         _provider.WriteText(id3, Generator.RandomString);

         var items = await _provider.ListAsync(null, null, true);
      }


      [Fact]
      public async Task List_VeryLongPrefix_NoResultsNoCrash()
      {
         await Assert.ThrowsAsync<ArgumentException>(async () => await _provider.ListAsync(null, Generator.GetRandomString(100000, false), true));
      }

      class TestDocument
      {
         public string M { get; set; }
      }

      [Fact]
      public void Objects_Add_Retreives()
      {
         var td = new TestDocument() { M = "string" };

         string id = Generator.GetRandomString(10, false);

         _provider.Write(id, td);

         TestDocument td2 = _provider.Read<TestDocument>(id);

         Assert.Equal("string", td2.M);
      }
   }
}
