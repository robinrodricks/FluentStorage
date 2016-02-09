using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aloneguid.Support;
using Config.Net;
using NUnit.Framework;
using Storage.Net.Aws.Blob;
using Storage.Net.Blob;
using Storage.Net.Blob.Files;
using Storage.Net.Azure.Blob;
using Storage.Net.OneDrive.Blob;

namespace Storage.Net.Tests.Integration
{
   [TestFixture("azure")]
   [TestFixture("disk-directory")]
   [TestFixture("aws-s3")]
   //[TestFixture("onedrive-personal")]
   //[TestFixture("onedrive-business")]
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
            //case "onedrive-personal":
            //_storage = new OneDriveBlobStorage();
            //break;
            case "aws-s3":
               _storage = new AwsS3BlobStorage(
                  TestSettings.AwsAccessKeyId,
                  TestSettings.AwsSecretAccessKey,
                  TestSettings.AwsTestBucketName);
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
      public void List_All_DoesntCrash()
      {
         List<string> allBlobNames = _storage.List(null).ToList();
      }

      [Test]
      public void Upload_New_CanDownload()
      {
         string content = Generator.GetRandomString(10000, false);
         string contentRead;
         string id = Guid.NewGuid().ToString();

         using(var ms = content.ToMemoryStream())
         {
            _storage.UploadFromStream(id, ms);
         }

         using (var s = new MemoryStream())
         {
            _storage.DownloadToStream(id, s);
            contentRead = Encoding.UTF8.GetString(s.ToArray());
         }

         Assert.AreEqual(content, contentRead);
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

      [Test]
      public void Download_DoesNotExist_ConsistentException()
      {
         string id = Generator.RandomString;

         using(var ms = new MemoryStream())
         {
            try
            {
               _storage.DownloadToStream(id, ms);

               Assert.Fail("the call must fail");
            }
            catch(StorageException ex)
            {
               Assert.AreEqual(ErrorCode.NotFound, ex.ErrorCode);
            }
            catch(Exception ex)
            {
               Assert.Fail();
            }
         }
      }

   }
}
