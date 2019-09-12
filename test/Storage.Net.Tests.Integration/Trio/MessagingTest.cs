using Xunit;
using Storage.Net.Messaging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Storage.Net.Tests.Integration.Messaging
{
   [Trait("Category", "Messenger")]
   public abstract class MessagingTest : IAsyncLifetime
   {
      private readonly MessagingFixture _fixture;
      private readonly IMessenger _msg;
      private readonly string _qn = Guid.NewGuid().ToString();

      protected MessagingTest(MessagingFixture fixture)
      {
         _fixture = fixture;
         _msg = fixture.Messenger;
      }

      public Task InitializeAsync() => Task.CompletedTask;

      public async Task DisposeAsync()
      {
         //clear up all the channels

         try
         {
            IReadOnlyCollection<string> channels = await _msg.ListChannelsAsync();
            await _msg.DeleteChannelsAsync(channels);
         }
         catch { }
      }

      [Fact]
      public async Task SendMessage_OneMessage_DoesntCrash()
      {
         var qm = QueueMessage.FromText("test");

         await _msg.SendAsync(_qn, qm);
      }

      [Fact]
      public async Task SendMessage_NullChannel_ArgumentException()
      {
         await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.SendAsync(null, QueueMessage.FromText("test")));
      }

      [Fact]
      public async Task SendMessage_NullMessages_ArgumentException()
      {
         await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.SendAsync(_qn, null));
      }

      [Fact]
      public async Task Channels_Create_list_contains_created_channel()
      {
         string channelName = Guid.NewGuid().ToString();

         //send one message so channel gets created
         await _msg.SendAsync(channelName, QueueMessage.FromText("test"));

         IReadOnlyCollection<string> channels = await _msg.ListChannelsAsync();

         Assert.Contains(channelName, channels);
      }

      [Fact]
      public async Task Channels_delete_goesaway()
      {
         string channelName = Guid.NewGuid().ToString();

         //send one message so channel gets created
         await _msg.SendAsync(channelName, QueueMessage.FromText("test"));

         await _msg.DeleteChannelAsync(channelName);

         IReadOnlyCollection<string> channels = await _msg.ListChannelsAsync();

         Assert.DoesNotContain(channelName, channels);

      }

      [Fact]
      public async Task Channels_delete_null_list_argument_exception()
      {
         await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.DeleteChannelsAsync(null));
      }

   }
}