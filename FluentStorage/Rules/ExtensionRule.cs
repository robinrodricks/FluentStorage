using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentStorage.Enums;
using FluentStorage.Model;

namespace FluentStorage.Rules;

/// <summary>
/// Only accept objects that have the given extension, or exclude objects of a given extension.
/// Originally from FluentFTP `FtpFileExtensionRule`.
/// </summary>
public class ExtensionRule : StorageRule {

	/// <summary>
	/// If true, only files of the given extension are transferred. If false, files of the given extension are excluded.
	/// </summary>
	public bool Whitelist { get; set; }

	/// <summary>
	/// The extensions to match
	/// </summary>
	public IList<string> Exts { get; set; }

	/// <summary>
	/// Only accept files that have the given extension, or exclude files of a given extension.
	/// </summary>
	/// <param name="whitelist">If true, only files of the given extension are transferred. If false, files of the given extension are excluded.</param>
	/// <param name="exts">The extensions to match</param>
	public ExtensionRule(bool whitelist, IList<string> exts) {
		if (exts == null) throw new ArgumentNullException(nameof(exts));
		Whitelist = whitelist;
		Exts = exts.Select(e => e.ToLower()).ToList();
	}

	/// <summary>
	/// Checks if the files has the given extension, or exclude files of the given extension.
	/// </summary>
	public override bool IsAllowed(StoreObject item) {

		// if no valid names, accept all objects
		if (Exts.Count == 0) {
			return true;
		}

		if (item.Type == StorageObjectType.File) {
			var ext = Path.GetExtension(item.Name).Replace(".", "").ToLower();
			if (Whitelist) {

				// whitelist
				if (string.IsNullOrEmpty(ext)) {
					return false;
				}
				else {
					return Exts.Contains(ext);
				}
			}
			else {

				// blacklist
				if (string.IsNullOrEmpty(ext)) {
					return true;
				}
				else {
					return !Exts.Contains(ext);
				}
			}
		}
		else {
			return true;
		}
	}

}