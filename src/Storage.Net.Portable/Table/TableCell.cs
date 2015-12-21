using System;

namespace Storage.Net.Table
{
   /// <summary>
   /// Reprecents data cell in a table storage
   /// </summary>
   public class TableCell
   {
      /// <summary>
      /// Creates a new instance of TableCell
      /// </summary>
      /// <param name="value">Raw value</param>
      /// <param name="dataType">Original data type</param>
      public TableCell(string value, CellType dataType)
      {
         RawValue = value;
         DataType = dataType;
      }

      public string RawValue { get; private set; }

      public CellType DataType { get; private set; }

      public static implicit operator string(TableCell cell)
      {
         return cell?.RawValue;
      }

      public static implicit operator TableCell(string s)
      {
         if(s == null) return null;
         return new TableCell(s, CellType.String);
      }

      public static implicit operator long(TableCell cell)
      {
         if(cell == null) return default(long);

         long result;
         long.TryParse(cell.RawValue, out result);
         return result;
      }

      public static implicit operator TableCell(long l)
      {
         return new TableCell(l.ToString(), CellType.Long);
      }
      
      public static implicit operator int(TableCell cell)
      {
         if(cell == null) return default(int);

         int result;
         int.TryParse(cell.RawValue, out result);
         return result;
      }

      public static implicit operator TableCell(int i)
      {
         return new TableCell(i.ToString(), CellType.Int);
      }

      public static implicit operator DateTime(TableCell cell)
      {
         if(cell == null) return default(DateTime);

         return new DateTime(long.Parse(cell.RawValue));
      }

      public static implicit operator TableCell(DateTime d)
      {
         return new TableCell(d.Ticks.ToString(), CellType.DateTime);
      }

      public static implicit operator bool(TableCell cell)
      {
         if(cell == null) return default(bool);

         return cell.RawValue == "1" || "true".Equals(cell.RawValue, StringComparison.OrdinalIgnoreCase);
      }

      public static implicit operator TableCell(bool b)
      {
         return new TableCell(b ? "1": "0", CellType.Boolean);
      }

      public static implicit operator TableCell(Enum value)
      {
         return new TableCell(value.ToString(), CellType.Enum);
      }

      public override string ToString()
      {
         return $"{RawValue} ({DataType})";
      }
   }
}
