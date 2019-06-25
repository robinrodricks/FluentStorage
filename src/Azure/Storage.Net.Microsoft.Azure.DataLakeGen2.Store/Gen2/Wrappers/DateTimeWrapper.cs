using System;
using Storage.Net.Microsoft.Azure.DataLakeGen2.Store.Gen2.Interfaces;

namespace Storage.Net.Microsoft.Azure.DataLakeGen2.Store.Gen2.Wrappers
{
   public class DateTimeWrapper : IDateTimeWrapper
   {
      public DateTime Now => DateTime.Now;
   }
}