using System;

namespace Storage.Net.Messaging
{
   public class QueueMessage
   {
      public QueueMessage(string id, string content)
      {
         if(id == null) throw new ArgumentNullException("id");

         Id = id;
         Content = content;
      }

      public string Id { get; private set; }

      public string Content { get;  set; }
   }
}
