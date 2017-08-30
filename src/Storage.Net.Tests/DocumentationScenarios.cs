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

         BlobId[] folderBlobs = (await storage.ListAsync("folder1", null, true)).ToArray();
      }

      public async Task Blobs_save_file_to_a_specific_folder()
      {
         //create the storage over a specific local directory
         IBlobStorageProvider storage = StorageFactory.Blobs.DirectoryFiles(new DirectoryInfo("c:\\tmp\\files"));


         string content = "test content";
         using (var s = new MemoryStream(Encoding.UTF8.GetBytes(content)))
         {
            await storage.WriteAsync("text.txt", s);
         }

         using (var s = new MemoryStream(Encoding.UTF8.GetBytes(content)))
         {
            string subfolderBlobId = StoragePath.Combine("level 0", "level 1", "in the folder.log");

            await storage.WriteAsync(subfolderBlobId, s);
         }
      }
   }
}
