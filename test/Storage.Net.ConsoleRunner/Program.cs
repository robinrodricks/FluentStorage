using System;
using System.Linq;
using System.Reflection;

namespace Storage.Net.ConsoleRunner
{
   class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine("Hello World!");

         var all = Assembly.GetEntryAssembly().GetReferencedAssemblies().Select(Assembly.Load).ToList();
      }
   }
}
