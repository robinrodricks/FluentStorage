using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Blobs.Gen2;
using Storage.Net;
using Storage.Net.Microsoft.Azure.Storage.Blobs.Gen2.Model;

namespace Blobs
{
   class ExtendedSdk
   {
      private readonly BlobServiceClient _sdkClient;
      private readonly HttpPipeline _httpPipeline;
      private readonly string _dfsBaseAddress;
      private static readonly JsonSerializerOptions _jo = new JsonSerializerOptions
      {
         PropertyNameCaseInsensitive = true
      };

      static ExtendedSdk()
      {
         _jo.Converters.Add(new DateTimeConverterUsingDateTimeParse());
      }

      public ExtendedSdk(BlobServiceClient sdkClient, string accountName)
      {
         _sdkClient = sdkClient ?? throw new ArgumentNullException(nameof(sdkClient));

         _httpPipeline = GetHttpPipeline(sdkClient);
         _dfsBaseAddress = GetDfsBaseAddress(accountName);
      }

      private static string GetDfsBaseAddress(string accountName) => 
         $"https://{accountName}.dfs.core.windows.net/";

      private static HttpPipeline GetHttpPipeline(BlobServiceClient sdkClient)
      {
         PropertyInfo httpPipelineProperty = 
            typeof(BlobServiceClient).GetProperty("Pipeline", BindingFlags.NonPublic | BindingFlags.Instance);

         HttpPipeline httpPipeline = httpPipelineProperty.GetValue(sdkClient) as HttpPipeline;

         if(httpPipeline == null)
            throw new ArgumentException($"cannot find {typeof(HttpPipeline)} via reflection");

         return httpPipeline;
      }

      public async Task<IReadOnlyCollection<Filesystem>> ListFilesystemsAsync(CancellationToken cancellationToken)
      {
         FilesystemList response = await InvokeAsync<FilesystemList>(
            "?resource=account",
            RequestMethod.Get,
            null,
            cancellationToken);

         return response.Filesystems;
      }

      public async Task CreateFilesystemAsync(string name, CancellationToken cancellationToken)
      {
         try
         {
            await InvokeAsync<string>($"{name}?resource=filesystem", RequestMethod.Put, null, cancellationToken)
               .ConfigureAwait(false);
         }
         catch(RequestFailedException ex) when (ex.ErrorCode == "FilesystemAlreadyExists")
         {

         }
      }

      public async Task DeleteFilesystemAsync(string name, CancellationToken cancellationToken)
      {
         try
         {
            await InvokeAsync<string>($"{name}?resource=filesystem", RequestMethod.Delete, null, cancellationToken)
               .ConfigureAwait(false);
         }
         catch(RequestFailedException ex) when (ex.ErrorCode == "FilesystemNotFound")
         {

         }
      }

      public async Task SetAccessControlAsync(
         string fullPath,
         AccessControl accessControl,
         CancellationToken cancellationToken)
      {
         DecomposePath(fullPath, out string filesystemName, out string relativePath, false);

         await InvokeAsync<string>(
            $"{filesystemName}/{relativePath}?action=setAccessControl",
            RequestMethod.Patch,
            new Dictionary<string, string>
            {
               ["x-ms-acl"] = accessControl.ToString()
            },
            cancellationToken);
      }

      public async Task<AccessControl> GetAccessControlAsync(
         string fullPath,
         bool getUpn,
         CancellationToken cancellationToken)
      {
         DecomposePath(fullPath, out string filesystemName, out string relativePath, false);

         IDictionary<string, string> headers = await InvokeAsync<IDictionary<string, string>>(
            $"{filesystemName}/{relativePath}?action=getAccessControl&upn={getUpn}",
            RequestMethod.Head,
            null,
            cancellationToken);

         headers.TryGetValue("x-ms-owner", out string owner);
         headers.TryGetValue("x-ms-group", out string group);
         headers.TryGetValue("x-ms-permissions", out string permissions);
         headers.TryGetValue("x-ms-acl", out string acl);

         return new AccessControl(owner, group, permissions, acl);
      }


      public async Task<TResult> InvokeAsync<TResult>(
         string relativeUrl,
         RequestMethod method,
         IDictionary<string, string> headers,
         CancellationToken cancellationToken)
      {
         HttpMessage message = _httpPipeline.CreateMessage();
         Request request = message.Request;

         var url = new Uri($"{_dfsBaseAddress}{relativeUrl}");

         request.Uri.Reset(url);
         request.Method = method;
         request.Headers.SetValue("x-ms-version", "2019-02-02");

         if(headers != null)
         {
            foreach(KeyValuePair<string, string> header in headers)
            {
               request.Headers.SetValue(header.Key, header.Value);
            }
         }

         await _httpPipeline.SendAsync(message, cancellationToken).ConfigureAwait(false);
         cancellationToken.ThrowIfCancellationRequested();
         CreateAndThrowIfError(message.Response);

         if(typeof(string) == typeof(TResult))
            return default;

         if(typeof(IDictionary<string, string>) == typeof(TResult))
         {
            var raderOnlyResult = new Dictionary<string, string>();
            foreach(HttpHeader header in message.Response.Headers)
            {
               raderOnlyResult[header.Name] = header.Value;
            }
            return (TResult)(object)raderOnlyResult;
         }

         string json;
         using(var src = new StreamReader(message.Response.ContentStream))
         {
            json = await src.ReadToEndAsync().ConfigureAwait(false);
         }

         return JsonSerializer.Deserialize<TResult>(json, _jo);
      }

      private void CreateAndThrowIfError(Response response)
      {
         int code = response.Status;
         if(code == 200 || code == 201 || code == 202 || code == 304)
            return;

         response.Headers.TryGetValue(Constants.HeaderNames.ErrorCode, out string errorCode);

         StringBuilder sb = new StringBuilder()
            .Append("Status: ")
            .Append(response.Status)
            .Append(" (")
            .Append(response.ReasonPhrase)
            .AppendLine(")");

         if(!string.IsNullOrEmpty(errorCode))
         {
            sb
               .AppendLine()
               .Append("ErrorCode: ")
               .AppendLine(errorCode);
         }

         sb
          .AppendLine()
          .AppendLine("Headers:");
         foreach(HttpHeader responseHeader in response.Headers)
         {
            sb
                .Append(responseHeader.Name)
                .Append(": ")
                .AppendLine(responseHeader.Value);
         }

         throw new RequestFailedException(
            code,
            sb.ToString(),
            errorCode,
            null);
      }

      private void DecomposePath(string path, out string filesystemName, out string relativePath, bool requireRelativePath = true)
      {
         GenericValidation.CheckBlobFullPath(path);
         string[] parts = StoragePath.Split(path);

         if(requireRelativePath && parts.Length < 2)
         {
            throw new ArgumentException($"path '{path}' must include filesystem name as root folder, i.e. 'filesystem/path'", nameof(path));
         }

         filesystemName = parts[0];

         relativePath = StoragePath.Combine(parts.Skip(1));
      }

   }
}
