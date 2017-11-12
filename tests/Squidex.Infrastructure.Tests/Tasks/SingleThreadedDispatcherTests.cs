// ==========================================================================
//  SingleThreadedDispatcherTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Tasks
{
    public class SingleThreadedDispatcherTests
    {
        private readonly SingleThreadedDispatcher sut = new SingleThreadedDispatcher();

        [Fact]
        public async Task Should_handle_messages_sequentially()
        {
            var source = Enumerable.Range(1, 100);
            var target = new List<int>();

            foreach (var item in source)
            {
                sut.DispatchAsync(() => target.Add(item)).Forget();
            }

            await sut.StopAndWaitAsync();

            Assert.Equal(source, target);
        }

        /*
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
        */
    }
}
