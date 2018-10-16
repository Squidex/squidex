// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Log.Internal;

namespace Squidex.Infrastructure.Log
{
    public sealed class FileChannel : DisposableObjectBase, ILogChannel
    {
        private readonly FileLogProcessor processor;
        private readonly object lockObject = new object();
        private bool isInitialized;

        public FileChannel(string path)
        {
            Guard.NotNullOrEmpty(path, nameof(path));

            processor = new FileLogProcessor(path);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                processor.Dispose();
            }
        }

        public void Log(SemanticLogLevel logLevel, string message)
        {
            if (!isInitialized)
            {
                lock (lockObject)
                {
                    if (!isInitialized)
                    {
                        processor.Initialize();

                        isInitialized = true;
                    }
                }
            }

            processor.EnqueueMessage(new LogMessageEntry { Message = message });
        }
    }
}
