namespace System
{
   /// <summary>
   /// <see cref="DateTime"/> extension methods
   /// </summary>
   public static class DateTimeExtensions
   {
      /// <summary>
      /// Strips time from the date structure
      /// </summary>
      public static DateTime RoundToDay(this DateTime time)
      {
         return new DateTime(time.Year, time.Month, time.Day);
      }

      /// <summary>
      /// Changes to the end of day time, i.e. hours, minutes and seconds are changed to 23:59:59
      /// </summary>
      /// <param name="time"></param>
      /// <returns></returns>
      public static DateTime EndOfDay(this DateTime time)
      {
         return new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);
      }

      /// <summary>
      /// Rounds to the closest minute
      /// </summary>
      /// <param name="time">Input date</param>
      /// <param name="round">Closest minute i.e. 15, 30, 45 etc.</param>
      /// <param name="roundLeft">Whether to use minimum or maximum value. For example
      /// when time is 13:14 and rounding is to every 15 minutes, when this parameter is true
      /// the result it 13:00, otherwise 13:15</param>
      /// <returns></returns>
      public static DateTime RoundToMinute(this DateTime time, int round, bool roundLeft)
      {
         int minute = time.Minute;
         int leftover = minute % round;
         if (leftover == 0) return time;
         int addHours = 0;
         minute -= leftover;

         if (!roundLeft) minute += round;
         if (minute > 59)
         {
            minute = minute % 60;
            addHours = 1;
         }

         return new DateTime(time.Year, time.Month, time.Day, time.Hour + addHours, minute, 0);
      }

      /// <summary>
      /// Strips off details after seconds
      /// </summary>
      /// <param name="time"></param>
      /// <returns></returns>
      public static DateTime RoundToSecond(this DateTime time)
      {
         return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Kind);
      }

      /// <summary>
      /// Returns true if the date is today's date.
      /// </summary>
      public static bool IsToday(this DateTime time)
      {
         DateTime now = DateTime.UtcNow;

         return now.Year == time.Year &&
            now.Month == time.Month &&
            now.Day == time.Day;
      }

      /// <summary>
      /// Returns true if the date is tomorrow's date.
      /// </summary>
      public static bool IsTomorrow(this DateTime time)
      {
         TimeSpan diff = DateTime.UtcNow - time;

         return diff.TotalDays >= 1 && diff.TotalDays < 2;
      }

      /// <summary>
      /// Returns date in "HH:mm" format
      /// </summary>
      public static string ToHourMinuteString(this DateTime time)
      {
         return time.ToString("HH:mm");
      }

      /// <summary>
      /// Formats date in ISO 8601 format
      /// </summary>
      public static string ToIso8601DateString(this DateTime time)
      {
         return time.ToString("o");
      }
   }
}
