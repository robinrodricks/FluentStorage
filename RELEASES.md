# Release Notes

#### FluentStorage.Azure.ServiceBus 6.0.0
(thanks GiampaoloGabba)
 - Fix: Completely rewrite package for the new SDK `Azure.Messaging.ServiceBus`
 - New: New API for construction `AzureServiceBusTopicReceiver` and `AzureServiceBusQueueReceiver`
 - New: `IMessenger` interface uses structured string format for referencing queues/topics
 - New: `IAzureServiceBusMessenger` interface with API to send/create/delete/count a queue/topic/subscription

#### FluentStorage 5.4.1
 - Fix: Remove unused dependency package `Newtonsoft.Json` from main project

#### FluentStorage 5.4.0
(thanks dammitjanet)
 - New: Constructor for `SymmetricEncryptionSink` and `AesSymmetricEncryptionSink` to pass IV and key 
 - New: Constructor for `EncryptedSink` abstract base class to pass in IV and key
 - New: Additional tests for encryption/decryption repeatability when the decrpytion IV is known
 - Fix: Resolved Xunit errors and issue with with Xunit `FileData` attribute only finding a single file per test
 - Fix: Package updates and consolidation to latest Xunit

#### FluentStorage 5.3.0
(thanks dammitjanet)
 - New: Addition of `AesSymmetricEncryptionSink` and `WithAesSymmetricEncryption` extension
 - Fix: Obsolesence of `SymmetricEncryptionSink` and `WithSymmetricEncryption` extension
 - New: Updated Tests for `AesSymmetricEncryptionSink`
 - New: Additional Blob/Stream file tests and XUnit `FileDataAttribute` to support tests

#### FluentStorage.AWS 5.2.2
 - Fix: `AwsS3BlobStorage` checks if a bucket exists before trying to create one (thanks AntMaster7)

#### FluentStorage.Azure.Blobs 5.2.2
 - Fix: Upgrade `System.Text.Json` package from v4 to v7

#### FluentStorage 5.2.2
 - Fix: Upgrade `System.Text.Json` package from v4 to v7
 - Fix: Local storage: Handling of `LastModificationTime` and `CreatedTime`
 - Fix: Local storage: `LastAccessTimeUtc` is saved as a Universal sortable string in the Blob properties

#### FluentStorage.SFTP 5.2.3
 - Fix: Various fixes to `ListAsync` path handling
 - Fix: Upgrade `SSH.NET` package from v2016 to v2020

#### FluentStorage.SFTP 5.2.2
 - New: Added support for a root path in the SFTP connection string
 - Fix: `GetBlobsAsync` should return an array with a single null if the file does not exist
 - Fix: `WriteAsync` will create the directory if it does not exist

#### FluentStorage 5.2.1
 - New: Implement `LocalDiskMessenger.StartProcessorAsync`
 - Package: Add Nuget reference to `Newtonsoft.Json 13.0.3`
 - Package: Remove Nuget reference to `Newtonsoft.Json 12.x.x`
 - Package: Remove Nuget reference to `NetBox` and add the required utilities within this library

#### FluentStorage.AWS 5.2.1
 - New: Implement server-side filtering in `AwsS3DirectoryBrowser.ListAsync` by supplying a `FilePrefix` (thanks SRJames)

#### FluentStorage.FTP 5.2.1
 - Fix: Support for the append parameter in FluentFtpBlobStorage (thanks candoumbe)
 - Fix: `IBlobStorage.WriteAsync` will create the directory hierarchy if required (thanks candoumbe)

#### FluentStorage 5.1.1
 - Fix: Implementation of `LocalDiskMessenger.StartProcessorAsync` (issue [#14](https://github.com/robinrodricks/FluentStorage/issues/14))(`netstandard2.1` / `net6.0` and above

#### FluentStorage 5.0.0
 - New: Introducing the FluentStorage set of libraries created from Storage.NET
 - New: Added SFTP provider using [SSH.NET](https://github.com/sshnet/SSH.NET)
 - Fix: FTP provider [FluentFTP](https://github.com/robinrodricks/FluentFTP) updated to v44
 - Fix: AWS Nugets bumped to latest versions as of Jan 2023
 - Fix: All nuget packages now target `netstandard2.0`,`netstandard2.1`,`net50`,`net60`
 - Change: Refactored package structure to simplify naming scheme
 - Change: Refactored entire codebase to simplify code organization
 - New: Documentation wiki created with one page per provider