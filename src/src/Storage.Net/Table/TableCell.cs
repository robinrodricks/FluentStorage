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

      /// <summary>
      /// Raw value without conversions
      /// </summary>
      public string RawValue { get; private set; }

      /// <summary>
      /// Data type
      /// </summary>
      public CellType DataType { get; private set; }

      /// <summary>
      /// Gets the raw value
      /// </summary>
      /// <param name="cell"></param>
      public static implicit operator string(TableCell cell)
      {
         return cell?.RawValue;
      }

      /// <summary>
      /// Constructs from string value
      /// </summary>
      public static implicit operator TableCell(string s)
      {
         if(s == null) return null;
         return new TableCell(s, CellType.String);
      }

      /// <summary>
      /// Attempts to convert to long data type, and if unsuccessfull returns 0
      /// </summary>
      /// <param name="cell"></param>
      public static implicit operator long(TableCell cell)
      {
         if(cell == null) return default(long);

         long result;
         long.TryParse(cell.RawValue, out result);
         return result;
      }

      /// <summary>
      /// Constructs from long data type
      /// </summary>
      /// <param name="l"></param>
      public static implicit operator TableCell(long l)
      {
         return new TableCell(l.ToString(), CellType.Long);
      }
      
      /// <summary>
      /// Attempts to convert to int data type, and if not successful returns 0
      /// </summary>
      /// <param name="cell"></param>
      public static implicit operator int(TableCell cell)
      {
         if(cell == null) return default(int);

         int result;
         int.TryParse(cell.RawValue, out result);
         return result;
      }

      /// <summary>
      /// Creates an instance from int data type
      /// </summary>
      /// <param name="i"></param>
      public static implicit operator TableCell(int i)
      {
         return new TableCell(i.ToString(), CellType.Int);
      }

      /// <summary>
      /// Attempts to convert to double data type, and if not successful returns 0
      /// </summary>
      /// <param name="cell"></param>
      public static implicit operator double(TableCell cell)
      {
         if (cell == null) return default(double);

         double.TryParse(cell.RawValue, out double result);
         return result;
      }

      /// <summary>
      /// Constructs from double data type
      /// </summary>
      /// <param name="d"></param>
      public static implicit operator TableCell(double d)
      {
         return new TableCell(d.ToString(), CellType.Double);
      }

      /// <summary>
      /// Attempts to convert to Guid data type, and if not successful returns default Guid
      /// </summary>
      /// <param name="cell"></param>
      public static implicit operator Guid(TableCell cell)
      {
         if (cell == null) return default(Guid);

         Guid.TryParse(cell.RawValue, out Guid result);
         return result;
      }

      /// <summary>
      /// Constructs from byte[] data type
      /// </summary>
      /// <param name="bytes"></param>
      public static implicit operator TableCell(byte[] bytes)
      {
         return new TableCell(bytes == null ? null : Convert.ToBase64String(bytes), CellType.ByteArray);
      }

      /// <summary>
      /// Attempts to convert to byte[] data type, and if not successful returns null
      /// </summary>
      /// <param name="cell"></param>
      public static implicit operator byte[](TableCell cell)
      {
         if (cell == null || cell.RawValue == null) return null;

         return cell.RawValue.Base64DecodeAsBytes();
      }


      /// <summary>
      /// Constructs from Guid data type
      /// </summary>
      /// <param name="g"></param>
      public static implicit operator TableCell(Guid g)
      {
         return new TableCell(g.ToString(), CellType.Guid);
      }

      /// <summary>
      /// Attempts to convert to DateTime, and if not successful fails miserably
      /// </summary>
      /// <param name="cell"></param>
      public static implicit operator DateTime(TableCell cell)
      {
         if(cell == null) return default(DateTime);

         return new DateTime(long.Parse(cell.RawValue));
      }

      /// <summary>
      /// Creates a new instance from DateTime
      /// </summary>
      /// <param name="d"></param>
      public static implicit operator TableCell(DateTime d)
      {
         return new TableCell(d.Ticks.ToString(), CellType.DateTime);
      }

      /// <summary>
      /// Attempts to convert to boolean data type, returns false if unsuccessful
      /// </summary>
      /// <param name="cell"></param>
      public static implicit operator bool(TableCell cell)
      {
         if(cell == null) return default(bool);

         return cell.RawValue == "1" || "true".Equals(cell.RawValue, StringComparison.OrdinalIgnoreCase);
      }

      /// <summary>
      /// Creates a new instance from boolean data type
      /// </summary>
      /// <param name="b"></param>
      public static implicit operator TableCell(bool b)
      {
         return new TableCell(b ? "1": "0", CellType.Boolean);
      }

      /// <summary>
      /// Creates a new instance from Enum data type
      /// </summary>
      /// <param name="value"></param>
      public static implicit operator TableCell(Enum value)
      {
         return new TableCell(value.ToString(), CellType.Enum);
      }

      /// <summary>
      /// Gets readable representation
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
         return $"{RawValue} ({DataType})";
      }
   }
}
