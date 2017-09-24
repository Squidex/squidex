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
        public class SuccessMessage : IMessage
        {
            public int Counter { get; set; }
        }

        public class FailedMessage : IMessage
        {
        }

        private sealed class MyActor : Actor
        {
            public List<object> Invokes { get; } = new List<object>();

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

            protected override Task OnMessage(IMessage message)
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
            sut.SendAsync(new InvalidOperationException()).Forget();

            await sut.StopAsync();

            Assert.True(sut.Invokes[0] is InvalidOperationException);
        }

        [Fact]
        public async Task Should_handle_messages_sequentially()
        {
            sut.SendAsync(new SuccessMessage { Counter = 1 }).Forget();
            sut.SendAsync(new SuccessMessage { Counter = 2 }).Forget();
            sut.SendAsync(new SuccessMessage { Counter = 3 }).Forget();

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
            sut.SendAsync(new FailedMessage()).Forget();
            sut.SendAsync(new SuccessMessage { Counter = 2 }).Forget();
            sut.SendAsync(new SuccessMessage { Counter = 3 }).Forget();

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
            sut.SendAsync(new SuccessMessage { Counter = 1 }).Forget();

            sut.StopAsync().Forget();

            sut.SendAsync(new SuccessMessage { Counter = 2 }).Forget();
            sut.SendAsync(new SuccessMessage { Counter = 3 }).Forget();
            sut.SendAsync(new InvalidOperationException()).Forget();

            await sut.StopAsync();

            sut.Invokes.ShouldBeEquivalentTo(new List<object>
            {
                new SuccessMessage { Counter = 1 },
                true
            });
        }
    }
}
