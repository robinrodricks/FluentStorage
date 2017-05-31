using Config.Net;
using System.Net;

namespace Storage.Net.Tests
{
   public class TestSettings : SettingsContainer
   {
      #region [ Azure ]

      public readonly Option<string> AzureStorageName = new Option<string>("Azure.Storage.Name", null);

      public readonly Option<string> AzureStorageKey = new Option<string>("Azure.Storage.Key", null);

      public readonly Option<string> ServiceBusConnectionString = new Option<string>("Azure.ServiceBus.ConnectionString", null);

      public readonly Option<string> ServiceBusQueueName = new Option<string>("Azure.ServiceBus.QueueName", "testqueuelocal");

      public readonly Option<string> ServiceBusTopicName = new Option<string>("Azure.ServiceBus.TopicName", "testtopiclocal");

      public readonly Option<string> EventHubConnectionString = new Option<string>("Azure.EventHub.ConnectionString", null);

      public readonly Option<string> EventHubPath = new Option<string>("Azure.EventHub.Path", null);

      public readonly Option<NetworkCredential> AzureDataLakeCredential = new Option<NetworkCredential>("Azure.DataLake.Store.Credential", null);

      public readonly Option<string> AzureDataLakeStoreAccountName = new Option<string>("Azure.DataLake.Store.AccountName", null);

      public readonly Option<string> AzureDataLakeSubscriptionId = new Option<string>("Azure.DataLake.SubscriptionId", null);

      #endregion

      #region [ Amazon Web Services ]

      public readonly Option<string> AwsAccessKeyId = new Option<string>("Aws.AccessKeyId", null);

      public readonly Option<string> AwsSecretAccessKey = new Option<string>("Aws.SecretAccessKey", null);

      public readonly Option<string> AwsTestBucketName = new Option<string>("Aws.TestBucketName", null);

      #endregion

      protected override void OnConfigure(IConfigConfiguration configuration)
      {
         configuration.UseIniFile("c:\\tmp\\integration-tests.ini");
         configuration.UseEnvironmentVariables();
      }

      private static TestSettings _instance;

      public static TestSettings Instance
      {
         get { return _instance ?? (_instance = new TestSettings()); }
      }
   }
}
