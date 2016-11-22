using System;
using System.Collections.Generic;
using System.Text;

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
         StringContent = content;
      }

      /// <summary>
      /// Creates an instance of <see cref="QueueMessage"/>
      /// </summary>
      /// <param name="id">Message ID</param>
      /// <param name="content">Message content</param>
      public QueueMessage(string id, byte[] content)
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
      /// Create queue message from message content
      /// </summary>
      /// <param name="content"></param>
      public QueueMessage(byte[] content) : this(null, content)
      {

      }


      /// <summary>
      /// Message ID
      /// </summary>
      public string Id { get; private set; }

      /// <summary>
      /// Gets the count of how many time this message was dequeued
      /// </summary>
      public int DequeueCount { get; set; }

      /// <summary>
      /// Message content as string
      /// </summary>
      public string StringContent
      {
         get { return Content == null ? null : Encoding.UTF8.GetString(Content, 0, Content.Length); }
         set
         {
            if (string.IsNullOrEmpty(value)) Content = null;
            else Content = Encoding.UTF8.GetBytes(value);
         }
      }

      /// <summary>
      /// Message content as byte array
      /// </summary>
      public byte[] Content { get; set; }

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

      /// <summary>
      /// Clones the message
      /// </summary>
      /// <returns></returns>
      public QueueMessage Clone()
      {
         var message = new QueueMessage(Id, Content);
         if (_properties != null) message._properties = new Dictionary<string, string>(_properties);
         return message;
      }
   }
}
