namespace NetBox {
    using global::System;
    using Xunit;

    public class ByteArrayExtensionsTest {
        [Theory]
        [InlineData(null, null)]
        [InlineData(new byte[] { }, "")]
        [InlineData(new byte[] { 0, 1, 2, 3, 4, 5 }, "000102030405")]
        public void ToHexString_Variable_Variable(byte[] input, string expected) {
            string? actual = input.ToHexString();

            Assert.Equal(expected, actual);
        }
    }
}