using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Storage.Net.Microsoft.Azure.Storage.Blob
{
   class AzureBlockBlobRandomAccessStream
   {
      private readonly CloudBlockBlob _blob;

      public AzureBlockBlobRandomAccessStream(CloudBlockBlob blob)
      {
         _blob = blob;
      }
   }
}
