using System;
using System.Collections.Generic;

namespace FluentStorage.Blobs;

public interface IBlob : ICloneable
{
    /// <summary>
    /// Gets the kind of item
    /// </summary>
    BlobItemKind Kind { get; }

    /// <summary>
    /// Simply checks if kind of this item is <see cref="BlobItemKind.Folder"/>
    /// </summary>
    bool IsFolder { get; }

    /// <summary>
    /// Simply checks if kind of this item is <see cref="BlobItemKind.File"/>
    /// </summary>
    bool IsFile { get; }

    /// <summary>
    /// Gets the folder path containing this item
    /// </summary>
    string FolderPath { get; }

    /// <summary>
    /// Gets the name of this blob, unique within the folder. In most providers this is the same as file name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Blob size
    /// </summary>
    long? Size { get; set; }

    /// <summary>
    /// MD5 content hash of the blob. Note that this property can be null if underlying storage has
    /// no information about the hash, or it's very expensive to calculate it, for instance it would require
    /// getting a whole content of the blob to hash it.
    /// </summary>
    string MD5 { get; set; }

    /// <summary>
    /// Creation time when known
    /// </summary>
    DateTimeOffset? CreatedTime { get; set; }

    /// <summary>
    /// Last modification time when known
    /// </summary>
    DateTimeOffset? LastModificationTime { get; set; }

    /// <summary>
    /// Gets full path to this blob which is a combination of folder path and blob name
    /// </summary>
    string FullPath { get; }

    /// <summary>
    /// Custom provider-specific properties. Key names are case-insensitive.
    /// </summary>
    Dictionary<string, object> Properties { get; }

    /// <summary>
    /// Try to get property and cast it to a specified type
    /// </summary>
    bool TryGetProperty<TValue>(string name, out TValue value, TValue defaultValue = default);

    /// <summary>
    /// User defined metadata. Key names are case-insensitive.
    /// </summary>
    Dictionary<string, string> Metadata { get; }

    /// <summary>
    /// Tries to add properties in pairs when value is not null
    /// </summary>
    void TryAddProperties(params object[] keyValues);

    /// <summary>
    /// Works just like <see cref="TryAddProperties(object[])"/> but prefixes all the keys
    /// </summary>
    void TryAddPropertiesWithPrefix(string prefix, params object[] keyValues);

    /// <summary>
    /// Tries to add properties from dictionary by key names
    /// </summary>
    void TryAddPropertiesFromDictionary(IDictionary<string, string> source, params string[] keyNames);

    /// <summary>
    /// Returns true if this item is a folder and it's a root folder
    /// </summary>
    bool IsRootFolder { get; }

    /// <summary>
    /// Full blob info, i.e type, id and path
    /// </summary>
    string ToString();

    /// <summary>
    /// Converts blob attributes (user metadata to byte array)
    /// </summary>
    byte[] AttributesToByteArray();

    /// <summary>
    /// Appends attributes from byte array representation
    /// </summary>
    void AppendAttributesFromByteArray(byte[] data);

    /// <summary>
    /// Prepends path to this blob's path without modifying blob's properties
    /// </summary>
    void PrependPath(string path);

    /// <summary>
    /// Changes full path of this blob without modifying any other property
    /// </summary>
    void SetFullPath(string fullPath);

    internal object Tag { get; set; }
}
