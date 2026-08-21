using System;
using FluentStorage.Enums;
using FluentStorage.Rules;

namespace FluentStorage.Model;

/// <summary>
/// Reports the progress of a file or folder transfer.
/// When transferring an entire folder, one instance is reported for each file.
/// </summary>
public sealed class StorageProgress {

	/// <summary>
	/// A value between 0-100 indicating percentage complete,
	/// or -1 if the transfer failed.
	/// </summary>
	public double Progress { get; set; }

	/// <summary>
	/// Number of bytes transferred for the current file.
	/// </summary>
	public long TransferredBytes { get; set; }

	/// <summary>
	/// Current transfer speed in bytes per second.
	/// </summary>
	public double TransferSpeed { get; set; }

	/// <summary>
	/// Estimated time remaining for the current file.
	/// </summary>
	public TimeSpan ETA { get; set; }

	/// <summary>
	/// Absolute remote object path.
	/// </summary>
	public string RemotePath { get; set; } = string.Empty;

	/// <summary>
	/// Absolute local file path.
	/// </summary>
	public string LocalPath { get; set; } = string.Empty;

	/// <summary>
	/// Zero-based index of the current file being transferred.
	/// </summary>
	public int FileIndex { get; set; }

	/// <summary>
	/// Total number of files in the folder transfer.
	/// </summary>
	public int FileCount { get; set; }

	/// <summary>
	/// Exception that occurred while transferring the current file.
	/// Null if the transfer completed successfully.
	/// </summary>
	public Exception? Error { get; set; }

	/// <summary>
	/// True if this file was skipped because it already existed.
	/// </summary>
	public bool Skipped { get; set; }

	/// <summary>
	/// Reason the file was skipped.
	/// </summary>
	public StorageReason SkipReason { get; set; }

	/// <summary>
	/// Rule due to which the file was skipped.
	/// </summary>
	public StorageRule SkipRule { get; set; }

	/// <summary>
	/// Returns the overall folder progress as a percentage,
	/// or -1 if the total file count is unknown.
	/// </summary>
	public double OverallProgress =>
		FileCount > 0
			? ((FileIndex - 1) + Math.Max(0, Progress) / 100d) / FileCount * 100d
			: -1;
}