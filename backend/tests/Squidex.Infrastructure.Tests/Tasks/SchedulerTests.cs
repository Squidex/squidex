using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Tasks
{
    public class SchedulerTests
    {
        private readonly ConcurrentBag<int> results = new ConcurrentBag<int>();
        private readonly Scheduler sut = new Scheduler();

        [Fact]
        public async Task Should_schedule_single_task()
        {
            Schedule(1);

            await sut.CompleteAsync();

            Assert.Equal(new[] { 1 }, results.ToArray());
        }

        [Fact]
        public async Task Should_schedule_multiple_tasks()
        {
            Schedule(1);
            Schedule(2);

            await sut.CompleteAsync();

            Assert.Equal(new[] { 1, 2 }, results.OrderBy(x => x).ToArray());
        }

        [Fact]
        public async Task Should_schedule_nested_tasks()
        {
            sut.Schedule(async _ =>
            {
                await Task.Delay(1);

                results.Add(1);

                sut.Schedule(async _ =>
                {
                    await Task.Delay(1);

                    results.Add(2);

                    Schedule(3);
                });
            });

            await sut.CompleteAsync();

            Assert.Equal(new[] { 1, 2, 3 }, results.OrderBy(x => x).ToArray());
        }

        [Fact]
        public async Task Should_ignore_schedule_after_completion()
        {
            Schedule(1);

            await sut.CompleteAsync();

            Schedule(3);

            await Task.Delay(50);

            Assert.Equal(new[] { 1 }, results.OrderBy(x => x).ToArray());
        }

        private void Schedule(int value)
        {
            sut.Schedule(async _ =>
            {
                await Task.Delay(1);

                results.Add(value);
            });
        }
    }
}
