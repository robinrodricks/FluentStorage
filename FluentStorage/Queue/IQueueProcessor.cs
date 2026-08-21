using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStorage.Queue;

/// <summary>
/// Message processing interface used to register a callback that receives a message
/// </summary>
public interface IQueueProcessor {
	/// <summary>
	/// Process the given messages in this queue
	/// </summary>
	Task ProcessMessages(List<QueueMessage> messages);
}