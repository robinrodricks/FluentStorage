using NetBox;
using Storage.Net;
using Storage.Net.Blob;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

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

      public void Blobs_list_files_in_a_folder()
      {
         IBlobStorage storage = StorageFactory.Blobs.AmazonS3BlobStorage(
            TestSettings.Instance.AwsAccessKeyId,
            TestSettings.Instance.AwsSecretAccessKey,
            TestSettings.Instance.AwsTestBucketName);

         storage.Write("folder1/file1", Generator.RandomString.ToMemoryStream());
         storage.Write("folder1/file2", Generator.RandomString.ToMemoryStream());
         storage.Write("folder2/file1", Generator.RandomString.ToMemoryStream());

         string[] folderBlobs =storage.List("folder1").ToArray();
      }

      public void Blobs_save_file_to_a_specific_folder()
      {
         //create the storage over a specific local directory
         IBlobStorage storage = StorageFactory.Blobs.DirectoryFiles(new DirectoryInfo("c:\\tmp\\files"));


         string content = "test content";
         using (var s = new MemoryStream(Encoding.UTF8.GetBytes(content)))
         {
            storage.Write("text.txt", s);
         }

         using (var s = new MemoryStream(Encoding.UTF8.GetBytes(content)))
         {
            string subfolderBlobId = StoragePath.Combine("level 0", "level 1", "in the folder.log");

            storage.Write(subfolderBlobId, s);
         }
      }

      public void Blobs_save_file_to_azure_storage_and_read_it_later()
      {
         //create the storage
         IBlobStorage storage = StorageFactory.Blobs.AzureBlobStorage(
            TestSettings.Instance.AzureStorageName,
            TestSettings.Instance.AzureStorageKey,
            "mycontainer");

         //upload it
         string content = "test content";
         using (var s = new MemoryStream(Encoding.UTF8.GetBytes(content)))
         {
            storage.Write("someid", s);
         }

         //read back
         using (var s = new MemoryStream())
         {
            storage.ReadToStream("someid", s);

            //content is now "test content"
            content = Encoding.UTF8.GetString(s.ToArray());
         }
         

      }
   }
}
