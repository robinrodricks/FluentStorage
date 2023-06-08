namespace NetBox.IO
{
   using System;
   using System.IO;

   /// <summary>
   /// Represents a stream that ignores <see cref="IDisposable"/> operations i.e. cannot be closed by the client
   /// </summary>
   class NonCloseableStream : DelegatedStream
   {
      /// <summary>
      /// Creates an instance of this class
      /// </summary>
      /// <param name="master">Master stream to delegate operations to</param>
      public NonCloseableStream(Stream master) : base(master)
      {

      }

      /// <summary>
      /// Overrides this call to do nothing
      /// </summary>
      /// <param name="disposing"></param>
      protected override void Dispose(bool disposing)
      {
         //does nothing on purpose
      }
   }
}
