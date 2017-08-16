using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NetBox;
using Xunit;
using Storage.Net.Aws.Blob;
using Storage.Net.Blob;
using Storage.Net.Blob.Files;
using NetBox.Model;
using Storage.Net.Microsoft.Azure.Storage.Blob;

namespace Storage.Net.Tests.Integration
{
   #region [ Test Variations ]

   public class AzureBlobStorageTest : BlobStorageTest
   {
      public AzureBlobStorageTest() : base("azure") { }
   }

   public class AzureDataLakeBlobStorageTest : BlobStorageTest
   {
      public AzureDataLakeBlobStorageTest() : base("azure-datalakestore") { }
   }

   public class DiskDirectoryBlobStorageTest : BlobStorageTest
   {
      public DiskDirectoryBlobStorageTest() : base("disk-directory") { }
   }

   public class AwsS3BlobStorageTest : BlobStorageTest
   {
      public AwsS3BlobStorageTest() : base("aws-s3") { }
   }

   public class InMemboryBlobStorageTest : BlobStorageTest
   {
      public InMemboryBlobStorageTest() : base("inmemory") { }
   }

   public class AzureKeyVaultBlobStorageTest : BlobStorageTest
   {
      public AzureKeyVaultBlobStorageTest() : base("azurekeyvault") { }
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
            case "azure-datalakestore":
               _storage = StorageFactory.Blobs.AzureDataLakeStoreByClientSecret(
                  TestSettings.Instance.AzureDataLakeStoreAccountName,
                  TestSettings.Instance.AzureDataLakeCredential);
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
            case "inmemory":
               _storage = StorageFactory.Blobs.InMemory();
               break;
            case "azurekeyvault":
               _storage = StorageFactory.Blobs.AzureKeyVault(
                  TestSettings.Instance.KeyVaultUri,
                  TestSettings.Instance.KeyVaultCreds);
               break;
         }
      }

      private string GetRandomStreamId(string prefix = null)
      {
         string id = Guid.NewGuid().ToString();
         if (prefix != null) id = prefix + "/" + id;

         using (Stream s = "kjhlkhlkhlkhlkh".ToMemoryStream())
         {
            _storage.Write(id, s);
         }

         return id;
      }

      [Fact]
      public void List_All_DoesntCrash()
      {
         List<BlobId> allBlobNames = _storage.List(null, null, true).ToList();
      }

      [Fact]
      public void List_ByFlatPrefix_Filtered()
      {
         string prefix = Generator.RandomString;
         string id1 = prefix + Generator.RandomString;
         string id2 = prefix = Generator.RandomString;

         List<BlobId> items = _storage.List(null, prefix, false).ToList();
         Assert.Equal(2, items.Count);
      }

      [Fact]
      public void List_FilesInFolder_NonRecursive()
      {
         string id = Generator.RandomString;

         _storage.WriteText(id, Generator.RandomString);

         var items = _storage.List(null, null, true);
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
         string id = Guid.NewGuid().ToString();

         _storage.WriteText(id, content);

         string contentRead = _storage.ReadText(id);

         Assert.Equal(content, contentRead);
      }

      [Fact]
      public void Upload_NullId_ConsistentException()
      {
         using (var ms = new MemoryStream(Generator.GetRandomBytes(10, 11)))
         {
            Assert.Throws<ArgumentNullException>(() => _storage.Write(null, ms));
         }
      }

      [Fact]
      public void Upload_VeryLongId_ConsistenException()
      {
         using (var ms = new MemoryStream(Generator.GetRandomBytes(10, 11)))
         {
            string id = Generator.GetRandomString(100000, false);
            Assert.Throws<ArgumentException>(() => _storage.Write(id, ms));
         }
      }

      [Fact]
      public void Upload_NullStream_ConsistentException()
      {
         Assert.Throws<ArgumentNullException>(() => _storage.Write(Generator.GetRandomString(5, false), null));
      }

      [Fact]
      public void OpenStream_NullId_ConsistentException()
      {
         Assert.Throws<ArgumentNullException>(() => _storage.OpenRead(null));
      }

      [Fact]
      public void OpenStream_LargeId_ConsistentException()
      {
         Assert.Throws<ArgumentException>(() => _storage.OpenRead(Generator.GetRandomString(500, false)));
      }

      [Fact]
      public void OpenStream_New_CanDownload()
      {
         string id = GetRandomStreamId();

         using (Stream s = _storage.OpenRead(id))
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

         Assert.Throws<ArgumentNullException>(() => _storage.ReadToStream(null, ms));
      }

      [Fact]
      public void Download_ByVeryLargeId_ConsistentException()
      {
         var ms = new MemoryStream();

         Assert.Throws<ArgumentException>(() => _storage.ReadToStream(Generator.GetRandomString(100, false), ms));
      }

      [Fact]
      public void Download_ToNullStream_ConsistentException()
      {
         Assert.Throws<ArgumentNullException>(() => _storage.ReadToStream("1", null));
      }


      [Fact]
      public void Download_DoesNotExist_ConsistentException()
      {
         string id = Generator.RandomString;

         using(var ms = new MemoryStream())
         {
            try
            {
               _storage.ReadToStream(id, ms);

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
         Assert.Throws<StorageException>(() => _storage.ReadToStream(id, ms));
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
         _storage.WriteText("one/two/three.json", "{}");
         _storage.WriteText("two/three/four.json", "{p:1}");

         BlobId[] files = _storage.List(null, null, true).ToArray();

         throw new NotImplementedException();
         //Assert.True(files.Length >= 2);
         //Assert.Contains("one/two/three.json", files);
         //Assert.Contains("two/three/four.json", files);
      }

      //bad input does crash blob storages, they are still not equal in that sense
      /*[Theory]
      [InlineData("the??one")]
      [InlineData("file*x$")]
      public void Characters_BadInput_DoesntCrash(string input)
      {
         _storage.Delete(input);

         _storage.Write(input, "content".ToMemoryStream());

         string listed = _storage.List(input).FirstOrDefault();
         Assert.NotNull(listed);
      }*/

      [Fact]
      public void Extensions_LocalFsUploadDownload_Succeeds()
      {
         string id = Generator.RandomString;
         string localFile = Path.Combine(TestDir.FullName, "blob.txt");

         _storage.Write(id, "content".ToMemoryStream());

         _storage.ReadToFile(id, localFile);

         string lfc = File.ReadAllText(localFile);
         Assert.Equal("content", lfc);

         _storage.WriteFile(id + "1", localFile);
         _storage.ReadToFile(id + "1", localFile + "1");

         lfc = File.ReadAllText(localFile + "1");
         Assert.Equal("content", lfc);
      }

      [Fact]
      public void Extensions_StringDownloadUPload_JustWorks()
      {
         string id = Generator.RandomString;
         string text = Generator.RandomString;

         _storage.WriteText(id, text);

         string text2 = _storage.ReadText(id);

         Assert.Equal(text, text2);
      }

      [Fact]
      public void Extensions_CopyTo_JustWorks()
      {
         string sourceId = Generator.GetRandomString(10, false);
         string targetId = Generator.GetRandomString(10, false);
         string text = Generator.RandomString;

         _storage.WriteText(sourceId, text);
         Assert.False(_storage.Exists(targetId));

         _storage.CopyTo(sourceId, _storage, targetId);

         Assert.True(_storage.Exists(targetId));
         Assert.Equal(text, _storage.ReadText(targetId));
         
      }

      [Fact]
      public void GetMeta_RequiredProperties_ReturnsCorrect()
      {
         string id = Generator.GetRandomString(10, false);
         string text = Generator.RandomString;

         _storage.WriteText(id, text);

         BlobMeta meta = _storage.GetMeta(id);

         Assert.Equal(Encoding.UTF8.GetByteCount(text), meta.Size);
         if (meta.MD5 != null)
         {
            Assert.Equal(text.GetHash(HashType.Md5), meta.MD5);
         }
      }

      [Fact]
      public void Append_KeepAppending_Grows()
      {
         string id = Generator.GetRandomString(10, false);

         _storage.Delete(id);

         try
         {
            var ms = Generator.RandomString.ToMemoryStream();
            _storage.Append(id, ms);

            var meta = _storage.GetMeta(id);
            Assert.Equal(ms.Length, meta.Size);

            var ms1 = Generator.RandomString.ToMemoryStream();
            _storage.Append(id, ms1);
            meta = _storage.GetMeta(id);
            Assert.Equal(ms.Length + ms1.Length, meta.Size);
         }
         catch(NotSupportedException)
         {
            //AWS doesnt' support appends!
         }
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

         _storage.Write(id, td);

         TestDocument td2 = _storage.Read<TestDocument>(id);

         Assert.Equal("string", td2.M);
      }
   }
}
