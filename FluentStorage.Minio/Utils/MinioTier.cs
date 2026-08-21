using System.Collections.Generic;
using FluentStorage.Enums;

namespace FluentStorage.Minio.Utils;

public static class MinioTier {

	public static Dictionary<string, StorageTier> ToFluentTier = new() {
		["STANDARD"] = StorageTier.Standard,
		["INTELLIGENT_TIERING"] = StorageTier.Intelligent,
		["STANDARD_IA"] = StorageTier.Nearline,
		["GLACIER_IR"] = StorageTier.Cold,
		["GLACIER"] = StorageTier.Archive,
		["DEEP_ARCHIVE"] = StorageTier.DeepArchive
	};

	public static Dictionary<StorageTier, string> FromFluentTier = new() {
		[StorageTier.Standard] = "STANDARD",
		[StorageTier.Intelligent] = "INTELLIGENT_TIERING",
		[StorageTier.Nearline] = "STANDARD_IA",
		[StorageTier.Cold] = "GLACIER_IR",
		[StorageTier.Archive] = "GLACIER",
		[StorageTier.DeepArchive] = "DEEP_ARCHIVE"
	};

}