using System;
using System.Net;
using Storage.Net.Blob;
using Storage.Net.Ftp;

namespace Storage.Net
{
   public static class Factory
   {
      public static IBlobStorage Ftp(this IBlobStorageFactory factory,
         string hostNameOrAddress, NetworkCredential credentials)
      {
         return new FluentFtpBlobStorage(hostNameOrAddress, credentials);
      }
   }
}
