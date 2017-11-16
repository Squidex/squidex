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
        public async Task Should_handle_async_messages_sequentially()
        {
            var source = Enumerable.Range(1, 100);
            var target = new List<int>();

            foreach (var item in source)
            {
                sut.DispatchAsync(() =>
                {
                    target.Add(item);

                    return TaskHelper.Done;
                }).Forget();
            }

            await sut.StopAndWaitAsync();

            Assert.Equal(source, target);
        }

        [Fact]
        public async Task Should_handle_sync_messages_sequentially()
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
    }
}
