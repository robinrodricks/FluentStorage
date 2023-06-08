namespace NetBox
{
   using global::System;
   using Xunit;

   public class DateTimeExtensionsTest
   {
      [Fact]
      public void RoundToDay_TestDateWithTime_TimeTrimmed()
      {
         Assert.Equal(new DateTime(2015, 09, 10), new DateTime(2015, 09, 10, 14, 17, 35).RoundToDay());
      }

      [Fact]
      public void ToHourMinuteString_Trivial_Trivial()
      {
         Assert.Equal("13:04", new DateTime(2014, 12, 3, 13, 4, 0).ToHourMinuteString());
      }

      [Theory]
      [InlineData("14:15", "14:18", 15, true)]
      [InlineData("14:30", "14:18", 15, false)]
      [InlineData("14:15", "14:15", 15, false)]
      [InlineData("14:15", "14:15", 15, true)]
      [InlineData("15:00", "14:59", 15, false)]
      [InlineData("14:00", "14:01", 15, true)]
      public void RoundToMinute_Variable_Variable(string expected, string actual, int round, bool roundToLeft)
      {
         DateTime actualDate = DateTime.UtcNow.RoundToDay().Add(TimeSpan.Parse(actual));
         DateTime expectedDate = DateTime.UtcNow.RoundToDay().Add(TimeSpan.Parse(expected));
         DateTime convertedDate = actualDate.RoundToMinute(round, roundToLeft);

         Assert.Equal(convertedDate, expectedDate);
      }

      [Fact]
      public void Convert_local_time_to_iso_string()
      {
         DateTime dt = DateTime.Now;

         string s = dt.ToIso8601DateString();

         Assert.Equal(dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK"), s);
      }

      [Fact]
      public void Convert_utc_time_to_iso_string()
      {
         DateTime dt = DateTime.UtcNow;

         string s = dt.ToIso8601DateString();

         Assert.Equal(dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK"), s);
      }

   }
}