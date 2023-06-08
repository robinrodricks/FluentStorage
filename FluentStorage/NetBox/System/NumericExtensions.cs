namespace System {
    using NetBox;

	/// <summary>
	/// <see cref="long"/> extension methods
	/// </summary>
	public static class LongExtensions {
        /// <summary>
        /// Converts number to readable size string in IEC format, i.e. 1024 converts to "1.02 KiB"
        /// </summary>
        public static string ToFileSizeString(this long number) {
            return ByteFormat.ToString(number, ByteFormat.Standard.Iec, null);
        }

        /// <summary>
        /// Converts number to readable size string in SI format, i.e. 1024 converts to "1.02 KB"
        /// </summary>
        public static string ToFileSizeUiString(this long number) {
            return ByteFormat.ToString(number, ByteFormat.Standard.Si, null);
        }
    }

	public static class IntExtensions {
        /// <summary>
        /// Converts number to readable size string in IEC format, i.e. 1024 converts to "1.02 KiB"
        /// </summary>
        public static string ToFileSizeString(this int number) {
            return ByteFormat.ToString(number, ByteFormat.Standard.Iec, null);
        }

        /// <summary>
        /// Converts number to readable size string in SI format, i.e. 1024 converts to "1.02 KB"
        /// </summary>
        public static string ToFileSizeUiString(this int number) {
            return ByteFormat.ToString(number, ByteFormat.Standard.Si, null);
        }

        /// <summary>
        /// Converts number to seconds
        /// </summary>
        /// <param name="number">Number of seconds</param>
        /// <returns>Timespan values</returns>
        public static TimeSpan Seconds(this int number) {
            return TimeSpan.FromSeconds(number);
        }

        /// <summary>
        /// Converts number to minutes
        /// </summary>
        /// <param name="number">Number of minutes</param>
        /// <returns>Timespan value</returns>
        public static TimeSpan Minutes(this int number) {
            return TimeSpan.FromMinutes(number);
        }

        /// <summary>
        /// Converts number to hours 
        /// </summary>
        /// <param name="number">Number of hours</param>
        /// <returns>Timespan value</returns>
        public static TimeSpan Hours(this int number) {
            return TimeSpan.FromHours(number);
        }
    }
}