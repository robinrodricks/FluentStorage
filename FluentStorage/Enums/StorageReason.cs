namespace FluentStorage.Enums;

public enum StorageReason {
	/// <summary>
	/// File did not pass a filtering rule
	/// </summary>
	Rule,
	/// <summary>
	/// File already exists
	/// </summary>
	Exists,
	/// <summary>
	/// Length and timestamp matched
	/// </summary>
	Timestamp,
	/// <summary>
	/// Length and checksum matched
	/// </summary>
	Checksum,
}