using System.Collections.Generic;
using FluentStorage.Enums;

namespace FluentStorage.GCP.Utils;

public static class GoogleTier {

	public static Dictionary<string, StorageTier> ToFluentTier = new() {
		["STANDARD"] = StorageTier.Standard,
		["NEARLINE"] = StorageTier.Nearline,
		["COLDLINE"] = StorageTier.Cold,
		["ARCHIVE"] = StorageTier.Archive
	};

	public static Dictionary<StorageTier, string> FromFluentTier = new() {
		[StorageTier.Standard] = "STANDARD",
		[StorageTier.Nearline] = "NEARLINE",
		[StorageTier.Cold] = "COLDLINE",
		[StorageTier.Archive] = "ARCHIVE"
	};

}