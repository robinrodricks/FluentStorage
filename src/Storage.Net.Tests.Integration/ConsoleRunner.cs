using LogMagic;
using LogMagic.Receivers;
using System;

namespace Storage.Net.Tests.Integration
{
   public class ConsoleRunner
   {
      private readonly ILog _log = L.G<ConsoleRunner>();

      public static void Main(string[] args)
      {
         L.AddReceiver(new PoshConsoleLogReceiver());
         //L.AddReceiver(new ConsoleLogReceiver());

         new ConsoleRunner().Run();

      }

      public void Run()
      {
         _log.D("test one");
         _log.D("something happened", new DivideByZeroException());
         _log.W("warning message");
         _log.I("information here");
         _log.W("warning message");

         Console.ReadLine();
      }
   }
}
