namespace NetBox {
    using System;

	public static class ByteFormat {
        //http://en.wikipedia.org/wiki/Kibibyte

        private const long Kb = 1000;         //kilobyte
        private const long KiB = 1024;        //kikibyte
        private const long Mb = Kb * 1000;      //megabyte
        private const long MiB = KiB * 1024;    //memibyte
        private const long Gb = Mb * 1000;      //gigabyte
        private const long GiB = MiB * 1024;    //gigibyte
        private const long Tb = Gb * 1000;      //terabyte
        private const long TiB = GiB * 1024;    //tebibyte
        private const long Pb = Tb * 1024;      //petabyte
        private const long PiB = TiB * 1024;    //pepibyte

        public enum Standard {
            /// <summary>
            ///  International System of Units
            /// </summary>
            Si,

            /// <summary>
            /// International Electrotechnical Commission
            /// </summary>
            Iec
        }

        /// <summary>
        /// Returns the best formatted string representation of a byte value
        /// </summary>
        /// <param name="bytes">number of bytes</param>
        /// <param name="st"></param>
        /// <returns>formatted string</returns>
        private static string ToString(long bytes, Standard st = Standard.Iec) {
            return ToString(bytes, st, null);
        }

        /// <summary>
        /// Returns the best formatted string representation of a byte value
        /// </summary>
        /// <param name="bytes">number of bytes</param>
        /// <param name="st"></param>
        /// <param name="customFormat">Defines a custom numerical format for the conversion.
        /// If this parameters is null or empty the default format will be used 0.00</param>
        /// <returns>formatted string</returns>
        public static string ToString(long bytes, Standard st, string? customFormat) {
            if(bytes == 0)
                return "0";

            if(string.IsNullOrEmpty(customFormat))
                customFormat = "0.00";

            string result;
            bool isNegative = bytes < 0;
            bytes = Math.Abs(bytes);

            if(st == Standard.Si) {
                if(bytes < Mb)
                    result = BytesToKb(bytes, customFormat);

                else if(bytes < Gb)
                    result = BytesToMb(bytes, customFormat);

                else if(bytes < Tb)
                    result = BytesToGb(bytes, customFormat);

                else if(bytes < Pb)
                    result = BytesToTb(bytes, customFormat);

                else
                    result = BytesToPb(bytes, customFormat);
            } else {
                if(bytes < MiB)
                    result = BytesToKib(bytes, customFormat);

                else if(bytes < GiB)
                    result = BytesToMib(bytes, customFormat);

                else if(bytes < TiB)
                    result = BytesToGib(bytes, customFormat);

                else if(bytes < PiB)
                    result = BytesToTib(bytes, customFormat);

                else
                    result = BytesToPib(bytes, customFormat);
            }

            return isNegative ? ("-" + result) : (result);
        }

        private static string BytesToPb(long bytes, string? customFormat) {
            double tb = bytes / ((double)Pb);
            return tb.ToString(customFormat) + " PB";
        }
        private static string BytesToPib(long bytes, string? customFormat) {
            double tb = bytes / ((double)PiB);
            return tb.ToString(customFormat) + " PiB";
        }

        private static string BytesToTb(long bytes, string? customFormat) {
            double tb = bytes / ((double)Tb);
            return tb.ToString(customFormat) + " TB";
        }
        private static string BytesToTib(long bytes, string? customFormat) {
            double tb = bytes / ((double)TiB);
            return tb.ToString(customFormat) + " TiB";
        }

        private static string BytesToGb(long bytes, string? customFormat) {
            double gb = bytes / ((double)Gb);
            return gb.ToString(customFormat) + " GB";
        }
        private static string BytesToGib(long bytes, string? customFormat) {
            double gb = bytes / ((double)GiB);
            return gb.ToString(customFormat) + " GiB";
        }

        private static string BytesToMb(long bytes, string? customFormat) {
            double mb = bytes / ((double)Mb);
            return mb.ToString(customFormat) + " MB";
        }
        private static string BytesToMib(long bytes, string? customFormat) {
            double mb = bytes / ((double)MiB);
            return mb.ToString(customFormat) + " MiB";
        }

        private static string BytesToKb(long bytes, string? customFormat) {
            double kb = bytes / ((double)Kb);
            return kb.ToString(customFormat) + " KB";
        }
        private static string BytesToKib(long bytes, string? customFormat) {
            double kb = bytes / ((double)KiB);
            return kb.ToString(customFormat) + " KiB";
        }
    }
}