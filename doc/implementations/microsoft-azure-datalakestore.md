# Microsoft Azure Data Lake Store (Preview)

Microsoft Azure implementations reside in a separate package hosted on [NuGet](https://www.nuget.org/packages/Storage.Net.Microsoft.Azure.DataLake.Store/). Follow the link for installation instructions.

This package tries to abstract access to [Azure Data Lake Store](https://azure.microsoft.com/en-gb/services/data-lake-store/) and making them available as `IBlobStorage`.

## Using

> WARNING!!! `Microsoft.Azure.Management.DataLake.Store` has a dependency on `Newtonsoft.Json v6` therefore you may need to add this to app.config:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
   <runtime>
      <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
         <dependentAssembly>
            <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
            <bindingRedirect oldVersion="0.0.0.0-9.0.0.0" newVersion="9.0.0.0" />
         </dependentAssembly>
      </assemblyBinding>
   </runtime>
</configuration>
```

This library supports service-to-service authentication either with a **client secret** or a **client certificate** (not supported in preview yet).

To create an instance of the client:

```csharp
IBlobStorage storage = StorageFactory.Blobs.AzureDataLakeStoreByClientSecret(
	"account_name", credentials);
```

where:
- **Account name** is the name of the ADLS account as displayed in the portal: ![Adl 04](adl-04.png)
- **Credentials** is an instance of `NetworkCredential` class where:
  - **domain** is active directory __tenant id__
  - **username** is active directory principal's __client id__ (application id)
  - **password** is active directory principal's __secret__

see appendix below on how to obtain this information.

### Important Implementation Notes

- Uploading a file always overwrites existing file if it exists, otherwise a new file is created. This still takes one network call.
- Appending to a file checks if a file exists first, so this operation results in two network calls.


## Appendix. Creating a Service Principal

### For authentication using Client Secret

#### Create Active Directory Application

Go to **Active Directory / App registrations**:

![Adl 00](adl-00.png)

Press **Add button**. Type any name for the application, but leave **Application Type** as **Web app / API** and set **sign-on url** to **`http://localhost`**:

![Adl 01](adl-01.png)

Now open the application and create a new key. We need to write down this **key** and **Application ID**:

![Adl 02](adl-02.png)

You can get the **tenant id** (sometimes called a **diretory id** or **domain**) from the directory properties:

![Adl 03](adl-03.png)

- 1
- 2
- 3