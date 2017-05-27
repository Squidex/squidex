// ==========================================================================
//  FileChannel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.Log.Internal;

namespace Squidex.Infrastructure.Log
{
    public sealed class FileChannel : DisposableObjectBase, ILogChannel, IExternalSystem
    {
        private readonly FileLogProcessor processor;

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
            processor.EnqueueMessage(new LogMessageEntry { Message = message, IsError = logLevel >= SemanticLogLevel.Error });
        }

        public void Connect()
        {
            processor.Connect();
        }
    }
}
