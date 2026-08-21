using System.Collections.Generic;
using FluentStorage.Enums;
using FluentStorage.Model;

namespace FluentStorage.Rules.Engine;

internal static class RuleEngine {

	/// <summary>
	/// Returns `null` if the object has passed all the rules, and returns the `StorageRule` if it did not pass the given rule.
	/// </summary>
	public static StorageRule ObjectPassesRules(StoreObject result, IList<StorageRule> rules) {
		foreach (var rule in rules) {
			if (!rule.IsAllowed(result)) {
				return rule;
			}
		}
		return null;
	}


	/// <summary>
	/// Filter objects that pass the rules, and record the objects which were rejected by the rules
	/// </summary>
	public static List<StoreObject> ProcessDownloadRules(IList<StorageRule> rules, List<StorageProgress> results, List<StoreObject> objects) {
		var newObjects = new List<StoreObject>();

		var count = objects.Count;
		for (int o = 0; o < count; o++) {
			var obj = objects[o];
			var rejectRule = ObjectPassesRules(obj, rules);
			if (rejectRule == null) {

				// include object
				newObjects.Add(obj);
			}
			else {

				// exclude object
				// remember files skipped due to rules
				results.Add(new StorageProgress {
					LocalPath = null, // TODO: expensive to compute but can be added if required
					RemotePath = obj.FullPath,
					FileIndex = o,
					FileCount = count,
					Skipped = true,
					SkipReason = StorageReason.Rule,
					SkipRule = rejectRule,
				});
			}
		}
		objects = newObjects;
		return objects;
	}


	/// <summary>
	/// Filter objects that pass the rules, and record the objects which were rejected by the rules
	/// </summary>
	public static (List<string>, List<string>) ProcessUploadRules(IList<StorageRule> rules, List<StorageProgress> results, List<string> files, List<string> relativeFiles) {

		var newFiles = new List<string>();
		var newRelativeFiles = new List<string>();

		// ensure all objects pass the defined rules (if any)
		var count = files.Count;
		for (int f = 0; f < count; f++) {
			var rejectRule = ObjectPassesRules(new StoreObject(relativeFiles[f], StorageObjectType.File), rules);
			if (rejectRule == null) {

				// include object
				newFiles.Add(files[f]);
				newRelativeFiles.Add(relativeFiles[f]);
			}
			else {

				// exclude object
				// remember files skipped due to rules
				results.Add(new StorageProgress {
					LocalPath = relativeFiles[f],
					RemotePath = null, // TODO: expensive to compute but can be added if required
					FileIndex = f,
					FileCount = count,
					Skipped = true,
					SkipReason = StorageReason.Rule,
					SkipRule = rejectRule,
				});
			}
		}

		return (newFiles, newRelativeFiles);
	}



}