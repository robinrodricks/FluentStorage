using NetBox.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage.Net.Table
{
   class EntityConverter
   {
      private TopLevelDictionarySerializer _serializer = new TopLevelDictionarySerializer();
      private const string PartitionKeyPropName = "PartitionKey";
      private const string RowKeyPropName = "RowKey";

      public T Convert<T>(TableRow row) where T: class, new()
      {
         var data = new Dictionary<string, object>
         {
            [PartitionKeyPropName] = row.PartitionKey,
            [RowKeyPropName] = row.RowKey
         };
         foreach (KeyValuePair<string, TableCell> cell in row)
         {
            data[cell.Key] = cell.Value.RawValue;
         }

         object result = _serializer.Deserialize(typeof(T), data);

         return result as T;
      }

      public TableRow[] Convert<T>(T[] entities)
      {
         List<Dictionary<string, object>> dics =
            entities.Select(e => _serializer.Serialize(e))
            .ToList();

         bool invalid = dics.Any(d => !d.ContainsKey(PartitionKeyPropName) || !d.ContainsKey(RowKeyPropName));

         if(invalid)
         {
            throw new ArgumentException($"one of the entities is missing {PartitionKeyPropName} or {RowKeyPropName}", nameof(entities));
         }

         var result = new List<TableRow>();
         foreach(Dictionary<string, object> dic in dics)
         {
            var row = new TableRow((string)dic[PartitionKeyPropName], (string)dic[RowKeyPropName]);

            foreach(KeyValuePair<string, object> kvp in dic)
            {
               if (kvp.Key == PartitionKeyPropName) continue;
               if (kvp.Key == RowKeyPropName) continue;
               if (kvp.Value == null) continue;

               Type t = kvp.Value.GetType();

               if (t == typeof(string))
                  row[kvp.Key] = (string)kvp.Value;
               else if (t == typeof(int))
                  row[kvp.Key] = (int)kvp.Value;
               else if (t == typeof(long))
                  row[kvp.Key] = (long)kvp.Value;
               else if (t == typeof(double))
                  row[kvp.Key] = (double)kvp.Value;
               else if (t == typeof(DateTime))
                  row[kvp.Key] = (DateTime)kvp.Value;
               else if (t == typeof(bool))
                  row[kvp.Key] = (bool)kvp.Value;
               else if (t == typeof(Guid))
                  row[kvp.Key] = (Guid)kvp.Value;
               else if (t == typeof(byte[]))
                  row[kvp.Key] = (byte[])kvp.Value;
               else
                  row[kvp.Key] = kvp.ToString();
            }

            result.Add(row);
         }
         return result.ToArray();
      }
   }
}
