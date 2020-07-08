using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Databricks.Client;
using Newtonsoft.Json;
using Storage.Net.Blobs;

namespace Storage.Net.Databricks
{
   class JobsStorage : GenericBlobStorage
   {
      private readonly IJobsApi _jobs;

      public JobsStorage(IJobsApi jobs) => _jobs = jobs;

      protected override bool CanListHierarchy => false;


      protected override async Task<IReadOnlyCollection<Blob>> ListAtAsync(
         string path, ListOptions options, CancellationToken cancellationToken)
      {
         if(StoragePath.IsRootPath(path))
         {
            IEnumerable<Job> jobs = await _jobs.List();

            var jobsBlobs = new List<Blob>();
            foreach(Job job in jobs)
            {
               jobsBlobs.Add(await ToBlobAsync(job));
            }

            return jobsBlobs;
         }

         // need job ID here - find by job name (crap!)
         string jobName = StoragePath.Split(path)[0];
         long? jobId = await GetJobIdFromJobNameAsync(jobName);
         IReadOnlyCollection<Run> allRuns = await GetAllJobRunsAsync(jobId.Value);
         List<Blob> rr = allRuns.Select(r => ToBlob(jobName, r)).ToList();
         rr.Reverse();  // jobs are better to see in reverse order - newest first
         return rr;
      }

      private async Task<IReadOnlyCollection<Run>> GetAllJobRunsAsync(long jobId)
      {
         var result = new List<Run>();
         RunList runsList;
         int offset = 0;
         do
         {
            runsList = await _jobs.RunsList(jobId, offset, 500, false, false).ConfigureAwait(false);
            List<Run> runs = runsList.Runs.ToList();
            offset += runs.Count;
            result.AddRange(runs);
         }
         while(runsList.HasMore);

         return result;
      }

      public override async Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default)
      {
         // download run: path is [job name]/[run id]

         string[] parts = StoragePath.Split(fullPath);

         if(parts.Length == 2)
         {
            if(long.TryParse(parts[1], out long runId))
            {
               (string notebookOutput, string error, Run run) = await _jobs.RunsGetOutput(runId);

               return notebookOutput == null
                  ? null
                  : new MemoryStream(Encoding.UTF8.GetBytes(notebookOutput));
            }
         }

         return null;
      }

      private async Task<long?> GetJobIdFromJobNameAsync(string jobName)
      {
         IEnumerable<Job> allJobs = await _jobs.List();

         foreach(Job j in allJobs)
         {
            if(j.Settings.Name == jobName)
            {
               return j.JobId;
            }
         }

         return null;
      }

      private async Task<Blob> ToBlobAsync(Job dbJob)
      {
         var blob = new Blob(dbJob.Settings.Name, BlobItemKind.Folder);
         blob.LastModificationTime = dbJob.CreatedTime;
         blob.TryAddProperties(
            "ObjectType", "job",
            "Id", dbJob.JobId,
            "CreatorUserName", dbJob.CreatorUserName,
            "Settings", Module.AsDbJson(dbJob.Settings));

         // get last run
         IReadOnlyCollection<Run> allRuns = await GetAllJobRunsAsync(dbJob.JobId);
         Run lastRun = allRuns.LastOrDefault();
         if(lastRun != null)
         {
            blob.TryAddProperties(
               "LastRunLifeCycleState", lastRun.State.LifeCycleState,
               "LastRunResultState", lastRun.State.ResultState,
               "LastRunEndTime", lastRun.EndTime,
               "LastRunExecutionDuration", lastRun.ExecutionDuration);
         }

         return blob;
      }

      private static Blob ToBlob(string jobName, Run dbRun)
        {
         var blob = new Blob(jobName, dbRun.RunId.ToString(), BlobItemKind.File);
         blob.LastModificationTime = dbRun.EndTime ?? dbRun.StartTime;
         blob.TryAddProperties(
            "StartTime", dbRun.StartTime,
            "EndTime", dbRun.EndTime,
            "ExecutionDuration", dbRun.ExecutionDuration,
            "IsCompleted", dbRun.IsCompleted,
            "JobId", dbRun.JobId,
            "NumberInJob", dbRun.NumberInJob,
            "RunId", dbRun.RunId,
            "RunPageUrl", dbRun.RunPageUrl,
            "Schedule", Module.AsDbJson(dbRun.Schedule),
            "SetupDuration", dbRun.SetupDuration,
            "LifeCycleState", dbRun.State.LifeCycleState,
            "ResultState", dbRun.State.ResultState,
            "StateMessage", dbRun.State.StateMessage,
            "Task", Module.AsDbJson(dbRun.Task),
            "Trigger", dbRun.Trigger);
         return blob;
      }
   }
}
