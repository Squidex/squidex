// ==========================================================================
//  ActorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Actors
{
    public class ActorTests
    {
        public class SuccessMessage
        {
            public int Counter { get; set; }
        }

        public class FailedMessage
        {
        }

        private sealed class MyActor : Actor, IActor
        {
            public List<object> Invokes { get; } = new List<object>();

            public void Tell(Exception exception)
            {
                FailAsync(exception).Forget();
            }

            public void Tell(object message)
            {
                DispatchAsync(message).Forget();
            }

            public Task StopAsync()
            {
                return StopAndWaitAsync();
            }

            protected override Task OnStop()
            {
                Invokes.Add(true);

                return TaskHelper.Done;
            }

            protected override Task OnError(Exception exception)
            {
                Invokes.Add(exception);

                return TaskHelper.Done;
            }

            protected override Task OnMessage(object message)
            {
                if (message is FailedMessage)
                {
                    throw new InvalidOperationException();
                }

                Invokes.Add(message);

                return TaskHelper.Done;
            }
        }

        private readonly MyActor sut = new MyActor();

        [Fact]
        public async Task Should_invoke_with_exception()
        {
            sut.Tell(new InvalidOperationException());

            await sut.StopAsync();

            Assert.True(sut.Invokes[0] is InvalidOperationException);
        }

        [Fact]
        public async Task Should_handle_messages_sequentially()
        {
            sut.Tell(new SuccessMessage { Counter = 1 });
            sut.Tell(new SuccessMessage { Counter = 2 });
            sut.Tell(new SuccessMessage { Counter = 3 });

            await sut.StopAsync();

            sut.Invokes.ShouldBeEquivalentTo(new List<object>
            {
                new SuccessMessage { Counter = 1 },
                new SuccessMessage { Counter = 2 },
                new SuccessMessage { Counter = 3 },
                true
            });
        }

        [Fact]
        public async Task Should_raise_error_event_when_event_handling_failed()
        {
            sut.Tell(new FailedMessage());
            sut.Tell(new SuccessMessage { Counter = 2 });
            sut.Tell(new SuccessMessage { Counter = 3 });

            await sut.StopAsync();

            Assert.True(sut.Invokes[0] is InvalidOperationException);

            sut.Invokes.Skip(1).ShouldBeEquivalentTo(new List<object>
            {
                new SuccessMessage { Counter = 2 },
                new SuccessMessage { Counter = 3 },
                true
            });
        }

        [Fact]
        public async Task Should_not_handle_messages_after_stop()
        {
            sut.Tell(new SuccessMessage { Counter = 1 });

            await sut.StopAsync();

            sut.Tell(new SuccessMessage { Counter = 2 });
            sut.Tell(new SuccessMessage { Counter = 3 });

            sut.Tell(new InvalidOperationException());

            sut.Invokes.ShouldBeEquivalentTo(new List<object>
            {
                new SuccessMessage { Counter = 1 },
                true
            });
        }

        [Fact]
        public void Should_call_stop_on_dispose()
        {
            sut.Tell(new SuccessMessage { Counter = 1 });

            sut.Dispose();

            sut.Invokes.ShouldBeEquivalentTo(new List<object>
            {
                new SuccessMessage { Counter = 1 },
                true
            });
        }
    }
}
