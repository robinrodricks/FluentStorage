using Aloneguid.Support;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Storage.Net.Messaging
{
   /// <summary>
   /// Creates queue in memory. All messages are lost when program exists.
   /// The class operates on <see cref="ConcurrentQueue{T}"/> implementation.
   /// Message IDs are generated based on random instance bound prefix and long integer taken from the current time
   /// tick count
   /// </summary>
   public class InMemoryMessagePublisherReceiver : IMessagePublisher, IMessageReceiver
   {
      private readonly ConcurrentQueue<QueueMessage> _queue = new ConcurrentQueue<QueueMessage>();
      private readonly ConcurrentQueue<QueueMessage> _deadLetterQueue = new ConcurrentQueue<QueueMessage>();
      private readonly string _instancePrefix = Generator.RandomString;
      private long _messageId = DateTime.UtcNow.Ticks;
      private Action<QueueMessage> _pumpDelegate;
      private bool _disposed;
      private bool _pumpRunning;

      /// <summary>
      /// Puts the message in the inmemory queue
      /// </summary>
      public void PutMessage(QueueMessage message)
      {
         if(message == null) return;
         _queue.Enqueue(message);
      }

      /// <summary>
      /// Puts the messages in the inmemory queue
      /// </summary>
      public void PutMessages(IEnumerable<QueueMessage> messages)
      {
         if(messages == null) return;
         foreach(QueueMessage message in messages)
         {
            var stampedMessage = RecreateWithId(message);
            _queue.Enqueue(stampedMessage);
         }
      }

      private QueueMessage RecreateWithId(QueueMessage message)
      {
         string id = $"{_instancePrefix}-{++_messageId}";

         var result = new QueueMessage(id, message.Content);
         if(message.Properties.Count > 0)
         {
            foreach(var pair in message.Properties)
            {
               result.Properties.Add(pair.Key, pair.Value);
            }
         }
         return result;
      }

      /// <summary>
      /// Does nothing
      /// </summary>
      public void Dispose()
      {
         _disposed = true;
      }

      /// <summary>
      /// Receives message from inmemory queue
      /// </summary>
      /// <returns></returns>
      public QueueMessage ReceiveMessage()
      {
         QueueMessage result;
         _queue.TryDequeue(out result);
         return result;
      }

      /// <summary>
      /// Doesn't do anything as <see cref="ReceiveMessage"/> always deletes the message from the queue
      /// </summary>
      public void ConfirmMessage(QueueMessage message)
      {
      }

      /// <summary>
      /// Not implemetned
      /// </summary>
      public void StartMessagePump(Action<QueueMessage> onMessage)
      {
         _pumpDelegate = onMessage;

         if(!_pumpRunning)
         {
#if PORTABLE
            throw new Exception("pump not supported in portable version");
#else
            var thread = new Thread(MessagePumpThreadEntry) { IsBackground = true };
            thread.Start();
#endif
            _pumpRunning = true;
         }

      }

#if !PORTABLE
      private void MessagePumpThreadEntry()
      {
         while(!_disposed)
         {
            QueueMessage msg;

            while(_queue.TryDequeue(out msg))
            {
               _pumpDelegate?.Invoke(msg);
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));
         }
      }
#endif

      /// <summary>
      /// Receives up to <paramref name="count"/> messages when available.
      /// </summary>
      /// <returns>Messages or null if there is nothing to fetch.</returns>
      public IEnumerable<QueueMessage> ReceiveMessages(int count)
      {
         var result = new List<QueueMessage>();
         while(result.Count < count && _queue.Count > 0)
         {
            QueueMessage message;
            if(_queue.TryDequeue(out message))
            {
               result.Add(message);
            }
         }
         return result;
      }

      /// <summary>
      /// Not implemented
      /// </summary>
      public void DeadLetter(QueueMessage message, string reason, string errorDescription)
      {
         throw new NotImplementedException();
      }
   }
}
