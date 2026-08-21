using System;
using FluentStorage.Enums;
using FluentStorage.Exceptions;
using LibGit2Sharp;

namespace FluentStorage.Git.Utils;

/// <summary>
/// Maps LibGit2Sharp exceptions into the common FluentStorage exception model.
/// </summary>
internal static class GitExceptionMapper {

	/// <summary>
	/// Converts a LibGit2Sharp exception into a <see cref="StorageException"/> with an appropriate error code.
	/// Other exceptions are returned unchanged.
	/// </summary>
	public static Exception Map(Exception ex) {
		if (ex is StorageException) {
			return ex;
		}

		switch (ex) {
			case RepositoryNotFoundException rnf:
				return new StorageException(StorageErrorCode.NotFound, rnf);
			case NotFoundException nf:
				return new StorageException(StorageErrorCode.NotFound, nf);
			case NameConflictException nc:
				return new StorageException(StorageErrorCode.DuplicateKey, nc);
			case CheckoutConflictException cc:
				return new StorageException(StorageErrorCode.Conflict, cc);
			case NonFastForwardException nff:
				return new StorageException(StorageErrorCode.Conflict, nff);
			case LibGit2SharpException lge:
				return new StorageException(lge.Message, lge);
			default:
				return ex;
		}
	}
}