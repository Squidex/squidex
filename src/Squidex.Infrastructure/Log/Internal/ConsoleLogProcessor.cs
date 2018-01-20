// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Log.Internal
{
    public sealed class ConsoleLogProcessor : DisposableObjectBase
    {
        private const int MaxQueuedMessages = 1024;
        private readonly IConsole console;
        private readonly BlockingCollection<LogMessageEntry> messageQueue = new BlockingCollection<LogMessageEntry>(MaxQueuedMessages);
        private readonly Task outputTask;

        public ConsoleLogProcessor()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                console = new WindowsLogConsole(true);
            }
            else
            {
                console = new AnsiLogConsole(false);
            }

            outputTask = Task.Factory.StartNew(ProcessLogQueue, this, TaskCreationOptions.LongRunning);
        }

        public void EnqueueMessage(LogMessageEntry message)
        {
            if (!messageQueue.IsAddingCompleted)
            {
                try
                {
                    messageQueue.Add(message);
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to enqueue log message: {ex}.");
                }
            }

            WriteMessage(message);
        }

        private static void ProcessLogQueue(object state)
        {
            var processor = (ConsoleLogProcessor)state;

            processor.ProcessLogQueue();
        }

        private void ProcessLogQueue()
        {
            foreach (var entry in messageQueue.GetConsumingEnumerable())
            {
                WriteMessage(entry);
            }
        }

        private void WriteMessage(LogMessageEntry entry)
        {
            console.WriteLine(entry.Color, entry.Message);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                messageQueue.CompleteAdding();

                try
                {
                    outputTask.Wait(1500);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to shutdown log queue grateful: {ex}.");
                }
            }
        }
    }
}
