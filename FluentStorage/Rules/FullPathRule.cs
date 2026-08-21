using System;
using System.Collections.Generic;
using FluentStorage.Enums;
using FluentStorage.Model;

namespace FluentStorage.Rules;

/// <summary>
/// Only accept objects that contain the given path, or exclude objects that contain a given path.
/// </summary>
public class FullPathRule : StorageRule {

	/// <summary>
	/// If true, only files of the given name are transferred. If false, files of the given name are excluded.
	/// </summary>
	public bool Whitelist { get; set; }

	/// <summary>
	/// The full paths names to check
	/// </summary>
	public IList<string> Paths { get; set; }

	/// <summary>
	/// Only accept objects that contain the given path, or exclude objects that contain a given path.
	/// </summary>
	/// <param name="whitelist">If true, only files of the given name are downloaded. If false, files of the given name are excluded.</param>
	/// <param name="names">The files names to match</param>
	public FullPathRule(bool whitelist, IList<string> names) {
		if (names == null) throw new ArgumentNullException(nameof(names));
		Whitelist = whitelist;
		Paths = names;
	}

	/// <summary>
	/// Checks if the object's path matches the given name
	/// </summary>
	public override bool IsAllowed(StoreObject item) {

		// if no valid names, accept all objects
		if (Paths.Count == 0) {
			return true;
		}

		if (item.Type == StorageObjectType.File) {
			var fileName = item.FullPath;
			if (Whitelist) {
				foreach (var p in Paths) {
					if (fileName.Contains(p)) return true;
				}
				return false;
			}
			else {
				foreach (var p in Paths) {
					if (fileName.Contains(p)) return false;
				}
				return true;
			}
		}
		else {
			return true;
		}
	}

}