using Config.Net;

namespace Storage.Net.Tests.Integration
{
   static class TestSettings
   {
      public static readonly Setting<string> AzureStorageName = new Setting<string>("Azure.Storage.Name", null);

      public static readonly Setting<string> AzureStorageKey = new Setting<string>("Azure.Storage.Key", null);

      public static readonly Setting<string> ServiceBusConnectionString = new Setting<string>("Azure.ServiceBus.ConnectionString", null);

      public static readonly Setting<string> OneDriveClientId = new Setting<string>("OneDrive.ClientId", null);

      public static readonly Setting<string> OneDriveClientSecret = new Setting<string>("OneDrive.ClientSecret", null);

      public static readonly Setting<string> AwsAccessKeyId = new Setting<string>("Aws.AccessKeyId", null);

      public static readonly Setting<string> AwsSecretAccessKey = new Setting<string>("Aws.SecretAccessKey", null);

      public static readonly Setting<string> AwsTestBucketName = new Setting<string>("Aws.TestBucketName", null);
   }
}
