namespace Storage.Net.Blob
{
   public class ListOptions
   {
      public string FolderPath { get; set; }

      public string Prefix { get; set; }

      public bool Recurse { get; set; }
   }
}
