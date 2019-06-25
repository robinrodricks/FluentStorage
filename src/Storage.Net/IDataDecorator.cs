using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Storage.Net
{
   public interface IDataDecorator
   {
      Stream DecorateWriter(Stream parentStream);

      Stream DecorateReader(Stream parentStream);
   }
}
