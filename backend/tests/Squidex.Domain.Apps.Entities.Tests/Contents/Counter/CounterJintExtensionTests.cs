// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Orleans;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Counter
{
    public class CounterJintExtensionTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICounterGrain counter = A.Fake<ICounterGrain>();
        private readonly JintScriptEngine sut;

        public CounterJintExtensionTests()
        {
            var extensions = new IJintExtension[]
            {
                new CounterJintExtension(grainFactory)
            };

            sut = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
                Options.Create(new JintScriptOptions
                {
                    TimeoutScript = TimeSpan.FromSeconds(2),
                    TimeoutExecution = TimeSpan.FromSeconds(10)
                }), extensions);
        }

        [Fact]
        public void Should_reset_counter()
        {
            var appId = DomainId.NewGuid();

            A.CallTo(() => grainFactory.GetGrain<ICounterGrain>(appId.ToString(), null))
                .Returns(counter);

            A.CallTo(() => counter.ResetAsync("my", 4))
                .Returns(3);

            const string script = @"
                return resetCounter('my', 4);
            ";

            var vars = new ScriptVars
            {
                ["appId"] = appId
            };

            var result = sut.Execute(vars, script).ToString();

            Assert.Equal("3", result);
        }

        [Fact]
        public async Task Should_reset_counter_with_callback()
        {
            var appId = DomainId.NewGuid();

            A.CallTo(() => grainFactory.GetGrain<ICounterGrain>(appId.ToString(), null))
                .Returns(counter);

            A.CallTo(() => counter.ResetAsync("my", 4))
                .Returns(3);

            const string script = @"
                resetCounterV2('my', function(result) {
                    complete(result);
                }, 4);
            ";

            var vars = new ScriptVars
            {
                ["appId"] = appId
            };

            var result = (await sut.ExecuteAsync(vars, script)).ToString();

            Assert.Equal("3", result);
        }

        [Fact]
        public void Should_increment_counter()
        {
            var appId = DomainId.NewGuid();

            A.CallTo(() => grainFactory.GetGrain<ICounterGrain>(appId.ToString(), null))
                .Returns(counter);

            A.CallTo(() => counter.IncrementAsync("my"))
                .Returns(3);

            const string script = @"
                return incrementCounter('my');
            ";

            var vars = new ScriptVars
            {
                ["appId"] = appId
            };

            var result = sut.Execute(vars, script).ToString();

            Assert.Equal("3", result);
        }

        [Fact]
        public async Task Should_increment_counter_with_callback()
        {
            var appId = DomainId.NewGuid();

            A.CallTo(() => grainFactory.GetGrain<ICounterGrain>(appId.ToString(), null))
                .Returns(counter);

            A.CallTo(() => counter.IncrementAsync("my"))
                .Returns(3);

            const string script = @"
                incrementCounter('my', function (result) {
                    complete(result);
                });
            ";

            var vars = new ScriptVars
            {
                ["appId"] = appId
            };

            var result = (await sut.ExecuteAsync(vars, script)).ToString();

            Assert.Equal("3", result);
        }
    }
}
