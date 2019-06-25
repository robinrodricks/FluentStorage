using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Storage.Net
{
   public interface IDataDecorator
   {
      Stream Transform(Stream parentStream);

      Stream Untransform(Stream parentStream);
   }
}
