using global::System;
using global::System.Collections.Generic;
using global::System.Text;
using Xunit;

namespace NetBox.System
{
   public class GuidExtensionsTest
   {
      [Fact]
      public void ShortGuid()
      {
         Guid guid = Guid.NewGuid();
         string shortGuid = guid.ToShortest();

         Assert.Equal(guid, shortGuid.FromShortestGuid());
      }
   }
}
