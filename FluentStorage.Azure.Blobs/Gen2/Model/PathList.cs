﻿using System.Collections.Generic;

namespace FluentStorage.Azure.Blobs.Gen2.Model {
	class PathList {
		public Gen2Path[] Paths { get; set; }

		public override string ToString() => $"{Paths?.Length ?? 0}";
	}
}
