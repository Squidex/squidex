// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Counter;

public class CounterJintExtensionTests
{
    private readonly ICounterService counterService = A.Fake<ICounterService>();
    private readonly JintScriptEngine sut;

    public CounterJintExtensionTests()
    {
        var extensions = new IJintExtension[]
        {
            new CounterJintExtension(counterService)
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

        A.CallTo(() => counterService.ResetAsync(appId, "my", 4, default))
            .Returns(3);

        const string script = @"
                return resetCounter('my', 4);
            ";

        var vars = new ScriptVars
        {
            ["appId"] = appId
        };

        var actual = sut.Execute(vars, script).ToString();

        Assert.Equal("3", actual);
    }

    [Fact]
    public async Task Should_reset_counter_with_callback()
    {
        var appId = DomainId.NewGuid();

        A.CallTo(() => counterService.ResetAsync(appId, "my", 4, A<CancellationToken>._))
            .Returns(3);

        const string script = @"
                resetCounterV2('my', function(actual) {
                    complete(actual);
                }, 4);
            ";

        var vars = new ScriptVars
        {
            ["appId"] = appId
        };

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

        Assert.Equal("3", actual);
    }

    [Fact]
    public void Should_increment_counter()
    {
        var appId = DomainId.NewGuid();

        A.CallTo(() => counterService.IncrementAsync(appId, "my", A<CancellationToken>._))
            .Returns(3);

        const string script = @"
                return incrementCounter('my');
            ";

        var vars = new ScriptVars
        {
            ["appId"] = appId
        };

        var actual = sut.Execute(vars, script).ToString();

        Assert.Equal("3", actual);
    }

    [Fact]
    public async Task Should_increment_counter_with_callback()
    {
        var appId = DomainId.NewGuid();

        A.CallTo(() => counterService.IncrementAsync(appId, "my", A<CancellationToken>._))
            .Returns(3);

        const string script = @"
                incrementCounter('my', function (actual) {
                    complete(actual);
                });
            ";

        var vars = new ScriptVars
        {
            ["appId"] = appId
        };

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

        Assert.Equal("3", actual);
    }
}
