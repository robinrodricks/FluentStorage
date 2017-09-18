using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Net.Microsoft.Azure.Storage.Blob
{
   public class AzureBlobEvent
   {
      public DateTime TimeStamp { get; private set; }

      public string OperationName { get; private set; }

      public string ResultCode { get; private set; }

      internal AzureBlobEvent(string[] parts)
      {
         TimeStamp = DateTime.Parse(parts[1]);

         OperationName = parts[2];
         ResultCode = parts[4];
      }

      public override string ToString()
      {
         return $"{TimeStamp} - {OperationName} => {ResultCode}";
      }
   }
}
