using System.Collections.Generic;
using Azure.Storage.Blobs.Models;
using FluentStorage.Enums;

namespace FluentStorage.Azure.Blobs.Storage;

public static class AzureTier {

	public static Dictionary<AccessTier, StorageTier> ToFluentTier = new Dictionary<AccessTier, StorageTier> {
		[AccessTier.Hot] = StorageTier.Standard,
		[AccessTier.Cool] = StorageTier.Nearline,
		[AccessTier.Cold] = StorageTier.Cold,
		[AccessTier.Archive] = StorageTier.Archive
	};

	public static Dictionary<StorageTier, AccessTier> AccessTierMap = new Dictionary<StorageTier, AccessTier> {
		[StorageTier.Standard] = AccessTier.Hot,
		[StorageTier.Nearline] = AccessTier.Cool,
		[StorageTier.Cold] = AccessTier.Cold,
		[StorageTier.Archive] = AccessTier.Archive
	};
}