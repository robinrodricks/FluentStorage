using System;
using System.Collections.Generic;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Message to be used in all the queueing code
   /// </summary>
   public class QueueMessage
   {
      private Dictionary<string, string> _properties;

      /// <summary>
      /// Creates an instance of <see cref="QueueMessage"/>
      /// </summary>
      /// <param name="id">Message ID</param>
      /// <param name="content">Message content</param>
      public QueueMessage(string id, string content)
      {
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
      public Dictionary<string, string> Properties
      {
         get
         {
            if(_properties == null) _properties = new Dictionary<string, string>();
            return _properties;
         }
      }
   }
}
