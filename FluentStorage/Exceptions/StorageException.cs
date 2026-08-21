using System;
using System.Collections.Generic;
using FluentStorage.Enums;

namespace FluentStorage.Exceptions;

/// <summary>
/// Generic storage exception
/// </summary>
public class StorageException : Exception {
	private static readonly Dictionary<StorageErrorCode, string> ErrorCodeToMessage = new Dictionary<StorageErrorCode, string>();

	/// <summary>
	/// Creates a new instance of <see cref="StorageException"/>
	/// </summary>
	public StorageException() {
	}

	/// <summary>
	/// Creates a new instance of <see cref="StorageException"/> with exception message
	/// </summary>
	public StorageException(string message) : base(message) {
	}

	static StorageException() {
		foreach (StorageErrorCode code in Enum.GetValues(typeof(StorageErrorCode))) {
			string message = $"request failed with code '{code}'";
			ErrorCodeToMessage[code] = message;
		}
	}

	/// <summary>
	/// Creates a new instance of <see cref="StorageException"/> by error code
	/// </summary>
	public StorageException(StorageErrorCode code, Exception innerException) : base(ErrorCodeToMessage[code], innerException) {
		ErrorCode = code;
	}

	/// <summary>
	/// Creates a new instance of <see cref="StorageException"/> with exception message and inner exception
	/// </summary>
	public StorageException(string message, Exception inner) : base(message, inner) {
	}

	/// <summary>
	/// Indicates the error code for this exception
	/// </summary>
	public StorageErrorCode ErrorCode { get; private set; }
}