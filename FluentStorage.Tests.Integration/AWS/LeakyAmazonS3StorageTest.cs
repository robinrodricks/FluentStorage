﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using FluentStorage.AWS.Blobs;
using FluentStorage.Blobs;
using Xunit;

namespace FluentStorage.Tests.Integration.AWS {
	[Trait("Category", "Blobs")]
	public class LeakyAmazonS3StorageTest {
		private readonly ITestSettings _settings;
		private readonly IAwsS3BlobStorage _storage;

		public LeakyAmazonS3StorageTest() {
			_settings = Settings.Instance;

			_storage = (IAwsS3BlobStorage)StorageFactory.Blobs.AwsS3(
			   _settings.AwsAccessKeyId, _settings.AwsSecretAccessKey, null, _settings.AwsTestBucketName, _settings.AwsTestBucketRegion);
		}
	}
}
