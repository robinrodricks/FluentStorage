using System;
using System.Collections.Generic;
using FluentStorage.Enums;
using FluentStorage.Model;

namespace FluentStorage.Rules;

/// <summary>
/// Only accept folders that have the given name, or exclude folders of a given name.
/// Originally from FluentFTP `FtpFolderNameRule`.
/// </summary>
public class DirectoryNameRule : StorageRule {

	/// <summary>
	/// Common folders to blacklist
	/// </summary>
	public static List<string> CommonBlacklistedFolders = new List<string> {
		".git",
		".svn",
		".DS_Store",
		"node_modules",
	};

	/// <summary>
	/// If true, only folders of the given name are transferred.
	/// If false, folders of the given name are excluded.
	/// </summary>
	public bool Whitelist { get; set; }

	/// <summary>
	/// The folder names to match
	/// </summary>
	public IList<string> Names { get; set; }

	/// <summary>
	/// Which path segment to start checking from
	/// </summary>
	public int StartSegment { get; set; }

	/// <summary>
	/// Only accept folders that have the given name, or exclude folders of a given name.
	/// </summary>
	/// <param name="whitelist">If true, only folders of the given name are downloaded. If false, folders of the given name are excluded.</param>
	/// <param name="names">The folder names to match</param>
	/// <param name="startSegment">Which path segment to start checking from. 0 checks root folder onwards. 1 skips root folder.</param>
	public DirectoryNameRule(bool whitelist, IList<string> names, int startSegment = 0) {
		if (names == null) throw new ArgumentNullException(nameof(names));
		Whitelist = whitelist;
		Names = names;
		StartSegment = startSegment;
	}

	/// <summary>
	/// Checks if the folders has the given name, or exclude folders of the given name.
	/// </summary>
	public override bool IsAllowed(StoreObject item) {

		// if no valid names, accept all objects
		if (Names.Count == 0) {
			return true;
		}

		// get the folder name of this item
		string[] dirNameParts = null;
		if (item.Type == StorageObjectType.File) {
			dirNameParts = StoragePath.SplitFast(item.FolderPath);
		}
		else if (item.Type == StorageObjectType.Folder) {
			dirNameParts = StoragePath.SplitFast(item.FullPath);
		}
		else {
			return true;
		}

		// check against whitelist or blacklist
		if (Whitelist) {

			// loop through path segments starting at given index
			for (int d = StartSegment; d < dirNameParts.Length; d++) {
				var dirName = dirNameParts[d];

				// whitelist
				if (Names.Contains(dirName.Trim())) {
					return true;
				}
			}
			return false;
		}
		else {

			// loop through path segments starting at given index
			for (int d = StartSegment; d < dirNameParts.Length; d++) {
				var dirName = dirNameParts[d];

				// blacklist
				if (Names.Contains(dirName.Trim())) {
					return false;
				}
			}
			return true;
		}
	}

}