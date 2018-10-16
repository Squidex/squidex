// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
        private StreamWriter writer;

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
                finally
                {
                    writer.Dispose();
                }
            }
        }

        public void Initialize()
        {
            var fileInfo = new FileInfo(path);
            try
            {
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                var fs = new FileStream(fileInfo.FullName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

                writer = new StreamWriter(fs, Encoding.UTF8);
                writer.AutoFlush = true;

                writer.WriteLine($"--- Started Logging {DateTime.UtcNow} ---", 1);
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Log directory '{fileInfo.Directory.FullName}' does not exist or cannot be created.", ex);
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
                        writer.WriteLine(entry.Message);
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
