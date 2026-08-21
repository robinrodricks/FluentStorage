using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentStorage.Enums;

namespace FluentStorage.Model;

/// <summary>
/// Manages a single object inside a bucket or file system.
/// </summary>
public sealed class StoreObject : IEquatable<StoreObject>, ICloneable {

	/// <summary>
	/// Gets the raw input string given by the storage provider, that was parsed to generate the values in this object.
	/// </summary>
	public string Input { get; private set; }

	/// <summary>
	/// Gets the type of the storage object (file/folder)
	/// </summary>
	public StorageObjectType Type { get; private set; }

	/// <summary>
	/// Returns true if the object is a folder
	/// </summary>
	public bool IsFolder => Type == StorageObjectType.Folder;

	/// <summary>
	/// Returns true if the object is a file
	/// </summary>
	public bool IsFile => Type == StorageObjectType.File;

	/// <summary>
	/// Gets the folder path containing this item
	/// </summary>
	public string FolderPath { get; private set; }

	/// <summary>
	/// Gets the name of this object, unique within the folder. In most providers this is the same as file name.
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Object size, in bytes.
	/// </summary>
	public long? Size { get; set; }

	/// <summary>
	/// MD5 content hash of the object.
	/// This can be null if storage provider does not provide the hash, or if it would require downloading the entire object to compute it.
	/// </summary>
	public string MD5 { get; set; }

	/// <summary>
	/// The date and time when the object was created
	/// </summary>
	public DateTimeOffset? DateCreated { get; set; }

	/// <summary>
	/// The date and time when the object was last modified
	/// </summary>
	public DateTimeOffset? DateModified { get; set; }

	/// <summary>
	/// Gets full path to this object on the storage provider (folder path + object name).
	/// Uses the unified path system. If you want the raw path returned by the provider, use `Input`.
	/// </summary>
	public string FullPath { get; private set; }

