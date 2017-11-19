// ==========================================================================
//  ConsoleLogProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Log.Internal
{
    public class ConsoleLogProcessor : DisposableObjectBase
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
            try
            {
                if (!IsDisposed)
                {
                    messageQueue.Add(message);
                }
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine("Console queue disposed.");
            }
        }

        private void ProcessLogQueue()
        {
            foreach (var entry in messageQueue.GetConsumingEnumerable())
            {
                console.WriteLine(entry.Color, entry.Message);
            }
        }

        private static void ProcessLogQueue(object state)
        {
            var processor = (ConsoleLogProcessor)state;

            processor.ProcessLogQueue();
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                messageQueue.CompleteAdding();
                messageQueue.Dispose();

                try
                {
                    outputTask.Wait(1500);
                }
                catch (Exception ex)
                {
                    if (!ex.Is<OperationCanceledException>())
                    {
                        throw;
                    }
                }
            }
        }
    }
}
