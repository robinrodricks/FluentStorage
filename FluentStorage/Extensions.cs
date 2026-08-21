using System;
using System.IO.Compression;
using FluentStorage.Queue;
using FluentStorage.Queue.Large;
using FluentStorage.Sinks;
using FluentStorage.Storage;

namespace FluentStorage;

public static class Extensions {


	/// <summary>
	/// Wraps message publisher so that if it's content is larger than <paramref name="minSizeLarge"/>, the content is uploaded
	/// to blob storage and cleared on the message itself. The message is then stamped with a property <see cref="QueueMessage.LargeMessageContentHeaderName"/>
	/// which contains blob path of the message content.
	/// </summary>
	/// <param name="messenger">Message publisher to wrap</param>
	/// <param name="offloadStorage">Blob storage used to offload the message content</param>
	/// <param name="minSizeLarge">Threshold size</param>
	/// <param name="blobPathGenerator">Optional generator for blob path used to save large message content.</param>
	/// <returns></returns>
	public static IQueue HandleLargeContent(this IQueue messenger, IStore offloadStorage, int minSizeLarge,
		Func<QueueMessage, string> blobPathGenerator = null) {
		return new LargeMessageMessenger(messenger, offloadStorage, minSizeLarge, blobPathGenerator, false);
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="blobStorage"></param>
	/// <param name="sinks"></param>
	/// <returns></returns>
	public static IStore WithSinks(this IStore blobStorage,
		params ITransformSink[] sinks) {
		return new SinkedStore(blobStorage, sinks);
	}

	/// <summary>
	/// Wraps blob storage into zip compression
	/// </summary>
	/// <param name="blobStorage"></param>
	/// <param name="compressionLevel"></param>
	/// <returns></returns>
	public static IStore WithGzipCompression(
		this IStore blobStorage, CompressionLevel compressionLevel = CompressionLevel.Optimal) {
		return blobStorage.WithSinks(new GZipSink(compressionLevel));
	}

#if !NET16

	/// <summary>
	/// 
	/// </summary>
	/// <param name="blobStorage"></param>
	/// <param name="encryptionKey"></param>
	/// <returns></returns>
	[Obsolete("Please use WithAesSymmetricEncryption as Rijndael is obsolete in .Net 6 and above")]
	public static IStore WithSymmetricEncryption(
		this IStore blobStorage,
		string encryptionKey) {
		return blobStorage.WithSinks(new SymmetricEncryptionSink(encryptionKey));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="blobStorage"></param>
	/// <param name="encryptionKey"></param>
	/// <param name="encryptionSecret"></param>
	/// <returns></returns>
	[Obsolete("Please use WithAesSymmetricEncryption as Rijndael is obsolete in .Net 6 and above")]
	public static IStore WithSymmetricEncryption(
		this IStore blobStorage,
		string encryptionKey,
		string encryptionSecret) {
		return blobStorage.WithSinks(new SymmetricEncryptionSink(encryptionKey, encryptionSecret));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="blobStorage"></param>
	/// <param name="encryptionKey"></param>
	/// <returns></returns>
	public static IStore WithAesSymmetricEncryption(
		this IStore blobStorage,
		string encryptionKey) {
		return blobStorage.WithSinks(new AesSymmetricEncryptionSink(encryptionKey));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="blobStorage"></param>
	/// <param name="encryptionKey"></param>
	/// <param name="encryptionSecret"></param>
	/// <returns></returns>
	public static IStore WithAesSymmetricEncryption(
		this IStore blobStorage,
		string encryptionKey,
		string encryptionSecret) {
		return blobStorage.WithSinks(new AesSymmetricEncryptionSink(encryptionKey, encryptionSecret));
	}

#endif


}