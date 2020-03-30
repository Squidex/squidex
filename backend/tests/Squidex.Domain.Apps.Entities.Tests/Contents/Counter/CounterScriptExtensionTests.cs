// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Orleans;
using Squidex.Domain.Apps.Core.Scripting;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Counter
{
    public class CounterScriptExtensionTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICounterGrain counter = A.Fake<ICounterGrain>();
        private readonly JintScriptEngine sut;

        public CounterScriptExtensionTests()
        {
            var extensions = new IScriptExtension[]
            {
                new CounterScriptExtension(grainFactory)
            };

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            sut = new JintScriptEngine(cache, extensions)
            {
                Timeout = TimeSpan.FromSeconds(1)
            };
        }

        [Fact]
        public void Should_reset_counter()
        {
            var appId = Guid.NewGuid();

            A.CallTo(() => grainFactory.GetGrain<ICounterGrain>(appId, null))
                .Returns(counter);

            A.CallTo(() => counter.ResetAsync("my", 4))
                .Returns(3);

            const string script = @"
                return resetCounter('my', 4);
            ";

            var context = new ScriptContext
            {
                ["appId"] = appId
            };

            var result = sut.Interpolate(context, script);

            Assert.Equal("3", result);
        }

        [Fact]
        public void Should_increment_counter()
        {
            var appId = Guid.NewGuid();

            A.CallTo(() => grainFactory.GetGrain<ICounterGrain>(appId, null))
                .Returns(counter);

            A.CallTo(() => counter.IncrementAsync("my"))
                .Returns(3);

            const string script = @"
                return incrementCounter('my');
            ";

            var context = new ScriptContext
            {
                ["appId"] = appId
            };

            var result = sut.Interpolate(context, script);

            Assert.Equal("3", result);
        }
    }
}
