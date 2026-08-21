namespace FluentStorage.Enums;

/// <summary>
/// All these values intentionally match Azure's implementation.
/// </summary>
public enum StorageUrlPermissions {

	/// <summary>
	/// No permissions.
	/// </summary>
	None = 0,

	/// <summary>
	/// [S3][Azure]
	/// Read the object.
	/// </summary>
	Read = 1,

	/// <summary>
	/// [Azure]
	/// Append data to the object.
	/// </summary>
	Add = 2,

	/// <summary>
	/// [S3][Azure]
	/// Create a new object.
	/// </summary>
	Create = 4,

	/// <summary>
	/// [S3][Azure]
	/// Write or overwrite the object.
	/// </summary>
	Write = 8,

	/// <summary>
	/// [S3][Azure]
	/// Delete the object.
	/// </summary>
	Delete = 16,

	/// <summary>
	/// [Azure]
	/// Delete a specific object version.
	/// </summary>
	DeleteVersion = 32,

	/// <summary>
	/// [Azure]
	/// Permanently delete a soft-deleted object.
	/// </summary>
	PermanentDelete = 64,

	/// <summary>
	/// [S3][Azure]
	/// List objects.
	/// </summary>
	List = 128,

	/// <summary>
	/// [Azure]
	/// Read or write object tags.
	/// </summary>
	Tag = 256,

	/// <summary>
	/// [Azure]
	/// Move or rename the object.
	/// </summary>
	Move = 512,

	/// <summary>
	/// [Azure]
	/// Execute the object (Data Lake).
	/// </summary>
	Execute = 1024,

	/// <summary>
	/// [Azure]
	/// Set the object's immutability policy.
	/// </summary>
	SetImmutabilityPolicy = 2048,

	/// <summary>
	/// [Azure]
	/// Filter objects by tags.
	/// </summary>
	Filter = 4096
}