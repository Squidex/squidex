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
    private readonly JintScriptEngine sut;

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

        public void ExtendAsync(ScriptExecutionContext context)
        {
            context.Engine.SetValue("setTimeout", new Delay((callback, time) =>
            {
                if (time == 0)
                {
                    context.Schedule((scheduler, ct) =>
                    {
                        scheduler.Run(callback);
                        return Task.CompletedTask;
                    });
                }
                else
                {
                    context.Schedule(async (scheduler, ct) =>
                    {
                        await Task.Delay(time, ct);
                        scheduler.Run(callback);
                    });
                }
            }));
        }
    }

    [Fact]
    public async Task ExecuteAsync_should_catch_script_syntax_errors()
    {
        const string script = @"
                invalid(()
            ";

        await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync([], script));
    }

    [Fact]
    public async Task ExecuteAsync_should_catch_script_runtime_errors()
    {
        const string script = @"
                throw 'Error';
            ";

        await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync([], script));
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

        await Assert.ThrowsAsync<ValidationException>(() => sut.TransformAsync([], script));
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

        await Assert.ThrowsAsync<ValidationException>(() => sut.TransformAsync(vars, script, contentOptions));
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

    [Fact]
    public void Should_not_throw_if_reading_undeclared_identifier()
    {
        // Reading an unknown name does not throw, it returns an internal Jint marker string. That is odd,
        // but it is what scripts have always seen here, see NullPropagation.TryUnresolvableReference.
        const string script = @"
                String(unknownName) + '|' + (typeof unknownName);
            ";

        var actual = sut.Execute(new ScriptVars(), script);

        Assert.Equal(JsonValue.Create("[[Unresolvable]]|undefined"), actual);
    }

    [Fact]
    public void Should_null_propagate_over_nullish_property_base()
    {
        var vars = new ScriptVars
        {
            ["value"] = 13,
        };

        const string script = @"
                ctx.unknown.deeper.evenDeeper === undefined;
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.True, actual);
    }

    [Fact]
    public void Should_chain_call_over_nullish_property_base()
    {
        var vars = new ScriptVars
        {
            ["value"] = 13,
        };

        const string script = @"
                ctx.unknown.deeper.someMethod() === undefined;
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.True, actual);
    }

    [Fact]
    public void Should_return_base_if_calling_non_callable_member()
    {
        var vars = new ScriptVars
        {
            ["value"] = "squidex",
        };

        const string script = @"
                ctx.value.notAFunction();
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create("squidex"), actual);
    }

    [Fact]
    public void Should_not_change_normal_member_reads_and_calls()
    {
        var vars = new ScriptVars
        {
            ["value"] = JsonValue.Create(new JsonObject().Add("name", JsonValue.Create("squidex"))),
        };

        const string script = @"
                ctx.value.name.toUpperCase();
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create("SQUIDEX"), actual);
    }

    [Fact]
    public void Should_convert_enum_to_name()
    {
        var vars = new ScriptVars
        {
            ["value"] = ScriptScope.ContentScript,
        };

        const string script = @"
                value;
            ";

        var actual = sut.Execute(vars, script);

        Assert.Equal(JsonValue.Create("ContentScript"), actual);
    }

    [Fact]
    public void Should_convert_flags_enum_to_names()
    {
        var vars = new ScriptVars
        {
            ["value"] = ScriptScope.ContentScript | ScriptScope.Transform,
        };

        const string script = @"
                value;
            ";

        var actual = sut.Execute(vars, script);

        Assert.Equal(JsonValue.Create("ContentScript, Transform"), actual);
    }

    [Fact]
    public void Should_convert_enum_member_of_wrapped_object_to_name()
    {
        var vars = new ScriptVars
        {
            ["value"] = new { scope = ScriptScope.Transform },
        };

        const string script = @"
                value.scope;
            ";

        var actual = sut.Execute(vars, script);

        Assert.Equal(JsonValue.Create("Transform"), actual);
    }

    [Fact]
    public void Should_project_json_object_with_source_key_order()
    {
        var vars = new ScriptVars
        {
            ["value"] = CreateJson(),
        };

        const string script = @"
                Object.keys(ctx.value).join(',');
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create("name,count,nested,items"), actual);
    }

    [Fact]
    public void Should_stringify_projected_json_object()
    {
        var vars = new ScriptVars
        {
            ["value"] = CreateJson(),
        };

        const string script = @"
                JSON.stringify(ctx.value);
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(
            JsonValue.Create("{\"name\":\"squidex\",\"count\":3,\"nested\":{\"flag\":true},\"items\":[1,2]}"),
            actual);
    }

    [Fact]
    public void Should_enumerate_projected_json_object()
    {
        var vars = new ScriptVars
        {
            ["value"] = CreateJson(),
        };

        const string script = @"
                var actual = [];
                for (var key in ctx.value) {
                    actual.push(key + '=' + (typeof ctx.value[key]));
                }
                actual.join(',');
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(
            JsonValue.Create("name=string,count=number,nested=object,items=object"),
            actual);
    }

    [Fact]
    public void Should_allow_mutation_of_projected_json_object()
    {
        var vars = new ScriptVars
        {
            ["value"] = CreateJson(),
        };

        const string script = @"
                ctx.value.name = 'changed';
                ctx.value.added = 42;
                delete ctx.value.count;
                ctx.value;
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        var expected =
            JsonValue.Create(
                new JsonObject()
                    .Add("name", JsonValue.Create("changed"))
                    .Add("nested", JsonValue.Create(new JsonObject().Add("flag", JsonValue.True)))
                    .Add("items", JsonValue.Create(new JsonArray().Add(JsonValue.Create(1)).Add(JsonValue.Create(2))))
                    .Add("added", JsonValue.Create(42)));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_round_trip_projected_json_object()
    {
        var json = CreateJson();

        var vars = new ScriptVars
        {
            ["value"] = json,
        };

        const string script = @"
                ctx.value;
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(json, actual);
    }

    [Fact]
    public void Should_enumerate_context_keys()
    {
        const string script = @"
                Object.keys(ctx).join(',');
            ";

        var actual = sut.Execute(CreateVars(), script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create("number,text,json,user"), actual);
    }

    [Fact]
    public void Should_answer_in_operator_for_context_keys()
    {
        const string script = @"
                ('json' in ctx) + ',' + ('unknown' in ctx);
            ";

        var actual = sut.Execute(CreateVars(), script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create("true,false"), actual);
    }

    [Fact]
    public void Should_report_types_of_context_values()
    {
        const string script = @"
                var actual = [];
                for (var key in ctx) {
                    actual.push(key + '=' + (typeof ctx[key]));
                }
                actual.join(',');
            ";

        var actual = sut.Execute(CreateVars(), script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create("number=number,text=string,json=object,user=object"), actual);
    }

    [Fact]
    public void Should_read_context_values()
    {
        const string script = @"
                ctx.number + '|' + ctx.text + '|' + ctx.json.name + '|' + ctx.user.id;
            ";

        var actual = sut.Execute(CreateVars(), script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create("13|hello|squidex|user1"), actual);
    }

    [Fact]
    public void Should_write_context_value_through_to_vars()
    {
        var vars = CreateVars();

        const string script = @"
                ctx.number = ctx.number * 2;
            ";

        sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(26.0, vars["number"]);
    }

    [Fact]
    public void Should_delete_context_value()
    {
        var vars = CreateVars();

        const string script = @"
                delete ctx.text;
                Object.keys(ctx).join(',');
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create("number,json,user"), actual);
    }

    [Fact]
    public void Should_not_map_unread_variable()
    {
        var principal = new CountingPrincipal();

        var vars = new ScriptVars
        {
            ["number"] = 13,
            ["user"] = principal,
        };

        const string script = @"
                number + 1;
            ";

        var actual = sut.Execute(vars, script);

        Assert.Equal(JsonValue.Create(14), actual);
        Assert.Equal(0, principal.Reads);
    }

    [Fact]
    public void Should_see_unread_variable_in_enumeration()
    {
        var principal = new CountingPrincipal();

        var vars = new ScriptVars
        {
            ["user"] = principal,
        };

        const string script = @"
                ('user' in globalThis) + ',' + (Object.getOwnPropertyNames(globalThis).indexOf('user') >= 0);
            ";

        var actual = sut.Execute(vars, script);

        Assert.Equal(JsonValue.Create("true,true"), actual);
        Assert.Equal(0, principal.Reads);
    }

    [Fact]
    public void Should_map_variable_on_first_read()
    {
        var principal = new CountingPrincipal();

        var vars = new ScriptVars
        {
            ["user"] = principal,
        };

        const string script = @"
                user.id;
            ";

        var actual = sut.Execute(vars, script);

        Assert.Equal(JsonValue.Create("user1"), actual);
        Assert.True(principal.Reads > 0);
    }

    [Fact]
    public void Should_not_map_unread_context_variable()
    {
        var principal = new CountingPrincipal();

        var vars = new ScriptVars
        {
            ["number"] = 13,
            ["user"] = principal,
        };

        const string script = @"
                ctx.number + 1;
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create(14), actual);
        Assert.Equal(0, principal.Reads);
    }

    [Fact]
    public void Should_see_unread_context_variable_in_enumeration()
    {
        var principal = new CountingPrincipal();

        var vars = new ScriptVars
        {
            ["number"] = 13,
            ["user"] = principal,
        };

        const string script = @"
                Object.keys(ctx).join(',') + '|' + ('user' in ctx);
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create("number,user|true"), actual);
        Assert.Equal(0, principal.Reads);
    }

    [Fact]
    public void Should_map_context_variable_on_first_read()
    {
        var principal = new CountingPrincipal();

        var vars = new ScriptVars
        {
            ["user"] = principal,
        };

        const string script = @"
                ctx.user.id;
            ";

        var actual = sut.Execute(vars, script, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create("user1"), actual);
        Assert.True(principal.Reads > 0);
    }

    private sealed class CountingPrincipal : ClaimsPrincipal
    {
        public int Reads { get; private set; }

        public CountingPrincipal()
            : base(new ClaimsIdentity(
            [
                new Claim(OpenIdClaims.Subject, "user1"),
                new Claim(OpenIdClaims.Name, "user"),
            ], "Squidex"))
        {
        }

        public override IEnumerable<Claim> Claims
        {
            get
            {
                Reads++;

                return base.Claims;
            }
        }
    }

    private static ScriptVars CreateVars()
    {
        return new ScriptVars
        {
            ["number"] = 13,
            ["text"] = "hello",
            ["json"] = CreateJson(),
            ["user"] = new ClaimsPrincipal(
                new ClaimsIdentity(
                [
                    new Claim(OpenIdClaims.Subject, "user1"),
                    new Claim(OpenIdClaims.Name, "user"),
                ], "Squidex")),
        };
    }

    private static JsonValue CreateJson()
    {
        return JsonValue.Create(
            new JsonObject()
                .Add("name", JsonValue.Create("squidex"))
                .Add("count", JsonValue.Create(3))
                .Add("nested", JsonValue.Create(new JsonObject().Add("flag", JsonValue.True)))
                .Add("items", JsonValue.Create(new JsonArray().Add(JsonValue.Create(1)).Add(JsonValue.Create(2)))));
    }
}
