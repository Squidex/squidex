// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using Jint.Runtime;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Scripting.Extensions;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.Scripting
{
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

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            sut = new JintScriptEngine(cache, extensions)
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            };
        }

        [Fact]
        public void Should_convert_html_to_text()
        {
            const string script = @"
                return html2Text(value);
            ";

            var vars = new ScriptVars
            {
                ["value"] = "<script>Invalid</script><STYLE>Invalid</STYLE><p>Hello World</p>"
            };

            var result = sut.Execute(vars, script).ToString();

            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void Should_convert_markdown_to_text()
        {
            const string script = @"
                return markdown2Text(value);
            ";

            var vars = new ScriptVars
            {
                ["value"] = "## Hello World"
            };

            var result = sut.Execute(vars, script).ToString();

            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void Should_count_words()
        {
            const string script = @"
                return wordCount(value);
            ";

            var vars = new ScriptVars
            {
                ["value"] = "Hello, World"
            };

            var result = ((JsonNumber)sut.Execute(vars, script)).Value;

            Assert.Equal(2, result);
        }

        [Fact]
        public void Should_count_characters()
        {
            const string script = @"
                return characterCount(value);
            ";

            var vars = new ScriptVars
            {
                ["value"] = "Hello, World"
            };

            var result = ((JsonNumber)sut.Execute(vars, script)).Value;

            Assert.Equal(10, result);
        }

        [Fact]
        public void Should_camel_case_value()
        {
            const string script = @"
                return toCamelCase(value);
            ";

            var vars = new ScriptVars
            {
                ["value"] = "Hello World"
            };

            var result = sut.Execute(vars, script).ToString();

            Assert.Equal("helloWorld", result);
        }

        [Fact]
        public void Should_pascal_case_value()
        {
            const string script = @"
                return toPascalCase(value);
            ";

            var vars = new ScriptVars
            {
                ["value"] = "Hello World"
            };

            var result = sut.Execute(vars, script).ToString();

            Assert.Equal("HelloWorld", result);
        }

        [Fact]
        public void Should_slugify_value()
        {
            const string script = @"
                return slugify(value);
            ";

            var vars = new ScriptVars
            {
                ["value"] = "4 Häuser"
            };

            var result = sut.Execute(vars, script).ToString();

            Assert.Equal("4-haeuser", result);
        }

        [Fact]
        public void Should_slugify_value_with_single_char()
        {
            const string script = @"
                return slugify(value, true);
            ";

            var vars = new ScriptVars
            {
                ["value"] = "4 Häuser"
            };

            var result = sut.Execute(vars, script).ToString();

            Assert.Equal("4-hauser", result);
        }

        [Fact]
        public void Should_compute_sha256_hash()
        {
            const string script = @"
                return sha256(value);
            ";

            var vars = new ScriptVars
            {
                ["value"] = "HelloWorld"
            };

            var result = sut.Execute(vars, script).ToString();

            Assert.Equal("HelloWorld".ToSha256(), result);
        }

        [Fact]
        public void Should_compute_md5_hash()
        {
            const string script = @"
                return md5(value);
            ";

            var vars = new ScriptVars
            {
                ["value"] = "HelloWorld"
            };

            var result = sut.Execute(vars, script).ToString();

            Assert.Equal("HelloWorld".ToMD5(), result);
        }

        [Fact]
        public async Task Should_throw_validation_exception_if_calling_reject()
        {
            const string script = @"
                reject()
            ";

            var options = new ScriptOptions
            {
                CanReject = true
            };

            var vars = new ScriptVars();

            var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync(vars, script, options));

            Assert.NotEmpty(ex.Errors);
        }

        [Fact]
        public async Task Should_throw_validation_exception_if_calling_reject_with_message()
        {
            const string script = @"
                reject('Not valid')
            ";

            var options = new ScriptOptions
            {
                CanReject = true
            };

            var vars = new ScriptVars();

            var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync(vars, script, options));

            Assert.Equal("Not valid", ex.Errors.Single().Message);
        }

        [Fact]
        public async Task Should_throw_security_exception_if_calling_reject()
        {
            const string script = @"
                disallow()
            ";

            var options = new ScriptOptions
            {
                CanDisallow = true
            };

            var vars = new ScriptVars();

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

            var vars = new ScriptVars();

            var ex = await Assert.ThrowsAsync<DomainForbiddenException>(() => sut.ExecuteAsync(vars, script, options));

            Assert.Equal("Operation not allowed", ex.Message);
        }

        [Fact]
        public async Task Should_throw_exception_if_getJson_url_is_null()
        {
            const string script = @"
                getJSON(null, function(result) {
                    complete(result);
                });
            ";

            var vars = new ScriptVars();

            await Assert.ThrowsAsync<JavaScriptException>(() => sut.ExecuteAsync(vars, script));
        }

        [Fact]
        public async Task Should_throw_exception_if_getJson_callback_is_null()
        {
            const string script = @"
                var url = 'http://squidex.io';

                getJSON(url, null);
            ";

            var vars = new ScriptVars();

            await Assert.ThrowsAsync<JavaScriptException>(() => sut.ExecuteAsync(vars, script));
        }

        [Fact]
        public async Task Should_make_getJson_request()
        {
            var httpHandler = SetupRequest();

            const string script = @"
                var url = 'http://squidex.io';

                getJSON(url, function(result) {
                    complete(result);
                });
            ";

            var vars = new ScriptVars();

            var result = await sut.ExecuteAsync(vars, script);

            httpHandler.ShouldBeMethod(HttpMethod.Get);
            httpHandler.ShouldBeUrl("http://squidex.io/");

            var expectedResult = JsonValue.Object().Add("key", 42);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task Should_make_getJson_request_with_headers()
        {
            var httpHandler = SetupRequest();

            const string script = @"
                var headers = {
                    'X-Header1': 1,
                    'X-Header2': '2'                
                };

                var url = 'http://squidex.io';

                getJSON(url, function(result) {
                    complete(result);
                }, headers);
            ";

            var vars = new ScriptVars();

            var result = await sut.ExecuteAsync(vars, script);

            httpHandler.ShouldBeMethod(HttpMethod.Get);
            httpHandler.ShouldBeUrl("http://squidex.io/");
            httpHandler.ShouldBeHeader("X-Header1", "1");
            httpHandler.ShouldBeHeader("X-Header2", "2");

            var expectedResult = JsonValue.Object().Add("key", 42);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task Should_make_deleteJson_request()
        {
            var httpHandler = SetupRequest();

            const string script = @"
                var url = 'http://squidex.io';

                deleteJSON(url, function(result) {
                    complete(result);
                });
            ";

            var vars = new ScriptVars();

            var result = await sut.ExecuteAsync(vars, script);

            httpHandler.ShouldBeMethod(HttpMethod.Delete);
            httpHandler.ShouldBeUrl("http://squidex.io/");

            var expectedResult = JsonValue.Object().Add("key", 42);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task Should_make_patchJson_request()
        {
            var httpHandler = SetupRequest();

            const string script = @"
                var url = 'http://squidex.io';

                var body = { key: 42 };

                patchJSON(url, body, function(result) {
                    complete(result);
                });
            ";

            var vars = new ScriptVars();

            var result = await sut.ExecuteAsync(vars, script);

            httpHandler.ShouldBeMethod(HttpMethod.Patch);
            httpHandler.ShouldBeUrl("http://squidex.io/");
            httpHandler.ShouldBeBody("{\"key\":42}", "text/json");

            var expectedResult = JsonValue.Object().Add("key", 42);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task Should_make_postJson_request()
        {
            var httpHandler = SetupRequest();

            const string script = @"
                var url = 'http://squidex.io';

                var body = { key: 42 };

                postJSON(url, body, function(result) {
                    complete(result);
                });
            ";

            var vars = new ScriptVars();

            var result = await sut.ExecuteAsync(vars, script);

            httpHandler.ShouldBeMethod(HttpMethod.Post);
            httpHandler.ShouldBeUrl("http://squidex.io/");
            httpHandler.ShouldBeBody("{\"key\":42}", "text/json");

            var expectedResult = JsonValue.Object().Add("key", 42);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task Should_make_putJson_request()
        {
            var httpHandler = SetupRequest();

            const string script = @"
                var url = 'http://squidex.io';

                var body = { key: 42 };

                putJSON(url, body, function(result) {
                    complete(result);
                });
            ";

            var vars = new ScriptVars();

            var result = await sut.ExecuteAsync(vars, script);

            httpHandler.ShouldBeMethod(HttpMethod.Put);
            httpHandler.ShouldBeUrl("http://squidex.io/");
            httpHandler.ShouldBeBody("{\"key\":42}", "text/json");

            var expectedResult = JsonValue.Object().Add("key", 42);

            Assert.Equal(expectedResult, result);
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
}
