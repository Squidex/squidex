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

namespace Squidex.Infrastructure.Actors
{
    public abstract class Actor : IDisposable
    {
        private readonly ActionBlock<object> block;
        private bool isStopped;

        private sealed class StopMessage
        {
        }

        private sealed class ErrorMessage
        {
            public Exception Exception { get; set; }
        }

        protected Actor()
        {
            var options = new ExecutionDataflowBlockOptions
            {
                MaxMessagesPerTask = -1,
                MaxDegreeOfParallelism = 1,
                BoundedCapacity = 10
            };

            block = new ActionBlock<object>(Handle, options);
        }

        public void Dispose()
        {
            StopAndWaitAsync().Wait();
        }

        protected async Task DispatchAsync(object message)
        {
            Guard.NotNull(message, nameof(message));

            await block.SendAsync(message);
        }

        protected async Task FailAsync(Exception exception)
        {
            Guard.NotNull(exception, nameof(exception));

            await block.SendAsync(new ErrorMessage { Exception = exception });
        }

        protected async Task StopAndWaitAsync()
        {
            await block.SendAsync(new StopMessage());
            await block.Completion;
        }

        protected virtual Task OnStop()
        {
            return TaskHelper.Done;
        }

        protected virtual Task OnError(Exception exception)
        {
            return TaskHelper.Done;
        }

        protected virtual Task OnMessage(object message)
        {
            return TaskHelper.Done;
        }

        private async Task Handle(object message)
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