	/// <summary>
	/// Custom provider-specific properties. Key names are case-insensitive.
	/// </summary>
	public Dictionary<string, object> Properties { get; private set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// User defined metadata. Key names are case-insensitive.
	/// </summary>
	public Dictionary<string, string> Metadata { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	internal object Tag { get; set; }


	/// <summary>
	/// Create a new StoreObject from a full object path
	/// </summary>
	public StoreObject(string fullPath, StorageObjectType objType = StorageObjectType.File) {
		SetFullPath(fullPath);

		Type = objType;
	}

	/// <summary>
	/// Creates a new StoreObject from a split folder path and item name
	/// </summary>
	public StoreObject(string folderPath, string name, StorageObjectType objType) {
		Name = name ?? throw new ArgumentNullException(nameof(name));
		Name = StoragePath.NormalizePart(Name);
		FolderPath = StoragePath.Normalize(folderPath);
		FullPath = StoragePath.Combine(FolderPath, Name);
		Type = objType;
		Input = name; // technically incorrect but best we can do
	}

	/// <summary>
	/// Changes full path of this object without modifying any other property
	/// </summary>
	public void SetFullPath(string fullPath) {

		// save raw
		Input = fullPath;

		string path = StoragePath.Normalize(fullPath);

		// save normalized full path
		FullPath = path;

		if (StoragePath.IsRootPath(path)) {
			Name = "";
			FolderPath = "";
		}
		else {
			string[] parts = StoragePath.Split(path);

			Name = parts.Last();
			FolderPath = StoragePath.GetParent(path);
		}
	}

	/// <summary>
	/// Returns true if this item is a folder and it's a root folder
	/// </summary>
	public bool IsRootFolder => Type == StorageObjectType.Folder && StoragePath.IsRootPath(FullPath);

	/// <summary>
	/// Full object info, i.e type, id and path
	/// </summary>
	public override string ToString() {
		string k = Type == StorageObjectType.File ? "file" : "folder";

		return $"{k}: {Name}@{FolderPath}";
	}

	/// <summary>
	/// Equality check
	/// </summary>
	/// <param name="other"></param>
	public bool Equals(StoreObject other) {
		if (ReferenceEquals(other, null))
			return false;

		return
			other.FullPath == FullPath &&
			other.Type == Type;
	}

	/// <summary>
	/// Equality check
	/// </summary>
	/// <param name="other"></param>
	public override bool Equals(object other) {
		if (ReferenceEquals(other, null))
			return false;
		if (ReferenceEquals(other, this))
			return true;
		if (other.GetType() != typeof(StoreObject))
			return false;

		return Equals((StoreObject)other);
	}

	/// <summary>
	/// Hash code calculation
	/// </summary>
	public override int GetHashCode() {
		return FullPath.GetHashCode() * Type.GetHashCode();
	}

	/// <summary>
	/// Constructs a file blob by full ID
	/// </summary>
	public static implicit operator StoreObject(string fullPath) {
		return new StoreObject(fullPath, StorageObjectType.File);
	}

	/// <summary>
	/// Converts blob to string by using full path
	/// </summary>
	/// <param name="blob"></param>
	public static implicit operator string(StoreObject blob) {
		return blob.FullPath;
	}

	/// <summary>
	/// Converts blob attributes (user metadata to byte array)
	/// </summary>
	/// <returns></returns>
	public byte[] AttributesToByteArray() {
		using (var ms = new MemoryStream()) {
			using (var b = new BinaryWriter(ms, Encoding.UTF8, true)) {
				b.Write((byte)1); //version marker

				b.Write((int)Metadata?.Count);   //number of metadata items

				foreach (KeyValuePair<string, string> pair in Metadata) {
					b.Write(pair.Key);
					b.Write(pair.Value);
				}
			}

			return ms.ToArray();
		}
	}

	/// <summary>
	/// Appends attributes from byte array representation
	/// </summary>
	/// <param name="data"></param>
	public void AppendAttributesFromByteArray(byte[] data) {
		if (data == null)
			return;

		using (var ms = new MemoryStream(data)) {
			using (var b = new BinaryReader(ms, Encoding.UTF8, true)) {
				byte version = b.ReadByte();  //to be used with versioning
				if (version != 1) {
					throw new ArgumentException($"version {version} is not supported", nameof(data));
				}

				int count = b.ReadInt32();
				if (count > 0) {
					for (int i = 0; i < count; i++) {
						string key = b.ReadString();
						string value = b.ReadString();

						Metadata[key] = value;
					}
				}
			}
		}
	}

	/// <summary>
	/// Prepends path to this object's path without modifying object's properties
	/// </summary>
	/// <param name="path"></param>
	public void PrependPath(string path) {
		if (path == null || StoragePath.IsRootPath(path))
			return;

		FolderPath = StoragePath.Combine(path, FolderPath);
	}

	/// <summary>
	/// Clones blob to best efforts
	/// </summary>
	/// <returns></returns>
	public object Clone() {
		var clone = (StoreObject)MemberwiseClone();
		clone.Metadata = new Dictionary<string, string>(Metadata, StringComparer.OrdinalIgnoreCase);
		clone.Properties = new Dictionary<string, object>(Properties, StringComparer.OrdinalIgnoreCase);
		return clone;
	}


	/// <summary>
	/// Try to get property and cast it to a specified type
	/// </summary>
	public bool TryGetProperty<TValue>(string name, out TValue value, TValue defaultValue = default) {
		if (name == null || !Properties.TryGetValue(name, out object objValue)) {
			value = defaultValue;
			return false;
		}

		if (objValue is TValue) {
			value = (TValue)objValue;
			return true;
		}

		value = defaultValue;
		return false;
	}

	/// <summary>
	/// Tries to add properties in pairs when value is not null
	/// </summary>
	/// <param name="keyValues"></param>
	public void TryAddProperties(params object[] keyValues) {
		for (int i = 0; i < keyValues.Length; i += 2) {
			string key = (string)keyValues[i];
			object value = keyValues[i + 1];

			if (key != null && value != null) {
				if (value is string s && string.IsNullOrEmpty(s))
					continue;

				Properties[key] = value;
			}
		}
	}

	/// <summary>
	/// Works just like <see cref="TryAddProperties(object[])"/> but prefixes all the keys
	/// </summary>
	/// <param name="prefix"></param>
	/// <param name="keyValues"></param>
	public void TryAddPropertiesWithPrefix(string prefix, params object[] keyValues) {
		if (string.IsNullOrEmpty(prefix))
			TryAddProperties(keyValues);

		object[] keyValuesWithPrefix = keyValues.Select((e, i) => i % 2 == 0 ? prefix + (string)e : e).ToArray();

		TryAddProperties(keyValuesWithPrefix);
	}

	/// <summary>
	/// Tries to add properties from dictionary by key names
	/// </summary>
	/// <param name="source"></param>
	/// <param name="keyNames"></param>
	public void TryAddPropertiesFromDictionary(IDictionary<string, string> source, params string[] keyNames) {
		if (source == null || keyNames == null)
			return;

		foreach (string key in keyNames) {
			if (source.TryGetValue(key, out string value)) {
				Properties[key] = value;
			}
		}
	}

}