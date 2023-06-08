namespace NetBox
{
   using global::System;
   using NetBox.Generator;
   using Xunit;

   public class RandomGeneratorTest
   {
      [Fact]
      public void RandomString_TwoStrings_NotEqual()
      {
         string s1 = RandomGenerator.RandomString;
         string s2 = RandomGenerator.RandomString;

         Assert.NotEqual(s1, s2);
      }

      [Theory]
      [InlineData(10)]
      [InlineData(100)]
      public void RandomString_SpecificLengthNoNulls_Matches(int length)
      {
         Assert.Equal(length, RandomGenerator.GetRandomString(length, false)!.Length);
      }

      [Fact]
      public void RandomBool_Anything_DoesntCrash()
      {
         bool b = RandomGenerator.RandomBool;
      }

      [Fact]
      public void RandomEnum_Random_Random()
      {
         EnumExample? random = RandomGenerator.GetRandomEnum<EnumExample>();

         //not sure how to validate
      }

      [Fact]
      public void RandomEnumNonGeneric_Random_Random()
      {
         EnumExample? random = (EnumExample?)RandomGenerator.RandomEnum(typeof(EnumExample));

         //not sure how to validate
      }

      [Fact]
      public void RandomInt_Random_Random()
      {
         int i = RandomGenerator.RandomInt;
      }

      [Fact]
      public void RandomDate_Interval_Matches()
      {
         DateTime randomDate = RandomGenerator.GetRandomDate(DateTime.UtcNow, DateTime.UtcNow.AddDays(10));
      }

      [Fact]
      public void RandomDate_Random_Random()
      {
         DateTime randomDate = RandomGenerator.RandomDate;
      }


      [Theory]
      [InlineData(-10L, 100L)]
      [InlineData(5L, 10L)]
      [InlineData(-100L, -1L)]
      [InlineData(0, 67)]
      public void RandomLong_VaryingRange_InRange(long min, long max)
      {
         long random = RandomGenerator.GetRandomLong(min, max);

         Assert.True(random >= min);
         Assert.True(random <= max);
      }

      [Fact]
      public void RandomLong_TwoGenerations_NotEqual()
      {
         long l1 = RandomGenerator.RandomLong;
         long l2 = RandomGenerator.RandomLong;

         Assert.NotEqual(l1, l2);
      }

      [Fact]
      public void RandomUri_TwoGenerations_NotEqual()
      {
         Uri? u1 = RandomGenerator.GetRandomUri(false);
         Uri? u2 = RandomGenerator.RandomUri;

         Assert.NotEqual(u1, u2);
      }

      private enum EnumExample
      {
         One,
         Two,
         Three
      }
   }
}