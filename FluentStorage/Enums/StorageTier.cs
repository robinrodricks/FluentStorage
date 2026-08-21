namespace FluentStorage.Enums;

/// <summary>
/// Specifies the storage tier or storage class of an object.
/// </summary>
public enum StorageTier {

	/// <summary>
	/// Object not found.
	/// </summary>
	NotFound = -1,

	/// <summary>
	/// Unknown or provider-specific storage tier.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// [Azure] Hot tier.
	/// [AWS] STANDARD storage class.
	/// [GCP] Standard storage class.
	/// </summary>
	Standard,

	/// <summary>
	/// [Azure] Not directly supported. Closest equivalent is Autotiering via lifecycle rules.
	/// [AWS] Intelligent-Tiering storage class.
	/// [GCP] Autoclass enabled bucket.
	/// </summary>
	Intelligent,

	/// <summary>
	/// [Azure] Cool tier.
	/// [AWS] STANDARD_IA storage class.
	/// [GCP] Nearline storage class.
	/// </summary>
	Nearline,

	/// <summary>
	/// [Azure] Cool tier.
	/// [AWS] STANDARD_IA storage class.
	/// [GCP] Nearline storage class.
	/// </summary>
	Cool,

	/// <summary>
	/// [Azure] Cold tier.
	/// [AWS] Glacier Instant Retrieval storage class.
	/// [GCP] Coldline storage class.
	/// </summary>
	Cold,

	/// <summary>
	/// [Azure] Cold tier.
	/// [AWS] Glacier Instant Retrieval storage class.
	/// [GCP] Coldline storage class.
	/// </summary>
	Coldline,

	/// <summary>
	/// [Azure] Archive tier.
	/// [AWS] Glacier Flexible Retrieval storage class.
	/// [GCP] Archive storage class.
	/// </summary>
	Frozen,

	/// <summary>
	/// [Azure] Archive tier.
	/// [AWS] Glacier Flexible Retrieval storage class.
	/// [GCP] Archive storage class.
	/// </summary>
	Archive,

	/// <summary>
	/// [Azure] Archive tier.
	/// [AWS] Glacier Deep Archive storage class.
	/// [GCP] Archive storage class.
	/// </summary>
	DeepArchive
}