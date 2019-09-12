using Xunit;
using Storage.Net.Messaging;
using System;
using System.Threading.Tasks;
using System.Linq;
using NetBox.Generator;
using System.Diagnostics;
using System.Collections.Generic;

namespace Storage.Net.Tests.Integration.Messaging
{
   [Trait("Category", "Messenger")]
   public abstract class MessagingTest : IAsyncLifetime
   {
      private readonly MessagingFixture _fixture;
      private readonly IMessenger _msg;

      protected MessagingTest(MessagingFixture fixture)
      {
         _fixture = fixture;
         _msg = fixture.Messenger;
      }

      public Task InitializeAsync() => Task.CompletedTask;

      public Task DisposeAsync() => Task.CompletedTask;

      [Fact]
      public async Task SendMessage_OneMessage_DoesntCrash()
      {
         var qm = QueueMessage.FromText("test");

         await _msg.SendAsync("test", qm);
      }

      [Fact]
      public async Task SendMessage_NullChannel_ArgumentException()
      {
         await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.SendAsync(null, QueueMessage.FromText("test")));
      }

      [Fact]
      public async Task SendMessage_NullMessages_ArgumentException()
      {
         await Assert.ThrowsAsync<ArgumentNullException>(() => _msg.SendAsync("test", null));
      }

   }
}