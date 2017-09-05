using NetBox;
using Storage.Net.Blob;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Net.Tests
{
   public class DocumentationScenarios
   {
      //[Fact]
      public void Run()
      {
         //Blobs_save_file_to_azure_storage_and_read_it_later();
         //Blobs_save_file_to_a_specific_folder();
         Blobs_list_files_in_a_folder();
      }

      public async Task Blobs_list_files_in_a_folder()
      {
         IBlobStorageProvider storage = StorageFactory.Blobs.AmazonS3BlobStorage(
            TestSettings.Instance.AwsAccessKeyId,
            TestSettings.Instance.AwsSecretAccessKey,
            TestSettings.Instance.AwsTestBucketName);

         await storage.WriteAsync("folder1/file1", Generator.RandomString.ToMemoryStream(), false);
         await storage.WriteAsync("folder1/file2", Generator.RandomString.ToMemoryStream(), false);
         await storage.WriteAsync("folder2/file1", Generator.RandomString.ToMemoryStream(), false);

         BlobId[] folderBlobs = (await storage.ListAsync(new ListOptions { FolderPath = "folder1", Recurse = true })).ToArray();
      }

      public async Task BlobStorage_sample1()
      {
         IBlobStorageProvider storage = StorageFactory.Blobs.AzureBlobStorage(
            TestSettings.Instance.AzureStorageName,
            TestSettings.Instance.AzureStorageKey,
            "container name");

         //upload it
         string content = "test content";
         using (var s = new MemoryStream(Encoding.UTF8.GetBytes(content)))
         {
            await storage.WriteAsync("someid", s);
         }

         //read back
         using (var s = new MemoryStream())
         {
            using (Stream ss = await storage.OpenReadAsync("someid"))
            {
               await ss.CopyToAsync(s);

               //content is now "test content"
               content = Encoding.UTF8.GetString(s.ToArray());
            }
         }
      }

      public async Task BlobStorage_sample2()
      {
         IBlobStorageProvider provider = StorageFactory.Blobs.AzureBlobStorage(
            TestSettings.Instance.AzureStorageName,
            TestSettings.Instance.AzureStorageKey,
            "container name");
         BlobStorage storage = new BlobStorage(provider);

         //upload it
         await storage.WriteTextAsync("someid", "test content");

         //read back
         string content = await storage.ReadTextAsync("someid");
      }

      public async Task Blobs_save_file_to_a_specific_folder()
      {
         //create the storage over a specific local directory
         IBlobStorageProvider provider = StorageFactory.Blobs.DirectoryFiles(new DirectoryInfo("c:\\tmp\\files"));
         var storage = new BlobStorage(provider);

         //write to the root folder
         await storage.WriteTextAsync("text.txt", "test content");

         string subfolderBlobId = StoragePath.Combine("level 0", "level 1", "in the folder.log");
         await storage.WriteTextAsync(subfolderBlobId, "test content");
      }

      public async Task List_all_files_in_a_folder()
      {
         IBlobStorageProvider provider = StorageFactory.Blobs.DirectoryFiles(new DirectoryInfo("c:\\tmp\\files"));
         var storage = new BlobStorage(provider);

         await storage.ListAsync(new ListOptions { Recurse = true });
         await storage.ListAsync(new ListOptions { FolderPath = "/folder1", Recurse = false });
      }
   }
}
