using System;

namespace FluentStorage.Utils.Hashing;

internal static class Crc32Hash {

	private static readonly uint[][] Crc32Table = CreateCrc32Table();

	/// <summary>
	/// Compute the CRC-32 hash for the giveen byte array.
	/// </summary>
	public static byte[] Compute(byte[] bytes) {
		uint crc = 0xFFFFFFFF;

		int offset = 0;
		int length = bytes.Length;

		// Process 8 bytes at a time
		while (length >= 8) {
			crc ^= BitConverter.ToUInt32(bytes, offset);

			crc =
				Crc32Table[7][(byte)(crc)] ^
				Crc32Table[6][(byte)(crc >> 8)] ^
				Crc32Table[5][(byte)(crc >> 16)] ^
				Crc32Table[4][(byte)(crc >> 24)] ^
				Crc32Table[3][bytes[offset + 4]] ^
				Crc32Table[2][bytes[offset + 5]] ^
				Crc32Table[1][bytes[offset + 6]] ^
				Crc32Table[0][bytes[offset + 7]];

			offset += 8;
			length -= 8;
		}

		// Remaining bytes
		while (length-- > 0) {
			crc = Crc32Table[0][(byte)(crc ^ bytes[offset++])] ^ (crc >> 8);
		}

		crc ^= 0xFFFFFFFF;

		return new[] {(byte)(crc >> 24),(byte)(crc >> 16),(byte)(crc >> 8),(byte)crc};
	}

	private static uint[][] CreateCrc32Table() {
		const uint polynomial = 0xEDB88320;

		var table = new uint[8][];

		table[0] = new uint[256];

		for (uint i = 0; i < 256; i++) {
			uint crc = i;

			for (int j = 0; j < 8; j++) {
				crc = (crc & 1) != 0
					? polynomial ^ (crc >> 1)
					: crc >> 1;
			}

			table[0][i] = crc;
		}

		for (int i = 1; i < 8; i++) {
			table[i] = new uint[256];

			for (int j = 0; j < 256; j++) {
				uint crc = table[i - 1][j];

				table[i][j] = table[0][crc & 0xFF] ^ (crc >> 8);
			}
		}

		return table;
	}
}