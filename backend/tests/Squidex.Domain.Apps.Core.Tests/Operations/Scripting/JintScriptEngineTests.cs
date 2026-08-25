// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Scripting.Extensions;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Validation;
using Engine = Jint.Engine;

namespace Squidex.Domain.Apps.Core.Operations.Scripting;

public class JintScriptEngineTests : IClassFixture<TranslationsFixture>
{
    private readonly ScriptOptions contentOptions = new ScriptOptions
    {
        CanReject = true,
        CanDisallow = true,
        AsContext = true,
    };

    private readonly IHttpClientFactory httpClientFactory = A.Fake<IHttpClientFactory>();
    private readonly IScriptEngine sut;

    public JintScriptEngineTests()
    {
        var extensions = new IJintExtension[]
        {
            new DateTimeJintExtension(),
            new HttpJintExtension(httpClientFactory),
            new StringJintExtension(),
            new StringWordsJintExtension(),
            new AsyncExtension(),
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{ \"key\": 42 }"),
        };

        var httpHandler = new MockupHttpHandler(httpResponse);

        A.CallTo(() => httpClientFactory.CreateClient(A<string>._))
            .Returns(new HttpClient(httpHandler));

        sut = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10),
                TimeoutPromise = TimeSpan.FromSeconds(8),
            }),
            extensions);
    }

    private sealed class AsyncExtension : IJintExtension
    {
        private delegate void Delay(Action callback, int time);

        public void ExtendAsync(Engine engine)
        {
            engine.SetValue("setTimeout", new Delay((callback, time) =>
            {
                engine.Schedule(async ct =>
                {
                    if (time > 0)
                    {
                        await Task.Delay(time, ct);
                    }

                    return true;
                },
                _ => callback());
            }));
        }
    }

    [Fact]
    public async Task ExecuteAsync_should_catch_script_syntax_errors()
    {
        const string script = @"
                invalid(()
            ";

        await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync([], script).AsTask());
    }

    [Fact]
    public async Task ExecuteAsync_should_catch_script_runtime_errors()
    {
        const string script = @"
                throw 'Error';
            ";

        await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync([], script).AsTask());
    }

    [Fact]
    public async Task TransformAsync_should_return_original_content_if_script_failed()
    {
        var content = new ContentData();

        var vars = new DataScriptVars
        {
            ["data"] = content,
        };

        const string script = @"
                x => x
            ";

        var actual = await sut.TransformAsync(vars, script, contentOptions);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task TransformAsync_should_transform_content()
    {
        var content =
            new ContentData()
                .AddField("number0",
                    new ContentFieldData()
                        .AddInvariant(1.0))
                .AddField("number1",
                    new ContentFieldData()
                        .AddInvariant(1.0));
        var expected =
            new ContentData()
                .AddField("number1",
                    new ContentFieldData()
                        .AddInvariant(2.0))
                .AddField("number2",
                    new ContentFieldData()
                        .AddInvariant(10.0));

        var vars = new DataScriptVars
        {
            ["data"] = content,
        };

        const string script = @"
                var data = ctx.data;

                delete data.number0;

                data.number1.iv = data.number1.iv + 1;
                data.number2 = { 'iv': 10 };

                replace(data);
            ";

        var actual = await sut.TransformAsync(vars, script, contentOptions);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TransformAsync_should_catch_javascript_error()
    {
        const string script = @"
                throw 'Error';
            ";

        await Assert.ThrowsAsync<ValidationException>(() => sut.TransformAsync([], script).AsTask());
    }

    [Fact]
    public async Task TransformAsync_should_throw_exception_if_script_failed()
    {
        var vars = new DataScriptVars
        {
            ["data"] = new ContentData(),
        };

        const string script = @"
                invalid(();
            ";

        await Assert.ThrowsAsync<ValidationException>(() => sut.TransformAsync(vars, script, contentOptions).AsTask());
    }

    [Fact]
    public async Task TransformAsync_should_return_original_content_if_not_replaced()
    {
        var vars = new DataScriptVars
        {
            ["data"] = new ContentData(),
        };

        const string script = @"
                var x = 0;
            ";

        var actual = await sut.TransformAsync(vars, script, contentOptions);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task TransformAsync_should_return_original_content_if_not_replaced_async()
    {
        var vars = new DataScriptVars
        {
            ["data"] = new ContentData(),
        };

        const string script = @"
                var x = 0;

                getJSON('http://mockup.squidex.io', function(actual) {
                    complete();
                });                    
            ";

        var actual = await sut.TransformAsync(vars, script, contentOptions);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task TransformAsync_should_transform_object()
    {
        var content = new ContentData();

        var expected =
            new ContentData()
                .AddField("operation",
                    new ContentFieldData()
                        .AddInvariant("MyOperation"));

        var vars = new DataScriptVars
        {
            ["data"] = content,
            ["dataOld"] = null,
            ["operation"] = "MyOperation",
        };

        const string script = @"
                var data = ctx.data;

                data.operation = { iv: ctx.operation };

                replace(data);
            ";

        var actual = await sut.TransformAsync(vars, script, contentOptions);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TransformAsync_should_transform_object_async()
    {
        var content = new ContentData();

        var expected =
            new ContentData()
                .AddField("operation",
                    new ContentFieldData()
                        .AddInvariant(42));

        var vars = new DataScriptVars
        {
            ["data"] = content,
            ["dataOld"] = null,
            ["operation"] = "MyOperation",
        };

        const string script = @"
                var data = ctx.data;

                getJSON('http://mockup.squidex.io', function(actual) {
                    data.operation = { iv: actual.key };

                    replace(data);
                });        

            ";

        var actual = await sut.TransformAsync(vars, script, contentOptions);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TransformAsync_should_not_ignore_transformation_if_async_not_set()
    {
        var vars = new DataScriptVars
        {
            ["data"] = new ContentData(),
            ["dataOld"] = null,
            ["operation"] = "MyOperation",
        };

        const string script = @"
                var data = ctx.data;

                getJSON('http://mockup.squidex.io', function(actual) {
                    data.operation = { iv: actual.key };

                    replace(data);
                });        

            ";

        var actual = await sut.TransformAsync(vars, script, contentOptions);

        Assert.NotEmpty(actual);
    }

    [Fact]
    public async Task TransformAsync_should_not_timeout_if_replace_never_called()
    {
        var vars = new DataScriptVars
        {
            ["data"] = new ContentData(),
            ["dataOld"] = null,
            ["operation"] = "MyOperation",
        };

        const string script = @"
                var data = ctx.data;

                getJSON('http://cloud.squidex.io/healthz', function(actual) {
                    data.operation = { iv: actual.key };
                });
            ";

        await sut.TransformAsync(vars, script, contentOptions);
    }

    [Fact]
    public async Task TransformAsync_should_transform_content_and_return_with_execute_transform()
    {
        var content =
            new ContentData()
                .AddField("number0",
                    new ContentFieldData()
                        .AddInvariant(1.0))
                .AddField("number1",
                    new ContentFieldData()
                        .AddInvariant(1.0));
        var expected =
            new ContentData()
                .AddField("number1",
                    new ContentFieldData()
                        .AddInvariant(2.0))
                .AddField("number2",
                    new ContentFieldData()
                        .AddInvariant(10.0));

        var vars = new DataScriptVars
        {
            ["data"] = content,
        };

        const string script = @"
                var data = ctx.data;

                delete data.number0;

                data.number1.iv = data.number1.iv + 1;
                data.number2 = { 'iv': 10 };

                replace(data);
            ";

        var actual = await sut.TransformAsync(vars, script, contentOptions);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TransformAsync_should_transform_content_with_old_content()
    {
        var content =
            new ContentData()
                .AddField("number0",
                    new ContentFieldData()
                        .AddInvariant(3.0));

        var oldContent =
            new ContentData()
                .AddField("number0",
                    new ContentFieldData()
                        .AddInvariant(5.0));

        var expected =
            new ContentData()
                .AddField("number0",
                    new ContentFieldData()
                        .AddInvariant(13.0));

        var userIdentity = new ClaimsIdentity();
        var userPrincipal = new ClaimsPrincipal(userIdentity);

        userIdentity.AddClaim(new Claim(OpenIdClaims.ClientId, "2"));

        var vars = new DataScriptVars
        {
            ["data"] = content,
            ["dataOld"] = oldContent,
            ["user"] = userPrincipal,
        };

        const string script = @"
                ctx.data.number0.iv = ctx.data.number0.iv + ctx.dataOld.number0.iv * parseInt(ctx.user.id, 10);

                replace(ctx.data);
            ";

        var actual = await sut.TransformAsync(vars, script, contentOptions);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Should_not_deadlock_if_callback_completes_synchronously()
    {
        // The callback runs on the thread of the evaluation, which holds the engine lock at that moment.
        const string script = @"
                function delay() {
                    return new Promise((resolve) => {
                        setTimeout(function () {
                            resolve(1);
                        }, 0);
                    });
                }

                (async () => {
                    let total = 0;

                    for (let i = 0; i < 10; i++) {
                        total += await delay();
                    }

                    complete(total);
                })()
            ";

        var actual = await sut.ExecuteAsync([], script);

        Assert.Equal(JsonValue.Create(10), actual);
    }

    [Fact]
    public async Task Should_throw_if_promise_is_rejected()
    {
        const string script = @"
                (async () => {
                    await new Promise((resolve, reject) => {
                        getJSON('http://mockup.squidex.io', function () {
                            reject('rejected');
                        });
                    });

                    complete(42);
                })()
            ";

        await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync([], script).AsTask());
    }

    [Fact]
    public async Task Should_not_throw_if_rejected_promise_is_handled()
    {
        const string script = @"
                (async () => {
                    try {
                        await new Promise((resolve, reject) => {
                            getJSON('http://mockup.squidex.io', function () {
                                reject('rejected');
                            });
                        });
                    } catch (e) {
                        complete(42);
                    }
                })()
            ";

        var actual = await sut.ExecuteAsync([], script);

        Assert.Equal(JsonValue.Create(42), actual);
    }

    [Fact]
    public void Should_not_leak_globals_between_executions()
    {
        var script = sut.CreateScript("var actual = typeof leaked; var leaked = 1; actual");

        for (var i = 1; i <= 3; i++)
        {
            Assert.Equal(JsonValue.Create("undefined"), script.Execute([]));
        }
    }

    [Fact]
    public void Should_leak_prototype_changes_between_executions_of_same_script()
    {
        // The snapshot restores the globals, but it does not undo changes to the prototypes. That is
        // acceptable because a script is never shared between apps, but it must not go unnoticed.
        var script = sut.CreateScript("var actual = ({}).polluted; Object.prototype.polluted = 'yes'; typeof actual");

        Assert.Equal(JsonValue.Create("undefined"), script.Execute([]));
        Assert.Equal(JsonValue.Create("string"), script.Execute([]));
    }

    [Fact]
    public void Should_not_leak_prototype_changes_to_other_scripts()
    {
        sut.CreateScript("Object.prototype.polluted = 'yes'; 1").Execute([]);

        var script = sut.CreateScript("typeof ({}).polluted");

        Assert.Equal(JsonValue.Create("undefined"), script.Execute([]));
    }

    [Fact]
    public void Should_complete_sync_script()
    {
        var script = sut.CreateScript("complete(42); 1");

        Assert.Equal(JsonValue.Create(42), script.Execute([]));
    }

    [Fact]
    public void Should_transform_with_sync_script()
    {
        var vars = new DataScriptVars
        {
            ["data"] = new ContentData(),
        };

        var script = sut.CreateScript("ctx.data.number = { iv: 42 }; replace()", contentOptions);

        var actual = script.Transform(vars);

        Assert.Equal(JsonValue.Create(42), actual["number"]!["iv"]);
    }

    [Fact]
    public async Task Should_cancel_async_script()
    {
        using var cts = new CancellationTokenSource();

        var script = sut.CreateAsyncScript("while (true) { }");

        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<Exception>(() => script.ExecuteAsync([], cts.Token).AsTask());
    }

    [Fact]
    public async Task Should_run_same_script_in_parallel()
    {
        var script = sut.CreateAsyncScript("const factor = 2; value.i * factor");

        var tasks = Enumerable.Range(1, 20).Select(async i =>
        {
            var vars = new ScriptVars
            {
                ["value"] = new { i },
            };

            return (i, actual: await script.ExecuteAsync(vars));
        });

        foreach (var (i, actual) in await Task.WhenAll(tasks))
        {
            Assert.Equal(JsonValue.Create(i * 2), actual);
        }
    }

    [Fact]
    public async Task Should_reuse_script_with_callbacks()
    {
        var script = sut.CreateAsyncScript(@"
                const factor = value.i;

                getJSON('http://mockup.squidex.io', function(actual) {
                    complete(actual.key * factor);
                });
            ");

        for (var i = 1; i <= 3; i++)
        {
            var vars = new ScriptVars
            {
                ["value"] = new { i },
            };

            Assert.Equal(JsonValue.Create(42 * i), await script.ExecuteAsync(vars));
        }
    }

    [Fact]
    public void Should_reuse_script_with_global_declarations()
    {
        var script = sut.CreateScript("const factor = 2; value.i * factor");

        for (var i = 1; i <= 3; i++)
        {
            var vars = new ScriptVars
            {
                ["value"] = new { i },
            };

            Assert.Equal(JsonValue.Create(i * 2), script.Execute(vars));
        }
    }

    [Fact]
    public async Task Should_reuse_async_script_with_global_declarations()
    {
        var script = sut.CreateAsyncScript("const factor = 2; value.i * factor");

        for (var i = 1; i <= 3; i++)
        {
            var vars = new ScriptVars
            {
                ["value"] = new { i },
            };

            Assert.Equal(JsonValue.Create(i * 2), await script.ExecuteAsync(vars));
        }
    }

    [Fact]
    public void Evaluate_should_return_true_if_expression_match()
    {
        var vars = new ScriptVars
        {
            ["value"] = new { i = 2 },
        };

        const string script = @"
                value.i == 2
            ";

        var actual = ((IScriptEngine)sut).Evaluate(vars, script);

        Assert.True(actual);
    }

    [Fact]
    public void Evaluate_should_return_true_if_status_match()
    {
        var vars = new ScriptVars
        {
            ["value"] = new { status = Status.Published },
        };

        const string script = @"
                value.status == 'Published'
            ";

        var actual = ((IScriptEngine)sut).Evaluate(vars, script);

        Assert.True(actual);
    }

    [Fact]
    public void Evaluate_should_return_false_if_expression_match()
    {
        var vars = new ScriptVars
        {
            ["value"] = new { i = 2 },
        };

        const string script = @"
                value.i == 3
            ";

        var actual = ((IScriptEngine)sut).Evaluate(vars, script);

        Assert.False(actual);
    }

    [Fact]
    public void Evaluate_should_return_false_if_script_is_invalid()
    {
        var vars = new ScriptVars
        {
            ["value"] = new { i = 2 },
        };

        const string script = @"
                function();
            ";

        var actual = ((IScriptEngine)sut).Evaluate(vars, script);

        Assert.False(actual);
    }

    [Fact]
    public void Should_handle_domain_id_as_string()
    {
        var id = DomainId.NewGuid();

        var vars = new ScriptVars
        {
            ["value"] = id,
        };

        const string script = @"
                value;
            ";

        var actual = sut.Execute(vars, script);

        Assert.Equal(id.ToString(), actual.ToString());
    }

    [Fact]
    public void Should_allow_null_vars()
    {
        var vars = new ScriptVars
        {
            ["value"] = null,
        };

        const string script = @"
                value;
            ";

        var actual = sut.Execute(vars, script);

        Assert.Equal(JsonValue.Null, actual);
    }

    [Fact]
    public void Should_not_allow_to_overwrite_initial_var()
    {
        var vars = new ScriptVars().SetInitial(13, "value");

        const string script = @"
                ctx.value = ctx.value * 2;
            ";

        sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(13, vars["value"]);
    }

    [Fact]
    public void Should_share_vars_between_executions()
    {
        var vars = new ScriptVars
        {
            ["value"] = 13,
        };

        const string script1 = @"
                ctx.shared = ctx.value * 2;
            ";

        const string script2 = @"
                ctx.shared + 2;
            ";

        sut.Execute(vars, script1, new ScriptOptions { AsContext = true });

        var actual = sut.Execute(vars, script2, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create(28), actual);
    }

    [Fact]
    public void Should_share_complex_vars_between_executions()
    {
        var vars = new ScriptVars
        {
            ["value"] = 13,
        };

        const string script1 = @"
                ctx.obj = { number: ctx.value * 2 };
            ";

        const string script2 = @"
                ctx.obj.number + 2;
            ";

        sut.Execute(vars, script1, new ScriptOptions { AsContext = true });

        var actual = sut.Execute(vars, script2, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create(28), actual);
    }

    [Fact]
    public async Task Should_share_vars_between_execution_for_transform()
    {
        var vars = new DataScriptVars
        {
            ["value"] = 13,
        };

        const string script1 = @"
                ctx.shared = { number: ctx.value * 2 };
            ";

        const string script2 = @"
                ctx.data.test = { iv: ctx.shared.number + 2 };
                replace();
            ";

        await sut.ExecuteAsync(vars, script1, new ScriptOptions { AsContext = true });

        var vars2 = new DataScriptVars
        {
            ["data"] = new ContentData(),
        };

        vars2.CopyFrom(vars);

        var actual = await sut.TransformAsync(vars2, script2, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create(28), actual["test"]!["iv"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task Should_not_run_callbacks_in_parallel(int waitTime)
    {
        var vars = new DataScriptVars
        {
            ["value"] = 13,
        };

        var script = @$"
            var x = ctx.value;
            for (var i = 0; i < 100; i++) {{
                setTimeout(function () {{
                    x++;
                    ctx.shared = x;
                }}, {waitTime});
            }}
        ";

        await sut.ExecuteAsync(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(113.0, vars["shared"]);
    }

    [Trait("Category", "Dependencies")]
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task Should_not_run_nested_callbacks_in_parallel(int waitTime)
    {
        var vars = new DataScriptVars
        {
            ["value"] = 13,
        };

        var script = @$"
            var x = ctx.value;
            for (var i = 0; i < 100; i++) {{
                setTimeout(function () {{
                    setTimeout(function () {{
                        x++;
                        ctx.shared = x;
                    }}, {waitTime});
                }}, {waitTime});
            }}
        ";

        await sut.ExecuteAsync(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(113.0, vars["shared"]);
    }

    [Fact]
    public async Task Should_set_metadata()
    {
        var vars = new DataScriptVars
        {
            ["metadata"] = new AssetMetadata(),
        };

        var script = @$"
            ctx.metadata['pixelWidth'] = 100;
        ";

        await sut.ExecuteAsync(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(100, ((AssetMetadata)vars["metadata"]!).GetInt32(KnownMetadataKeys.PixelWidth));
    }

    [Fact]
    public async Task Should_run_with_promises()
    {
        var vars = new DataScriptVars();

        const string script = @"
                function asyncMethod() {
                    return new Promise((resolve, reject) => {
                        getJSON('http://cloud.squidex.io/healthz', (data) => {
                            resolve(data);
                        }, {}, true);
                    });
                }

                (async () => {
                    await asyncMethod();
                    complete(42)
                })()
            ";

        var result = await sut.ExecuteAsync(vars, script, contentOptions);

        Assert.Equal(42.0, result.Value);
    }
}
