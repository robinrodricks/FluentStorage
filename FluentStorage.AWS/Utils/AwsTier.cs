using System.Collections.Generic;
using Amazon.S3;
using FluentStorage.Enums;

namespace FluentStorage.AWS.Utils;

public static class AwsTier {

	public static Dictionary<S3StorageClass, StorageTier> ToFluentTier = new Dictionary<S3StorageClass, StorageTier> {
		[S3StorageClass.Standard] = StorageTier.Standard,
		[S3StorageClass.IntelligentTiering] = StorageTier.Intelligent,
		[S3StorageClass.StandardInfrequentAccess] = StorageTier.Nearline,
		[S3StorageClass.GlacierInstantRetrieval] = StorageTier.Cold,
		[S3StorageClass.Glacier] = StorageTier.Archive,
		[S3StorageClass.DeepArchive] = StorageTier.DeepArchive
	};

	public static Dictionary<StorageTier, S3StorageClass> FromFluentTier = new Dictionary<StorageTier, S3StorageClass> {
		[StorageTier.Standard] = S3StorageClass.Standard,
		[StorageTier.Intelligent] = S3StorageClass.IntelligentTiering,
		[StorageTier.Nearline] = S3StorageClass.StandardInfrequentAccess,
		[StorageTier.Cold] = S3StorageClass.GlacierInstantRetrieval,
		[StorageTier.Archive] = S3StorageClass.Glacier,
		[StorageTier.DeepArchive] = S3StorageClass.DeepArchive
	};

}