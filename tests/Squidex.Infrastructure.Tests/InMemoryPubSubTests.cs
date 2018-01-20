// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure
{
    public class InMemoryPubSubTests
    {
        private readonly InMemoryPubSub sut = new InMemoryPubSub();

        private sealed class MessageA
        {
            public string Text { get; set; }
        }

        private sealed class MessageB
        {
            public string Text { get; set; }
        }

        [Fact]
        public void Should_publish_to_handlers()
        {
            var channel1Events = new List<string>();
            var channel2Events = new List<string>();

            sut.Subscribe<MessageA>(m =>
            {
                channel1Events.Add(m.Text);
            });

            sut.Subscribe<MessageA>(m =>
            {
                channel1Events.Add(m.Text);
            });

            sut.Subscribe<MessageB>(m =>
            {
                channel2Events.Add(m.Text);
            });

            sut.Publish(new MessageA { Text = "1" }, true);
            sut.Publish(new MessageA { Text = "2" }, true);
            sut.Publish(new MessageA { Text = "3" }, false);

            sut.Publish(new MessageB { Text = "a" }, true);
            sut.Publish(new MessageB { Text = "b" }, true);

            Assert.Equal(new[] { "1", "1", "2", "2" }, channel1Events.ToArray());
            Assert.Equal(new[] { "a", "b" }, channel2Events.ToArray());
        }

        [Fact]
        public async Task Should_make_request_reply_requests()
        {
            sut.ReceiveAsync<int, int>(x =>
            {
                return Task.FromResult(x + x);
            }, true);

            var response = await sut.RequestAsync<int, int>(2, TimeSpan.FromSeconds(2), true);

            Assert.Equal(4, response);
        }

        [Fact]
        public async Task Should_timeout_when_response_is_too_slow()
        {
            sut.ReceiveAsync<int, int>(async x =>
            {
                await Task.Delay(1000);

                return x + x;
            }, true);

            await Assert.ThrowsAsync<TaskCanceledException>(() => sut.RequestAsync<int, int>(1, TimeSpan.FromSeconds(0.5), true));
        }

        [Fact]
        public async Task Should_timeout_when_nobody_responds()
        {
            await Assert.ThrowsAsync<TaskCanceledException>(() => sut.RequestAsync<int, int>(2, TimeSpan.FromSeconds(0.5), true));
        }
    }
}
