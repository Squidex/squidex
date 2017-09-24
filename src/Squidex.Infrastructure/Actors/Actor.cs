// ==========================================================================
//  Actor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Squidex.Infrastructure.Tasks;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Infrastructure.Actors
{
    public abstract class Actor : IActor, IDisposable
    {
        private readonly ActionBlock<IMessage> block;
        private bool isStopped;

        private sealed class StopMessage : IMessage
        {
        }

        private sealed class ErrorMessage : IMessage
        {
            public Exception Exception;
        }

        protected Actor()
        {
            block = new ActionBlock<IMessage>(Handle, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, BoundedCapacity = 10 });
        }

        public void Dispose()
        {
            StopAsync().Wait();
        }

        public async Task StopAsync()
        {
            await block.SendAsync(new StopMessage());
            await block.Completion;
        }

        public Task SendAsync(IMessage message)
        {
            Guard.NotNull(message, nameof(message));

            return block.SendAsync(message);
        }

        public Task SendAsync(Exception exception)
        {
            Guard.NotNull(exception, nameof(exception));

            return block.SendAsync(new ErrorMessage { Exception = exception });
        }

        protected virtual Task OnStop()
        {
            return TaskHelper.Done;
        }

        protected virtual Task OnError(Exception exception)
        {
            return TaskHelper.Done;
        }

        protected virtual Task OnMessage(IMessage message)
        {
            return TaskHelper.Done;
        }

        private async Task Handle(IMessage message)
        {
            if (isStopped)
            {
                return;
            }

            switch (message)
            {
                case StopMessage stopMessage:
                {
                    isStopped = true;

                    block.Complete();

                    await OnStop();

                    break;
                }

                case ErrorMessage errorMessage:
                {
                    await OnError(errorMessage.Exception);

                    break;
                }

                default:
                {
                    try
                    {
                        await OnMessage(message);
                    }
                    catch (Exception ex)
                    {
                        await OnError(ex);
                    }

                    break;
                }
            }
        }
    }
}
