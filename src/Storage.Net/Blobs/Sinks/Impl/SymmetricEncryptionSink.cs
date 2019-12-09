#if !NET16
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NetBox.Extensions;

namespace Storage.Net.Blobs.Sinks.Impl
{
   class SymmetricEncryptionSink : ITransformSink
   {
      private readonly SymmetricAlgorithm _cryptoAlgorithm;

      public SymmetricEncryptionSink(string base64Key)
      {
         _cryptoAlgorithm = new RijndaelManaged();
         _cryptoAlgorithm.Key = Convert.FromBase64String(base64Key);
         _cryptoAlgorithm.GenerateIV();
      }

      public Stream OpenReadStream(string fullPath, Stream parentStream)
      {
         return new CryptoStream(parentStream, _cryptoAlgorithm.CreateDecryptor(), CryptoStreamMode.Read);
      }

      public Stream OpenWriteStream(ref string fullPath, Stream parentStream)
      {
         return new CryptoStream(parentStream, _cryptoAlgorithm.CreateEncryptor(), CryptoStreamMode.Write);
      }
   }
}
#endif