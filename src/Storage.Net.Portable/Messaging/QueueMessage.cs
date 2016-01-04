using System;
using System.Collections.Generic;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Message to be used with <see cref="IMessageQueue"/>
   /// </summary>
   public class QueueMessage
   {
      /// <summary>
      /// Creates an instance of <see cref="QueueMessage"/>
      /// </summary>
      /// <param name="id">Message ID</param>
      /// <param name="content">Message content</param>
      public QueueMessage(string id, string content)
      {
         if(id == null) throw new ArgumentNullException("id");

         Id = id;
         Content = content;
      }

      /// <summary>
      /// Create queue message from message content
      /// </summary>
      /// <param name="content"></param>
      public QueueMessage(string content) : this(null, content)
      {
         
      }

      /// <summary>
      /// Message ID
      /// </summary>
      public string Id { get; private set; }

      /// <summary>
      /// Message content
      /// </summary>
      public string Content { get;  set; }

      /// <summary>
      /// Extra properties for this message
      /// </summary>
      public Dictionary<string, string> Properties { get; set; }
   }
}
