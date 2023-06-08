namespace NetBox {
    using global::System;
    using global::System.IO;
    using global::System.Text;
    using global::System.Threading.Tasks;
    using Xunit;

    public class StringExtensionsTest {
        [Theory]
        [InlineData("<string>test text</string>", "test text")]
        public void StripHtml_Variable_Variable(string html, string stripped) {
            Assert.Equal(stripped, html.StripHtml());
        }

        [Fact]
        public void Base64_Encode_Decodes() {
            string s = "test string";
            string? s64 = s.Base64Encode();
            string? s2 = s64.Base64Decode();

            Assert.Equal(s, s2);
        }

        [Fact]
        public void ToMemoryStream_TestString_ReadsBack() {
            string input = "test stream";
            using(MemoryStream? ms = input.ToMemoryStream()) {
                string s = Encoding.UTF8.GetString(ms!.ToArray());
                Assert.Equal(input, s);
            }
        }

        [Fact]
        public void ToMemoryStream_EncodingTestString_ReadsBack() {
            string input = "test stream";
            using(var ms = input.ToMemoryStream(Encoding.ASCII)) {
                string s = Encoding.ASCII.GetString(ms!.ToArray());
                Assert.Equal(input, s);
            }
        }

        [Theory]
        [InlineData("the %variable%", "%", "%", true, "%variable%")]
        [InlineData("the %variable%", "%", "%", false, "variable")]
        [InlineData("this is a test", "Sean", "test", false, null)]
        [InlineData("this is a test", " is", "test", false, " a ")]
        public void FindTagged_Variations(string input, string startTag, string endTag, bool includeOuter, string expected) {
            Assert.Equal(expected, input.FindTagged(startTag, endTag, includeOuter));
        }

        [Fact]
        public void ReplaceTextBetween_ReturnsPastInStringIfStartTokenDoesNotExistInPassedInString() {
            string s = "this is a test";
            Assert.Equal("this is a test", s.ReplaceTagged("Sean", "test", "me", false));
        }

        [Fact]
        public void ReplaceTextBetween_ReturnsPastInStringIfEndTokenDoesNotExistInPassedInString() {
            string s = "this is a test";
            Assert.Equal("this is a test", s.ReplaceTagged("This", "Sean", "me", false));
        }

        [Fact]
        public void ReplaceTextBetween_RemovesOuterTokens() {
            string s = "This is a test";
            Assert.Equal("This unit test", s.ReplaceTagged(" is", "a ", " unit ", true));
        }

        [Fact]
        public void ReplaceTextBetween_DoesNotRemoveOuterTokens() {
            string s = "This is a test";
            Assert.Equal("This is unit a test", s.ReplaceTagged(" is", "a ", " unit ", false));
        }

        [Theory]
        [InlineData("One Two", "OneTwo")]
        [InlineData("one two Three", "OneTwoThree")]
        [InlineData("one tWo Three", "OneTwoThree")]
        [InlineData(null, null)]
        [InlineData("one tw", "OneTw")]
        public void SpacedToCamelCase_Variable_Variable(string input, string expected) {
            Assert.Equal(expected, input.SpacedToCamelCase());
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("O", "O")]
        [InlineData("o", "O")]
        [InlineData("one", "One")]
        [InlineData("tWo", "Two")]
        [InlineData("1234", "1234")]
        public void Capitalize_Variable_Variable(string input, string expected) {
            Assert.Equal(expected, input.Capitalize());
        }

        [Theory]
        [InlineData(null, null, null, null)]
        [InlineData("word", "ord", 1, null)]
        [InlineData("word", "ord", -3, null)]
        [InlineData("word", "word", -4, null)]
        [InlineData("word", "rd", -2, null)]
        [InlineData("word", "word", 0, null)]
        [InlineData("word", "word", null, null)]
        [InlineData("word", "wor", null, 3)]
        [InlineData("word", "wo", null, -2)]
        [InlineData("word", "", null, -10)]
        [InlineData("word", "or", 1, -1)]
        public void Slice_Variable_Variable(string input, string expected, int? start, int? end) {
            string result = input.Slice(start, end)!;

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, null, null, null)]
        [InlineData("key:value", null, "key:value", null)]
        [InlineData("key:value", ":", "key", "value")]
        [InlineData("key:value", "=", "key:value", null)]
        [InlineData("key:", ":", "key", "")]
        public void SplitByDelimiter_Variable_Variable(string input, string delimiter, string expectedKey, string expectedValue) {
            Tuple<string, string?>? result = input.SplitByDelimiter(delimiter);

            string? key = result?.Item1;
            string? value = result?.Item2;

            Assert.Equal(expectedKey, key);
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void Empty_string_from_hex_returns_empty_byte_array() {
            byte[]? r = "".FromHexToBytes();
            Assert.NotNull(r);
            Assert.Empty(r);
        }

        [Fact]
        public void Null_string_from_hex_returns_null() {
            byte[]? r = StringExtensions.FromHexToBytes(null);
            Assert.Null(r);
        }

        [Fact]
        public void Invalid_hex_from_hex_returns_null() {
            byte[]? r = "zztop".FromHexToBytes();
            Assert.Null(r);
        }

        [Fact]
        public void One_byte_from_hex_returns_empty_byte_array() {
            byte[]? r = "z".FromHexToBytes();
            Assert.NotNull(r);
            Assert.Empty(r);

        }

        [Fact]
        public void RemoteLinesContaining() {
            string text = @"#Header
![dfdsfdf](000.png)
some text";

            string nt = text.RemoveLinesContaining("000.png").Trim();

            Assert.Equal(@"#Header
some text", nt, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("the string", "the+string")]
        [InlineData("lk>i*", "lk%3Ei*")]
        public void UrlEncode_Variable_Variable(string decoded, string encoded) {
            string encodedNow = decoded.UrlEncode()!;

            Assert.Equal(encoded, encodedNow);
        }

        [Fact]
        public async Task DownloadToTempFile() {
            using(TempFile? tf = await "https://microsoft.com".DownloadUrlToTempFile()) {
                Assert.True(File.Exists(tf!));
            }
        }

        public class XmlDoc {
            public string? SV { get; set; }
        }

        public enum XmlEnum {
            One,
            Two
        }

        private class HiddenDoc {
            public string? SV { get; set; }
        }

        public class NonXmlDoc {
#pragma warning disable IDE0060 // Remove unused parameter
            public NonXmlDoc(int i) {

            }
#pragma warning restore IDE0060 // Remove unused parameter
        }

    }
}