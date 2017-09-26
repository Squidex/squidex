// ==========================================================================
//  InMemoryPubSubTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Xunit;

namespace Squidex.Infrastructure
{
    public class InMemoryPubSubTests
    {
        private readonly InMemoryPubSub sut = new InMemoryPubSub();

        [Fact]
        public void Should_publish_to_handlers()
        {
            var channel1Events = new List<string>();
            var channel2Events = new List<string>();

            sut.Subscribe("channel1", x =>
            {
                channel1Events.Add(x);
            });

            sut.Subscribe("channel1", x =>
            {
                channel1Events.Add(x);
            });

            sut.Subscribe("channel2", x =>
            {
                channel2Events.Add(x);
            });

            sut.Publish("channel1", "1", true);
            sut.Publish("channel1", "2", true);
            sut.Publish("channel1", "3", false);

            sut.Publish("channel2", "a", true);
            sut.Publish("channel2", "b", true);

            Assert.Equal(new[] { "1", "1", "2", "2" }, channel1Events.ToArray());
            Assert.Equal(new[] { "a", "b" }, channel2Events.ToArray());
        }
    }
}
