// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Contents.Counter;

public class CounterJintExtensionTests : GivenContext
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
        A.CallTo(() => counterService.ResetAsync(AppId.Id, "my", 4, default))
            .Returns(3);

        const string script = @"
                resetCounter('my', 4);
            ";

        var vars = new ScriptVars
        {
            ["appId"] = AppId.Id
        };

        var actual = sut.Execute(vars, script).ToString();

        Assert.Equal("3", actual);
    }

    [Fact]
    public async Task Should_reset_counter_with_callback()
    {
        A.CallTo(() => counterService.ResetAsync(AppId.Id, "my", 4, A<CancellationToken>._))
            .Returns(3);

        const string script = @"
                resetCounterV2('my', function(actual) {
                    complete(actual);
                }, 4);
            ";

        var vars = new ScriptVars
        {
            ["appId"] = AppId.Id
        };

        var actual = (await sut.ExecuteAsync(vars, script, ct: CancellationToken)).ToString();

        Assert.Equal("3", actual);
    }

    [Fact]
    public void Should_increment_counter()
    {
        A.CallTo(() => counterService.IncrementAsync(AppId.Id, "my", A<CancellationToken>._))
            .Returns(3);

        const string script = @"
                incrementCounter('my');
            ";

        var vars = new ScriptVars
        {
            ["appId"] = AppId.Id
        };

        var actual = sut.Execute(vars, script).ToString();

        Assert.Equal("3", actual);
    }

    [Fact]
    public async Task Should_increment_counter_with_callback()
    {
        A.CallTo(() => counterService.IncrementAsync(AppId.Id, "my", A<CancellationToken>._))
            .Returns(3);

        const string script = @"
                incrementCounter('my', function (actual) {
                    complete(actual);
                });
            ";

        var vars = new ScriptVars
        {
            ["appId"] = AppId.Id
        };

        var actual = (await sut.ExecuteAsync(vars, script, ct: CancellationToken)).ToString();

        Assert.Equal("3", actual);
    }
}
