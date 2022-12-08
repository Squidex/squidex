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
        AsContext = true
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
            new StringWordsJintExtension()
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{ \"key\": 42 }")
        };

        var httpHandler = new MockupHttpHandler(httpResponse);

        A.CallTo(() => httpClientFactory.CreateClient(A<string>._))
            .Returns(new HttpClient(httpHandler));

        sut = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            }),
            extensions);
    }

    [Fact]
    public async Task ExecuteAsync_should_catch_script_syntax_errors()
    {
        const string script = @"
                invalid(()
            ";

        await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync(new ScriptVars(), script));
    }

    [Fact]
    public async Task ExecuteAsync_should_catch_script_runtime_errors()
    {
        const string script = @"
                throw 'Error';
            ";

        await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync(new ScriptVars(), script));
    }

    [Fact]
    public async Task TransformAsync_should_return_original_content_if_script_failed()
    {
        var content = new ContentData();

        var vars = new DataScriptVars
        {
            ["data"] = content
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
            ["data"] = content
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

        await Assert.ThrowsAsync<ValidationException>(() => sut.TransformAsync(new DataScriptVars(), script));
    }

    [Fact]
    public async Task TransformAsync_should_throw_exception_if_script_failed()
    {
        var vars = new DataScriptVars
        {
            ["data"] = new ContentData()
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
            ["data"] = new ContentData()
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
            ["data"] = new ContentData()
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
            ["operation"] = "MyOperation"
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
            ["operation"] = "MyOperation"
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
            ["operation"] = "MyOperation"
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
            ["operation"] = "MyOperation"
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
            ["data"] = content
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
            ["user"] = userPrincipal
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
            ["value"] = new { i = 2 }
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
            ["value"] = new { status = Status.Published }
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
            ["value"] = new { i = 2 }
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
            ["value"] = new { i = 2 }
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
            ["value"] = id
        };

        const string script = @"
                return value;
            ";

        var actual = sut.Execute(vars, script);

        Assert.Equal(id.ToString(), actual.ToString());
    }

    [Fact]
    public void Should_share_vars_between_executions()
    {
        var vars = new ScriptVars
        {
            ["value"] = 13
        };

        const string script1 = @"
                ctx.value = ctx.value * 2;
            ";

        const string script2 = @"
                return ctx.value + 2;
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
            ["value"] = 13
        };

        const string script1 = @"
                ctx.obj = { number: ctx.value * 2 };
            ";

        const string script2 = @"
                return ctx.obj.number + 2;
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
            ["value"] = 13
        };

        const string script1 = @"
                ctx.obj = { number: ctx.value * 2 };
            ";

        const string script2 = @"
                ctx.data.test = { iv: ctx.obj.number + 2 };
                replace();
            ";

#pragma warning disable MA0042 // Do not use blocking calls in an async method
        sut.Execute(vars, script1, new ScriptOptions { AsContext = true });
#pragma warning restore MA0042 // Do not use blocking calls in an async method

        var vars2 = new DataScriptVars
        {
            ["data"] = new ContentData()
        };

        foreach (var (key, value) in vars)
        {
            if (!vars2.ContainsKey(key))
            {
                vars2[key] = value;
            }
        }

        var actual = await sut.TransformAsync(vars2, script2, new ScriptOptions { AsContext = true });

        Assert.Equal(JsonValue.Create(28), actual["test"]!["iv"]);
    }
}
