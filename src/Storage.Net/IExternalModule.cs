using System;
using System.Collections.Generic;
using System.Text;
using Storage.Net.ConnectionString;

namespace Storage.Net
{
   public interface IExternalModule
   {
      IConnectionFactory ConnectionFactory { get; }
   }
}
