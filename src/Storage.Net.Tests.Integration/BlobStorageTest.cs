using System;
using System.IO;
using Config.Net;
using NUnit.Framework;
using Storage.Net.Blob;
using Storage.Net.Blob.Files;
using Storage.Net.Azure.Blob;

namespace Storage.Net.Tests.Integration
{
   [TestFixture("azure")]
   [TestFixture("disk-directory")]
   public class BlobStorageTest : AbstractTestFixture
   {
      private readonly string _type;
      private IBlobStorage _storage;

      public BlobStorageTest(string type)
      {
         _type = type;
      }

      [SetUp]
      public void SetUp()
      {
         switch (_type)
         {
            case "azure":
               _storage = new AzureBlobStorage(
                  Cfg.Read(TestSettings.AzureStorageName),
                  Cfg.Read(TestSettings.AzureStorageKey),
                  "blobstoragetest");
               break;
            case "disk-directory":
               _storage = new DirectoryFilesBlobStorage(TestDir);
               break;
         }
      }

      private string GetRandomStreamId()
      {
         string id = Guid.NewGuid().ToString();

         using (Stream s = "kjhlkhlkhlkhlkh".ToMemoryStream())
         {
            _storage.UploadFromStream(id, s);
         }

         return id;
      }

      [Test]
      public void Upload_New_CanDownload()
      {
         string id = GetRandomStreamId();

         using (Stream s = new MemoryStream())
         {
            _storage.DownloadToStream(id, s);
         }
      }

      [Test]
      public void OpenStream_New_CanDownload()
      {
         string id = GetRandomStreamId();

         using (Stream s = _storage.OpenStreamToRead(id))
         {
            var ms = new MemoryStream();
            s.CopyTo(ms);

            Assert.Greater(ms.Length, 0);
         }
      }

   }
}
