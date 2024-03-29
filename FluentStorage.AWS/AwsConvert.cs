﻿using System;
using System.Collections.Generic;
using System.Text;
using Amazon;

namespace FluentStorage.AWS {
	static class AwsConvert {
		public static RegionEndpoint ToRegionEndpoint(this string s) {
			if (s is null)
				throw new ArgumentNullException(nameof(s));

			RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(s);

			return endpoint;
		}
	}
}