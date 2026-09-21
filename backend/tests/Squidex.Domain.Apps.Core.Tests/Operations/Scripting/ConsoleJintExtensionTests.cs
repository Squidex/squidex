// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.HandleRules.Extensions;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Scripting.Extensions;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Flows;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Operations.Scripting;

public class ConsoleJintExtensionTests : IClassFixture<TranslationsFixture>
{
    private readonly IScriptLogStore scriptLogStore = A.Fake<IScriptLogStore>();
    private readonly DomainId appId = DomainId.NewGuid();
    private readonly IScriptEngine sut;

    public ConsoleJintExtensionTests()
    {
        var extensions = new IJintExtension[]
        {
            new ConsoleJintExtension(NullLogger<ConsoleJintExtension>.Instance, scriptLogStore),
        };

        sut = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10),
            }),
            extensions);
    }

    [Fact]
    public void Should_collect_log_entries()
    {
        const string script = @"
                console.log('Hello', 42, true, null, undefined, { a: 1 }, [1, 2]);
                console.info('Info');
                console.warn('Warn');
                console.error('Error');
            ";

        using var log = ScriptLog.Begin("my-script");

        sut.Execute(new ScriptVars(), script);

        Assert.Equal(
        [
            "console.log: Hello 42 true null undefined {\"a\":1} [1,2]",
            "console.info: Info",
            "console.warn: Warn",
            "console.error: Error",
        ], log.ToLines());
    }

    [Fact]
    public async Task Should_collect_log_entries_from_async_script()
    {
        var vars = new ScriptVars
        {
            ["value"] = "Hello",
        };

        const string script = @"
                console.log(value);
            ";

        using var log = ScriptLog.Begin("my-script");

        await sut.ExecuteAsync(vars, script);

        Assert.Equal(["console.log: Hello"], log.ToLines());
    }

    [Fact]
    public void Should_not_fail_without_scope()
    {
        const string script = @"
                console.log('Hello');
                42;
            ";

        var actual = sut.Execute(new ScriptVars(), script).AsNumber;

        Assert.Equal(42, actual);
    }

    [Fact]
    public void Should_limit_number_of_entries()
    {
        const string script = @"
                for (var i = 0; i < 150; i++) {
                    console.log(i);
                }
            ";

        using var log = ScriptLog.Begin("my-script");

        sut.Execute(new ScriptVars(), script);

        var lines = log.ToLines();

        Assert.Equal(ScriptLog.MaxEntries + 1, lines.Count);
        Assert.Equal("... 50 more entries omitted.", lines[^1]);
        Assert.Equal(150, log.Snapshot().TotalEntries);
    }

    [Fact]
    public void Should_truncate_long_entries()
    {
        const string script = @"
                console.log('a'.repeat(2000));
            ";

        using var log = ScriptLog.Begin("my-script");

        sut.Execute(new ScriptVars(), script);

        var line = Assert.Single(log.ToLines());

        Assert.Equal($"console.log: {new string('a', ScriptLog.MaxLength)}...", line);
    }

    [Fact]
    public void Should_restore_previous_scope()
    {
        using var outer = ScriptLog.Begin("my-script");

        using (var inner = ScriptLog.Begin("my-script"))
        {
            Assert.Same(inner, ScriptLog.Current);
        }

        Assert.Same(outer, ScriptLog.Current);
    }

    [Fact]
    public async Task Should_not_add_log_entries_to_validation_error()
    {
        const string script = @"
                console.log('Hello');
                reject('Failed');
            ";

        var ex = await Assert.ThrowsAsync<ValidationException>(() => ScriptLog.CollectAsync("my-script", async () =>
        {
            await sut.ExecuteAsync(new ScriptVars(), script, new ScriptOptions { CanReject = true });
        }));

        Assert.Equal(["Failed"], ex.Errors.Select(x => x.Message));
    }

    [Fact]
    public void Should_write_to_flow_console()
    {
        const string script = @"
                console.log('Log');
                console.info('Info');
                console.warn('Warn');
                console.error('Error');
                console.debug('Debug');
            ";

        var lines = new List<string>();

        FlowConsole.Output = (message, _) => lines.Add(message);
        try
        {
            sut.Execute(new ScriptVars(), script);
        }
        finally
        {
            FlowConsole.Output = null!;
        }

        Assert.Equal(["Log", "INFO: Info", "WARN: Warn", "ERROR: Error", "DEBUG: Debug"], lines);
    }

    [Fact]
    public async Task Should_not_be_replaced_by_rule_extension()
    {
        IScriptEngine engine = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10),
            }),
            [
                new ConsoleJintExtension(NullLogger<ConsoleJintExtension>.Instance, scriptLogStore),
                new EventJintExtension(A.Fake<IUrlGenerator>()),
            ]);

        var vars = new ScriptVars
        {
            ["appId"] = appId,
        };

        const string script = @"
                console.log('Hello');
            ";

        await ScriptLog.CollectAsync("my-script", async () =>
        {
            await engine.ExecuteAsync(vars, script);
        });

        A.CallTo(() => scriptLogStore.Log(appId, "my-script", A<ScriptLog>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_store_log_of_named_scope()
    {
        var vars = new ScriptVars
        {
            ["appId"] = appId,
        };

        const string script = @"
                console.log('Hello');
                console.log('World');
            ";

        await ScriptLog.CollectAsync("my-script", async () =>
        {
            await sut.ExecuteAsync(vars, script);
        });

        A.CallTo(() => scriptLogStore.Log(appId, "my-script", A<ScriptLog>.That.Matches(x => x.ToLines().Count == 2)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_store_log_if_script_failed()
    {
        var vars = new ScriptVars
        {
            ["appId"] = appId,
        };

        const string script = @"
                console.log('Hello');
                reject('Failed');
            ";

        await Assert.ThrowsAsync<ValidationException>(() => ScriptLog.CollectAsync("my-script", async () =>
        {
            await sut.ExecuteAsync(vars, script, new ScriptOptions { CanReject = true });
        }));

        A.CallTo(() => scriptLogStore.Log(appId, "my-script", A<ScriptLog>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_store_log_without_entries()
    {
        var vars = new ScriptVars
        {
            ["appId"] = appId,
        };

        const string script = @"
                42;
            ";

        await ScriptLog.CollectAsync("my-script", async () =>
        {
            await sut.ExecuteAsync(vars, script);
        });

        A.CallTo(() => scriptLogStore.Log(A<DomainId>._, A<string>._, A<ScriptLog>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public void Should_not_store_log_if_not_persisted()
    {
        var vars = new ScriptVars
        {
            ["appId"] = appId,
        };

        const string script = @"
                console.log('Hello');
            ";

        using (ScriptLog.Begin("my-script"))
        {
            sut.Execute(vars, script);
        }

        A.CallTo(() => scriptLogStore.Log(A<DomainId>._, A<string>._, A<ScriptLog>._))
            .MustNotHaveHappened();
    }
}
