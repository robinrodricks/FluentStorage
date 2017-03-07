using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NetBox;
using Config.Net;
using Xunit;
using Storage.Net.Aws.Blob;
using Storage.Net.Blob;
using Storage.Net.Blob.Files;
using Storage.Net.Microsoft.Azure.Blob;

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
                  TestSettings.Instance.AzureStorageName,
                  TestSettings.Instance.AzureStorageKey,
                  "blobstoragetest");
               break;
            case "disk-directory":
               _storage = new DirectoryFilesBlobStorage(TestDir);
               break;
            //break;
            case "aws-s3":
               _storage = new AwsS3BlobStorage(
                  TestSettings.Instance.AwsAccessKeyId,
                  TestSettings.Instance.AwsSecretAccessKey,
                  TestSettings.Instance.AwsTestBucketName);
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
      public void List_VeryLongPrefix_NoResultsNoCrash()
      {
         Assert.Throws<ArgumentException>(() => _storage.List(Generator.GetRandomString(100000, false)));
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
      public void Upload_NullId_ConsistentException()
      {
         using (var ms = new MemoryStream(Generator.GetRandomBytes(10, 11)))
         {
            Assert.Throws<ArgumentNullException>(() => _storage.UploadFromStream(null, ms));
         }
      }

      [Fact]
      public void Upload_VeryLongId_ConsistenException()
      {
         using (var ms = new MemoryStream(Generator.GetRandomBytes(10, 11)))
         {
            string id = Generator.GetRandomString(100000, false);
            Assert.Throws<ArgumentException>(() => _storage.UploadFromStream(id, ms));
         }
      }

      [Fact]
      public void Upload_NullStream_ConsistentException()
      {
         Assert.Throws<ArgumentNullException>(() => _storage.UploadFromStream(Generator.GetRandomString(5, false), null));
      }

      [Fact]
      public void OpenStream_NullId_ConsistentException()
      {
         Assert.Throws<ArgumentNullException>(() => _storage.OpenStreamToRead(null));
      }

      [Fact]
      public void OpenStream_LargeId_ConsistentException()
      {
         Assert.Throws<ArgumentException>(() => _storage.OpenStreamToRead(Generator.GetRandomString(500, false)));
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
      public void Download_ByNullId_ConsistentException()
      {
         var ms = new MemoryStream();

         Assert.Throws<ArgumentNullException>(() => _storage.DownloadToStream(null, ms));
      }

      [Fact]
      public void Download_ByVeryLargeId_ConsistentException()
      {
         var ms = new MemoryStream();

         Assert.Throws<ArgumentException>(() => _storage.DownloadToStream(Generator.GetRandomString(100, false), ms));
      }

      [Fact]
      public void Download_ToNullStream_ConsistentException()
      {
         Assert.Throws<ArgumentNullException>(() => _storage.DownloadToStream("1", null));
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

      [Fact]
      public void Delete_IdTooLong_ConsistentException()
      {
         Assert.Throws<ArgumentException>(() => _storage.Delete(Generator.GetRandomString(10000, false)));
      }

      [Fact]
      public void Folders_SaveToSubfolders_FilesListed()
      {
         _storage.UploadFromStream("one/two/three.json", "{}".ToMemoryStream());
         _storage.UploadFromStream("two/three/four.json", "{p:1}".ToMemoryStream());

         string[] files = _storage.List(null).ToArray();

         Assert.True(files.Length >= 2);
         Assert.Contains("one/two/three.json", files);
         Assert.Contains("two/three/four.json", files);
      }

      //bad input does crash blob storages, they are still not equal in that sense
      /*[Theory]
      [InlineData("the??one")]
      [InlineData("file*x$")]
      public void Characters_BadInput_DoesntCrash(string input)
      {
         _storage.Delete(input);

         _storage.UploadFromStream(input, "content".ToMemoryStream());

         string listed = _storage.List(input).FirstOrDefault();
         Assert.NotNull(listed);
      }*/

      [Fact]
      public void Extensions_LocalFsUploadDownload_Succeeds()
      {
         string id = Generator.RandomString;
         string localFile = Path.Combine(TestDir.FullName, "blob.txt");

         _storage.UploadFromStream(id, "content".ToMemoryStream());

         _storage.DownloadToFile(id, localFile);

         string lfc = File.ReadAllText(localFile);
         Assert.Equal("content", lfc);

         _storage.UploadFromFile(id + "1", localFile);
         _storage.DownloadToFile(id + "1", localFile + "1");

         lfc = File.ReadAllText(localFile + "1");
         Assert.Equal("content", lfc);
      }

      [Fact]
      public void Extensions_StringDownloadUPload_JustWorks()
      {
         string id = Generator.RandomString;
         string text = Generator.RandomString;

         _storage.UploadText(id, text);

         string text2 = _storage.DownloadText(id);

         Assert.Equal(text, text2);
      }

      [Fact]
      public void Append_KeepAppending_Grows()
      {
         string id = Generator.GetRandomString(10, false);

         _storage.Delete(id);

         try
         {
            var ms = Generator.RandomString.ToMemoryStream();
            _storage.AppendFromStream(id, ms);

            var meta = _storage.GetMeta(id);
            Assert.Equal(ms.Length, meta.Size);

            var ms1 = Generator.RandomString.ToMemoryStream();
            _storage.AppendFromStream(id, ms1);
            meta = _storage.GetMeta(id);
            Assert.Equal(ms.Length + ms1.Length, meta.Size);
         }
         catch(NotSupportedException)
         {
            //AWS doesnt' support appends!
         }
      }
   }
}
