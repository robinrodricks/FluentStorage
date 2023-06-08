namespace NetBox.Performance
{
   using System;
   using System.Diagnostics;

   /// <summary>
   /// Measures a time slice as precisely as possible
   /// </summary>
   public class TimeMeasure : IDisposable
   {
      private readonly Stopwatch _sw = new Stopwatch();

      /// <summary>
      /// Creates the measure object
      /// </summary>
      public TimeMeasure()
      {
         _sw.Start();
      }

      /// <summary>
      /// Returns number of elapsed ticks since the start of measure.
      /// The measuring process will continue running.
      /// </summary>
      public long ElapsedTicks => _sw.ElapsedTicks;

      /// <summary>
      /// Returns number of elapsed milliseconds since the start of measure.
      /// The measuring process will continue running.
      /// </summary>
      public long ElapsedMilliseconds => _sw.ElapsedMilliseconds;


      /// <summary>
      /// Gets time elapsed from the time this measure was created
      /// </summary>
      public TimeSpan Elapsed => _sw.Elapsed;

      /// <summary>
      /// Stops measure object if still running
      /// </summary>
      public void Dispose()
      {
         if (_sw.IsRunning)
         {
            _sw.Stop();
         }
      }
   }
}
