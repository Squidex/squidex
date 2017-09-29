// ==========================================================================
//  FileLogProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Log.Internal
{
    public class FileLogProcessor : DisposableObjectBase
    {
        private const int MaxQueuedMessages = 1024;
        private const int Retries = 10;
        private readonly BlockingCollection<LogMessageEntry> messageQueue = new BlockingCollection<LogMessageEntry>(MaxQueuedMessages);
        private readonly Task outputTask;
        private readonly string path;

        public FileLogProcessor(string path)
        {
            this.path = path;

            outputTask = Task.Factory.StartNew(ProcessLogQueue, this, TaskCreationOptions.LongRunning);
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
                    if (!ex.Is<OperationCanceledException>())
                    {
                        throw;
                    }
                }
            }
        }

        public void Connect()
        {
            var fileInfo = new FileInfo(path);

            if (!fileInfo.Directory.Exists)
            {
                throw new ConfigurationException($"Log directory '{fileInfo.Directory.FullName}' does not exist.");
            }
        }

        public void EnqueueMessage(LogMessageEntry message)
        {
            messageQueue.Add(message);
        }

        private async Task ProcessLogQueue()
        {
            foreach (var entry in messageQueue.GetConsumingEnumerable())
            {
                for (var i = 1; i <= Retries; i++)
                {
                    try
                    {
                        File.AppendAllText(path, entry.Message + Environment.NewLine, Encoding.UTF8);

                        break;
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(i * 10);

                        if (i == Retries)
                        {
                            Console.WriteLine($"Failed to write to log file '{path}': {ex}");
                        }
                    }
                }
            }
        }

        private static Task ProcessLogQueue(object state)
        {
            var processor = (FileLogProcessor)state;

            return processor.ProcessLogQueue();
        }
    }
}
