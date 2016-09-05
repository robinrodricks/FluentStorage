using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aloneguid.Support;
using Config.Net;
using Xunit;
using Storage.Net.Aws.Blob;
using Storage.Net.Blob;
using Storage.Net.Blob.Files;
using Storage.Net.Azure.Blob;

namespace Storage.Net.Tests.Integration
{
   #region [ Test Variations ]

   public class AzureBlobStorageTest : BlobStorageTest
   {
      public AzureBlobStorageTest() : base("azure") { }
   }

   public class DiskDirectoryBlobStorageTest : BlobStorageTest
   {
      public DiskDirectoryBlobStorageTest() : base("disk-directory") { }
   }

   public class AwsS3BlobStorageTest : BlobStorageTest
   {
      public AwsS3BlobStorageTest() : base("aws-s3") { }
   }

   #endregion

   public abstract class BlobStorageTest : AbstractTestFixture
   {
      private readonly string _type;
      private IBlobStorage _storage;

      public BlobStorageTest(string type)
      {
         _type = type;

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
            //break;
            case "aws-s3":
               _storage = new AwsS3BlobStorage(
                  TestSettings.AwsAccessKeyId,
                  TestSettings.AwsSecretAccessKey,
                  TestSettings.AwsTestBucketName);
               break;
         }
      }

      private string GetRandomStreamId(string prefix = null)
      {
         string id = Guid.NewGuid().ToString();
         if (prefix != null) id = prefix + id;

         using (Stream s = "kjhlkhlkhlkhlkh".ToMemoryStream())
         {
            _storage.UploadFromStream(id, s);
         }

         return id;
      }

      [Fact]
      public void List_All_DoesntCrash()
      {
         List<string> allBlobNames = _storage.List(null).ToList();
      }

      [Fact]
      public void List_ByPrefix_Filtered()
      {
         int countBefore1 = _storage.List("pref1").ToList().Count;
         int countBefore2 = _storage.List("pref2").ToList().Count;

         string blob1 = GetRandomStreamId("pref1");
         string blob2 = GetRandomStreamId("pref1");
         string blob3 = GetRandomStreamId("pref2");

         List<string> pref1 = _storage.List("pref1").ToList();
         List<string> pref2 = _storage.List("pref2").ToList();

         Assert.Equal(2 + countBefore1, pref1.Count);
         Assert.Equal(1 + countBefore2, pref2.Count);
      }

      [Fact]
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

         Assert.Equal(content, contentRead);
      }

      [Fact]
      public void OpenStream_New_CanDownload()
      {
         string id = GetRandomStreamId();

         using (Stream s = _storage.OpenStreamToRead(id))
         {
            var ms = new MemoryStream();
            s.CopyTo(ms);

            Assert.True(ms.Length > 0);
         }
      }

      [Fact]
      public void Download_DoesNotExist_ConsistentException()
      {
         string id = Generator.RandomString;

         using(var ms = new MemoryStream())
         {
            try
            {
               _storage.DownloadToStream(id, ms);

               Assert.True(false, "the call must fail");
            }
            catch(StorageException ex)
            {
               Assert.Equal(ErrorCode.NotFound, ex.ErrorCode);
            }
            catch(Exception ex)
            {
               Assert.True(false, "this exception is not expected: " + ex);
            }
         }
      }

      [Fact]
      public void Exists_ByNull_ConsistentException()
      {
         Assert.Throws<ArgumentNullException>(() => _storage.Exists(null));
      }

      [Fact]
      public void Exists_NoObject_ReturnsFalse()
      {
         Assert.False(_storage.Exists(Generator.RandomString));
      }

      [Fact]
      public void Exists_HasObject_ReturnsTrue()
      {
         string id = GetRandomStreamId();

         Assert.True(_storage.Exists(id));
      }

      [Fact]
      public void Delete_CreateNewAndDelete_CannotFindAgain()
      {
         string id = GetRandomStreamId();

         _storage.Delete(id);

         var ms = new MemoryStream();
         Assert.Throws<StorageException>(() => _storage.DownloadToStream(id, ms));
      }

      [Fact]
      public void Delete_NullInKey_ConsistentException()
      {
         Assert.Throws<ArgumentNullException>(() => _storage.Delete(null));
      }
   }
}
