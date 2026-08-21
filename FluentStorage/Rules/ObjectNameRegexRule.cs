using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentStorage.Enums;
using FluentStorage.Model;

namespace FluentStorage.Rules;

/// <summary>
/// Only accept objects whose names match the given regular expression(s), or exclude objects that match.
/// Originally from FluentFTP `FtpFileNameRegexRule`.
/// </summary>
public class ObjectNameRegexRule : StorageRule {

	/// <summary>
	/// If true, only items where one of the supplied regex pattern matches are transferred.
	/// If false, items where one of the supplied regex pattern matches are excluded.
	/// </summary>
	public bool Whitelist { get; set; }

	/// <summary>
	/// The objects names to match
	/// </summary>
	public List<string> RegexPatterns { get; set; }

	/// <summary>
	/// Only accept items that match one of the supplied regex patterns.
	/// </summary>
	/// <param name="whitelist">If true, only items where one of the supplied regex pattern matches are transferred. If false, items where one of the supplied regex pattern matches are excluded.</param>
	/// <param name="regexPatterns">The list of regex patterns to match. Only valid patterns are accepted and stored. If none of the patterns are valid, this rule is disabled and passes all objects.</param>
	public ObjectNameRegexRule(bool whitelist, IList<string> regexPatterns) {
		if (regexPatterns == null) throw new ArgumentNullException(nameof(regexPatterns));
		Whitelist = whitelist;
		RegexPatterns = regexPatterns.Where(x => IsValidRegEx(x)).ToList();
	}

	/// <summary>
	/// Checks if the object name matches any RegexPattern
	/// </summary>
	public override bool IsAllowed(StoreObject item) {

		// if no valid regex patterns, accept all objects
		if (RegexPatterns.Count == 0) {
			return true;
		}

		// only check objects
		if (item.Type == StorageObjectType.File) {
			var fileName = item.Name;

			if (Whitelist) {
				return RegexPatterns.Any(x => Regex.IsMatch(fileName, x));
			}
			else {
				return !RegexPatterns.Any(x => Regex.IsMatch(fileName, x));
			}
		}
		else {
			return true;
		}
	}

	/// <summary>
	/// Checks if RexEx Pattern is valid
	/// </summary>
	public static bool IsValidRegEx(string pattern) {
		bool isValid = true;

		if ((pattern != null) && (pattern.Trim().Length > 0)) {
			try {
				Regex.Match("", pattern);
			}
			catch (ArgumentException) {
				// BAD PATTERN: Syntax error
				isValid = false;
			}
		}
		else {
			//BAD PATTERN: Pattern is null or blank
			isValid = false;
		}

		return (isValid);
	}

}