using System;

namespace DeploymentApp.Utilities
{
    public class ConsoleLogger : ILogger
    {
        public void Start(string message)
        {
            Console.Write((message + "...").PadRight(80));
        }

        public void Success()
        {
            Console.WriteLine("succeeded.");
        }

        public void Skipped(string reason)
        {
            Console.WriteLine($"skipped: {reason}");
        }

        public void Failed(Exception ex)
        {
            Console.WriteLine($"failed with {ex.Message}.");
        }
    }
}