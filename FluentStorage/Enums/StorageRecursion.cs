namespace FluentStorage.Enums;

/// <summary>
/// Controls recursion mode
/// </summary>
public enum StorageRecursion {
	/// <summary>
	/// Recurse locally - for each folder on the remote datastore, iterate and query in a separate task
	/// </summary>
	Local = 1,

	/// <summary>
	/// Recurse remotely - let the remote datastore return the entire folder tree
	/// </summary>
	Remote = 2
}