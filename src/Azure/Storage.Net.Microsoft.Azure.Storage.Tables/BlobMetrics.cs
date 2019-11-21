using System;
using System.Collections.Generic;
using System.Text;

namespace Storage.Net.Microsoft.Azure.Storage.Tables
{
   /// <summary>
   /// Basic insight into blob metrics
   /// </summary>
   public class BlobMetrics
   {
      /// <summary>
      /// Creates a new instance of metrics row
      /// </summary>
      public BlobMetrics(DateTime date, bool isAnalytics, long capacityBytes, int containerCount, long objectCount)
      {
         Date = date;
         IsAnalytics = isAnalytics;
         CapacityBytes = capacityBytes;
         ContainerCount = containerCount;
         ObjectCount = objectCount;
      }

      /// <summary>
      /// Date of metric collection
      /// </summary>
      public DateTime Date { get; }

      /// <summary>
      /// When true, metrics are for analytics, otherwise for storage
      /// </summary>
      public bool IsAnalytics { get; }

      /// <summary>
      /// Capacity
      /// </summary>
      public long CapacityBytes { get; }

      /// <summary>
      /// Number of containers
      /// </summary>
      public int ContainerCount { get; }

      /// <summary>
      /// Number of objects
      /// </summary>
      public long ObjectCount { get; }
   }
}
