using System;

namespace DeploymentApp.Utilities
{
    public class ConsoleHelper
    {
        public static void Start(string message)
        {
            Console.Write((message + "...").PadRight(80));
        }

        public static void Success()
        {
            Console.WriteLine("succeeded.");
        }

        public static void Skipped(string reason)
        {
            Console.WriteLine($"skipped: {reason}");
        }

        public static void Failed(Exception ex)
        {
            Console.WriteLine($"failed with {ex.Message}.");
        }
    }
}