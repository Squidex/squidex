// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Jint.Runtime;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Scripting.Extensions;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.Operations.Scripting;

public class JintScriptEngineHelperTests : IClassFixture<TranslationsFixture>
{
    private readonly IHttpClientFactory httpClientFactory = A.Fake<IHttpClientFactory>();
    private readonly JintScriptEngine sut;

    public JintScriptEngineHelperTests()
    {
        var extensions = new IJintExtension[]
        {
            new DateTimeJintExtension(),
            new HttpJintExtension(httpClientFactory),
            new StringJintExtension(),
            new StringWordsJintExtension()
        };

        sut = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            }),
            extensions);
    }

    [Fact]
    public void Should_convert_html_to_text()
    {
        var vars = new ScriptVars
        {
            ["value"] = "<script>Invalid</script><STYLE>Invalid</STYLE><p>Hello World</p>"
        };

        const string script = @"
                return html2Text(value);
            ";

        var actual = sut.Execute(vars, script).AsString;

        Assert.Equal("Hello World", actual);
    }

    [Fact]
    public void Should_convert_markdown_to_text()
    {
        var vars = new ScriptVars
        {
            ["value"] = "## Hello World"
        };

        const string script = @"
                return markdown2Text(value);
            ";

        var actual = sut.Execute(vars, script).AsString;

        Assert.Equal("Hello World", actual);
    }

    [Fact]
    public void Should_count_words()
    {
        var vars = new ScriptVars
        {
            ["value"] = "Hello, World"
        };

        const string script = @"
                return wordCount(value);
            ";

        var actual = sut.Execute(vars, script).AsNumber;

        Assert.Equal(2, actual);
    }

    [Fact]
    public void Should_count_characters()
    {
        var vars = new ScriptVars
        {
            ["value"] = "Hello, World"
        };

        const string script = @"
                return characterCount(value);
            ";

        var actual = sut.Execute(vars, script).AsNumber;

        Assert.Equal(10, actual);
    }

    [Fact]
    public void Should_camel_case_value()
    {
        var vars = new ScriptVars
        {
            ["value"] = "Hello World"
        };

        const string script = @"
                return toCamelCase(value);
            ";

        var actual = sut.Execute(vars, script).AsString;

        Assert.Equal("helloWorld", actual);
    }

    [Fact]
    public void Should_pascal_case_value()
    {
        var vars = new ScriptVars
        {
            ["value"] = "Hello World"
        };

        const string script = @"
                return toPascalCase(value);
            ";

        var actual = sut.Execute(vars, script).AsString;

        Assert.Equal("HelloWorld", actual);
    }

    [Fact]
    public void Should_slugify_value()
    {
        var vars = new ScriptVars
        {
            ["value"] = "4 Häuser"
        };

        const string script = @"
                return slugify(value);
            ";

        var actual = sut.Execute(vars, script).AsString;

        Assert.Equal("4-haeuser", actual);
    }

    [Fact]
    public void Should_slugify_value_with_single_char()
    {
        var vars = new ScriptVars
        {
            ["value"] = "4 Häuser"
        };

        const string script = @"
                return slugify(value, true);
            ";

        var actual = sut.Execute(vars, script).AsString;

        Assert.Equal("4-hauser", actual);
    }

    [Fact]
    public void Should_compute_sha256_hash()
    {
        var vars = new ScriptVars
        {
            ["value"] = "HelloWorld"
        };

        const string script = @"
                return sha256(value);
            ";

        var actual = sut.Execute(vars, script).AsString;

        Assert.Equal("HelloWorld".ToSha256(), actual);
    }

    [Fact]
    public void Should_compute_sha512_hash()
    {
        var vars = new ScriptVars
        {
            ["value"] = "HelloWorld"
        };

        const string script = @"
                return sha512(value);
            ";

        var actual = sut.Execute(vars, script).AsString;

        Assert.Equal("HelloWorld".ToSha512(), actual);
    }

    [Fact]
    public void Should_compute_md5_hash()
    {
        var vars = new ScriptVars
        {
            ["value"] = "HelloWorld"
        };

        const string script = @"
                return md5(value);
            ";

        var actual = sut.Execute(vars, script).AsString;

        Assert.Equal("HelloWorld".ToMD5(), actual);
    }

    [Fact]
    public void Should_compute_guid()
    {
        var vars = new ScriptVars
        {
        };

        const string script = @"
                return guid();
            ";

        var actual = sut.Execute(vars, script).AsString;

        Assert.True(Guid.TryParse(actual, out _));
    }

    [Fact]
    public async Task Should_throw_validation_exception_if_calling_reject()
    {
        var options = new ScriptOptions
        {
            CanReject = true
        };

        var vars = new ScriptVars
        {
        };

        const string script = @"
                reject()
            ";

        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync(vars, script, options));

        Assert.NotEmpty(ex.Errors);
    }

    [Fact]
    public async Task Should_throw_validation_exception_if_calling_reject_with_message()
    {
        var options = new ScriptOptions
        {
            CanReject = true
        };

        var vars = new ScriptVars
        {
        };

        const string script = @"
                reject('Not valid')
            ";

        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync(vars, script, options));

        Assert.Equal("Not valid", ex.Errors.Single().Message);
    }

    [Fact]
    public async Task Should_throw_security_exception_if_calling_reject()
    {
        var options = new ScriptOptions
        {
            CanDisallow = true
        };

        var vars = new ScriptVars
        {
        };

        const string script = @"
                disallow()
            ";

        var ex = await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.ExecuteAsync(vars, script, options));

        Assert.Equal("Script has forbidden the operation.", ex.Message);
    }

    [Fact]
    public async Task Should_throw_security_exception_if_calling_reject_with_message()
    {
        const string script = @"
                disallow('Operation not allowed')
            ";

        var options = new ScriptOptions
        {
            CanDisallow = true
        };

        var vars = new ScriptVars
        {
        };

        var ex = await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.ExecuteAsync(vars, script, options));

        Assert.Equal("Operation not allowed", ex.Message);
    }

    [Fact]
    public async Task Should_throw_exception_if_getJson_url_is_null()
    {
        var vars = new ScriptVars
        {
        };

        const string script = @"
                getJSON(null, function(actual) {
                    complete(actual);
                });
            ";

        await Assert.ThrowsAsync<JavaScriptException>(() => sut.ExecuteAsync(vars, script));
    }

    [Fact]
    public async Task Should_throw_exception_if_getJson_callback_is_null()
    {
        var vars = new ScriptVars
        {
        };

        const string script = @"
                var url = 'http://squidex.io';

                getJSON(url, null);
            ";

        await Assert.ThrowsAsync<JavaScriptException>(() => sut.ExecuteAsync(vars, script));
    }

    [Fact]
    public async Task Should_make_getJson_request()
    {
        var httpHandler = SetupRequest();

        var vars = new ScriptVars
        {
        };

        const string script = @"
                var url = 'http://squidex.io';

                getJSON(url, function(actual) {
                    complete(actual);
                });
            ";

        var actual = await sut.ExecuteAsync(vars, script);

        httpHandler.ShouldBeMethod(HttpMethod.Get);
        httpHandler.ShouldBeUrl("http://squidex.io/");

        var expectedResult = new JsonObject().Add("key", 42);

        Assert.Equal(expectedResult, actual);
    }

    [Fact]
    public async Task Should_make_getJson_request_with_headers()
    {
        var httpHandler = SetupRequest();

        var vars = new ScriptVars
        {
        };

        const string script = @"
                var headers = {
                    'X-Header1': 1,
                    'X-Header2': '2'                
                };

                var url = 'http://squidex.io';

                getJSON(url, function(actual) {
                    complete(actual);
                }, headers);
            ";

        var actual = await sut.ExecuteAsync(vars, script);

        httpHandler.ShouldBeMethod(HttpMethod.Get);
        httpHandler.ShouldBeUrl("http://squidex.io/");
        httpHandler.ShouldBeHeader("X-Header1", "1");
        httpHandler.ShouldBeHeader("X-Header2", "2");

        var expectedResult = new JsonObject().Add("key", 42);

        Assert.Equal(expectedResult, actual);
    }

    [Fact]
    public async Task Should_make_deleteJson_request()
    {
        var httpHandler = SetupRequest();

        var vars = new ScriptVars
        {
        };

        const string script = @"
                var url = 'http://squidex.io';

                deleteJSON(url, function(actual) {
                    complete(actual);
                });
            ";

        var actual = await sut.ExecuteAsync(vars, script);

        httpHandler.ShouldBeMethod(HttpMethod.Delete);
        httpHandler.ShouldBeUrl("http://squidex.io/");

        var expectedResult = new JsonObject().Add("key", 42);

        Assert.Equal(expectedResult, actual);
    }

    [Fact]
    public async Task Should_make_patchJson_request()
    {
        var httpHandler = SetupRequest();

        var vars = new ScriptVars
        {
        };

        const string script = @"
                var url = 'http://squidex.io';

                var body = { key: 42 };

                patchJSON(url, body, function(actual) {
                    complete(actual);
                });
            ";

        var actual = await sut.ExecuteAsync(vars, script);

        httpHandler.ShouldBeMethod(HttpMethod.Patch);
        httpHandler.ShouldBeUrl("http://squidex.io/");
        httpHandler.ShouldBeBody("{\"key\":42}", "text/json");

        var expectedResult = new JsonObject().Add("key", 42);

        Assert.Equal(expectedResult, actual);
    }

    [Fact]
    public async Task Should_make_postJson_request()
    {
        var httpHandler = SetupRequest();

        var vars = new ScriptVars
        {
        };

        const string script = @"
                var url = 'http://squidex.io';

                var body = { key: 42 };

                postJSON(url, body, function(actual) {
                    complete(actual);
                });
            ";

        var actual = await sut.ExecuteAsync(vars, script);

        httpHandler.ShouldBeMethod(HttpMethod.Post);
        httpHandler.ShouldBeUrl("http://squidex.io/");
        httpHandler.ShouldBeBody("{\"key\":42}", "text/json");

        var expectedResult = new JsonObject().Add("key", 42);

        Assert.Equal(expectedResult, actual);
    }

    [Fact]
    public async Task Should_make_putJson_request()
    {
        var httpHandler = SetupRequest();

        var vars = new ScriptVars
        {
        };

        const string script = @"
                var url = 'http://squidex.io';

                var body = { key: 42 };

                putJSON(url, body, function(actual) {
                    complete(actual);
                });
            ";

        var actual = await sut.ExecuteAsync(vars, script);

        httpHandler.ShouldBeMethod(HttpMethod.Put);
        httpHandler.ShouldBeUrl("http://squidex.io/");
        httpHandler.ShouldBeBody("{\"key\":42}", "text/json");

        var expectedResult = new JsonObject().Add("key", 42);

        Assert.Equal(expectedResult, actual);
    }

    private MockupHttpHandler SetupRequest()
    {
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{ \"key\": 42 }")
        };

        var httpHandler = new MockupHttpHandler(httpResponse);

        A.CallTo(() => httpClientFactory.CreateClient(A<string>._))
            .Returns(new HttpClient(httpHandler));

        return httpHandler;
    }
}
