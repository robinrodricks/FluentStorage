using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Storage.Net.Mssql
{
   static class ExceptonTranslator
   {
      public static Exception Translate(Exception ex)
      {
         return null;
      }

      public static SqlException GetSqlException(Exception ex)
      {
         SqlException sqlEx = null;

         if (ex is SqlException sqlEx1)
         {
            sqlEx = sqlEx1;
         }
         else if (ex is InvalidOperationException ioex && ioex.InnerException is SqlException sqlEx2)
         {
            sqlEx = sqlEx2;
         }

         return sqlEx;
      }
   }
}
