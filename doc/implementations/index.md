# Implementations

This page lists known implementation of storage primitives

| Library | Status | Blobs | Tables | Messaging |
|---------|--------|-------|--------|-----------|
|[built-in](local-disk.md)|stable|Files|CSV files||
|[Storage.Net.Microsoft.Azure](microsoft-azure.md)|stable|Azure Storage Block Blobs and Append Blobs|Azure Storage Tables|- Azure Storage Queues<br>-Azure Service Bus Queues (.NET 4.5.1 only)<br>-Azure Service Bus Topics (.NET 4.5.1 only)<br>-Azure Event Hub|
|[Storage.Net.Microsoft.Azure.DataLake.Store](microsoft-azure-datalakestore.md)|alpha|Data Lake Store|||
|[Storage.Net.Amazon.AWS](amazon-aws.md)|stable|Amazon S3 (.NET 4.5.1 only)|||
|[Storage.Net.Microsoft.ServiceFabric](microsoft-servicefabric.md)|alpha||||