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

      public IEnumerator<KeyValuePair<string, TableCell>> GetEnumerator()
      {
         return _keyToValue.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      public void Add(KeyValuePair<string, TableCell> item)
      {
         Add(item.Key, item.Value);
      }

      public void Clear()
      {
         _keyToValue.Clear();
      }

      public bool Contains(KeyValuePair<string, TableCell> item)
      {
         return _keyToValue.ContainsKey(item.Key);
      }

      public void CopyTo(KeyValuePair<string, TableCell>[] array, int arrayIndex)
      {
         throw new NotSupportedException();
      }

      public bool Remove(KeyValuePair<string, TableCell> item)
      {
         TableCell value;
         return _keyToValue.TryRemove(item.Key, out value);
      }

      public int Count
      {
         get { return _keyToValue.Count; }
      }

      public bool IsReadOnly
      {
         get { return false; }
      }

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

      public bool ContainsKey(string key)
      {
         return _keyToValue.ContainsKey(key);
      }

      public bool Remove(string key)
      {
         TableCell value;
         return _keyToValue.TryRemove(key, out value);
      }

      public bool TryGetValue(string key, out TableCell value)
      {
         return _keyToValue.TryGetValue(key, out value);
      }

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

      public ICollection<string> Keys
      {
         get { return _keyToValue.Keys; }
      }

      public ICollection<TableCell> Values
      {
         get { return _keyToValue.Values; }
      }

      #endregion

      public TableRow Clone(string rowKey = null, string partitionKey = null)
      {
         var clone = new TableRow(partitionKey ?? PartitionKey, rowKey ?? RowKey);
         foreach(KeyValuePair<string, TableCell> pair in _keyToValue)
         {
            clone._keyToValue[pair.Key] = pair.Value;
         }
         return clone;
      }

      public override string ToString()
      {
         return $"{PartitionKey} : {RowKey}";
      }

      #region [ Value Helpers ]

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
