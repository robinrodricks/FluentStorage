using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Storage.Net.Table
{
   /// <summary>
   /// Represents a table row in table data structure.
   /// </summary>
   public class TableRow : IDictionary<string, TableCell>, IEquatable<TableRow>
   {
      private readonly ConcurrentDictionary<string, TableCell> _keyToValue = new ConcurrentDictionary<string, TableCell>(); 

      /// <summary>
      /// Creates a new instance from partition key and row key
      /// </summary>
      public TableRow(string partitionKey, string rowKey) : this(new TableRowId(partitionKey, rowKey))
      {
      }

      /// <summary>
      /// Creates a new instance from <see cref="TableRowId"/>
      /// </summary>
      /// <param name="id"></param>
      public TableRow(TableRowId id)
      {
         if (id == null) throw new ArgumentNullException(nameof(id));
         Id = id;
      }

      /// <summary>
      /// Row ID
      /// </summary>
      public TableRowId Id { get; private set; }

      /// <summary>
      /// Partition key
      /// </summary>
      public string PartitionKey { get { return Id.PartitionKey; }}

      /// <summary>
      /// Row key
      /// </summary>
      public string RowKey { get { return Id.RowKey; }}

      /// <summary>
      /// Checks row equality
      /// </summary>
      public bool Equals(TableRow other)
      {
         if(ReferenceEquals(other, null)) return false;
         if(ReferenceEquals(other, this)) return true;
         if(GetType() != other.GetType()) return false;

         return other.Id.PartitionKey == Id.PartitionKey && other.Id.RowKey == Id.RowKey;
      }

      #region [IDictionary]

      /// <summary>
      /// Get enumerator for cells inside the row
      /// </summary>
      public IEnumerator<KeyValuePair<string, TableCell>> GetEnumerator()
      {
         return _keyToValue.GetEnumerator();
      }

      /// <summary>
      /// Get enumerator for cells inside the row
      /// </summary>
      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      /// <summary>
      /// IDictionary.Add
      /// </summary>
      public void Add(KeyValuePair<string, TableCell> item)
      {
         Add(item.Key, item.Value);
      }

      /// <summary>
      /// Clears cells
      /// </summary>
      public void Clear()
      {
         _keyToValue.Clear();
      }

      /// <summary>
      /// IDictionary.Contains
      /// </summary>
      public bool Contains(KeyValuePair<string, TableCell> item)
      {
         return _keyToValue.ContainsKey(item.Key);
      }

      /// <summary>
      /// IDictionary.CopyTo
      /// </summary>
      public void CopyTo(KeyValuePair<string, TableCell>[] array, int arrayIndex)
      {
         throw new NotSupportedException();
      }

      /// <summary>
      /// IDictionary.Remove
      /// </summary>
      public bool Remove(KeyValuePair<string, TableCell> item)
      {
         TableCell value;
         return _keyToValue.TryRemove(item.Key, out value);
      }

      /// <summary>
      /// IDictionary.Count
      /// </summary>
      public int Count
      {
         get { return _keyToValue.Count; }
      }

      /// <summary>
      /// IDictionary.IsReadOnly
      /// </summary>
      public bool IsReadOnly
      {
         get { return false; }
      }

      /// <summary>
      /// IDictionary.Add
      /// </summary>
      public void Add(string key, TableCell value)
      {
         if(value == null)
         {
            _keyToValue.TryRemove(key, out value);
         }
         else
         {
            _keyToValue[key] = value;
         }
      }

      /// <summary>
      /// IDictionary.ContainsKey
      /// </summary>
      public bool ContainsKey(string key)
      {
         return _keyToValue.ContainsKey(key);
      }

      /// <summary>
      /// IDictionary.Remove
      /// </summary>
      public bool Remove(string key)
      {
         TableCell value;
         return _keyToValue.TryRemove(key, out value);
      }

      /// <summary>
      /// IDictionary.TryGetValue
      /// </summary>
      public bool TryGetValue(string key, out TableCell value)
      {
         return _keyToValue.TryGetValue(key, out value);
      }

      /// <summary>
      /// IDictionary.this
      /// </summary>
      public TableCell this[string key]
      {
         get
         {
            TableCell value;
            if(!_keyToValue.TryGetValue(key, out value)) return null;
            return value;
         }
         set { Add(key, value); }
      }

      /// <summary>
      /// IDictionary.Keys
      /// </summary>
      public ICollection<string> Keys
      {
         get { return _keyToValue.Keys; }
      }

      /// <summary>
      /// IDictionary.Values
      /// </summary>
      public ICollection<TableCell> Values
      {
         get { return _keyToValue.Values; }
      }

      #endregion

      /// <summary>
      /// Clones the row
      /// </summary>
      /// <param name="rowKey">When specified, the clone receives this value for the Row Key</param>
      /// <param name="partitionKey">When speified, the clone receives this value for the Partition Key</param>
      /// <returns></returns>
      public TableRow Clone(string rowKey = null, string partitionKey = null)
      {
         var clone = new TableRow(partitionKey ?? PartitionKey, rowKey ?? RowKey);
         foreach(KeyValuePair<string, TableCell> pair in _keyToValue)
         {
            clone._keyToValue[pair.Key] = pair.Value;
         }
         return clone;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
         return $"{PartitionKey} : {RowKey}";
      }

      #region [ Value Helpers ]

      /// <summary>
      /// Gets value as enum
      /// </summary>
      public TEnum GetEnum<TEnum>(string key) where TEnum : struct
      {
         if(key == null) return default(TEnum);
         if (!typeof(TEnum).IsEnum()) return default(TEnum);

         TableCell cell;
         if(!_keyToValue.TryGetValue(key, out cell) || cell.RawValue == null) return default(TEnum);

         TEnum value;
         if (!Enum.TryParse(cell.RawValue, true, out value)) return default(TEnum);
         return value;
      }

      #endregion
   }
}
