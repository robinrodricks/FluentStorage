using Moq;
using NetBox.Generator;
using Storage.Net.Blob;
using Storage.Net.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Storage.Net.Tests.Messaging
{
    public class LargeMessagePublishingTest
    {
        private readonly Mock<IBlobStorage> _blobStorage = new Mock<IBlobStorage>();
        private readonly IMessagePublisher _publisher;

        public LargeMessagePublishingTest()
        {
            _publisher = StorageFactory.Messages
                .InMemoryPublisher(nameof(LargeMessagePublishingTest))
                .HandleLargeContent(_blobStorage.Object, 100);
        }

        [Fact]
        public async Task SendMessage_Small_AllContent()
        {
            var smallMessage = new QueueMessage(RandomGenerator.GetRandomBytes(50, 50));

            //send small message
            await _publisher.PutMessageAsync(smallMessage);

            //validate that small message was never uploaded
            _blobStorage.Verify(s => s.WriteAsync(It.IsAny<string>(), It.IsAny<Stream>(), false, default(CancellationToken)), Times.Never);

            //validate that message does not have
            Assert.False(smallMessage.Properties.ContainsKey(QueueMessage.LargeMessageContentHeaderName));
        }

        [Fact]
        public async Task SendMessage_Large_NoContentAndUploadedAndHasId()
        {
            var largeMessage = new QueueMessage(RandomGenerator.GetRandomBytes(150, 150));

            //send large message
            await _publisher.PutMessageAsync(largeMessage);

            //validate that small message was uploaded once
            _blobStorage.Verify(s => s.WriteAsync(It.IsAny<string>(), It.IsAny<Stream>(), false, default(CancellationToken)), Times.Once);

            //validate that message has offload header
            Assert.True(largeMessage.Properties.ContainsKey(QueueMessage.LargeMessageContentHeaderName));
        }
    }
}
