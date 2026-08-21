using System;
using Amazon;

namespace FluentStorage.AWS.Utils;

static class AwsConvert {
	public static RegionEndpoint ToRegionEndpoint(this string s) {
		if (s is null)
			throw new ArgumentNullException(nameof(s));

		RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(s);

		return endpoint;
	}
}