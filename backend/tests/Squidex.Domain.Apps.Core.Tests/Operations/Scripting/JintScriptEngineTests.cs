// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.Scripting
{
    public class JintScriptEngineTests
    {
        private readonly IHttpClientFactory httpClientFactory = A.Fake<IHttpClientFactory>();
        private readonly JintScriptEngine sut;

        public JintScriptEngineTests()
        {
            sut = new JintScriptEngine(httpClientFactory)
            {
                Timeout = TimeSpan.FromSeconds(1)
            };
        }

        [Fact]
        public void Should_throw_validation_exception_when_calling_reject()
        {
            const string script = @"
                reject()
            ";

            var ex = Assert.Throws<ValidationException>(() => sut.Execute(new ScriptContext(), script));

            Assert.Empty(ex.Errors);
        }

        [Fact]
        public void Should_throw_validation_exception_when_calling_reject_with_message()
        {
            const string script = @"
                reject('Not valid')
            ";

            var ex = Assert.Throws<ValidationException>(() => sut.Execute(new ScriptContext(), script));

            Assert.Equal("Not valid", ex.Errors.Single().Message);
        }

        [Fact]
        public void Should_throw_security_exception_when_calling_reject()
        {
            const string script = @"
                disallow()
            ";

            var ex = Assert.Throws<DomainForbiddenException>(() => sut.Execute(new ScriptContext(), script));

            Assert.Equal("Not allowed", ex.Message);
        }

        [Fact]
        public void Should_throw_security_exception_when_calling_reject_with_message()
        {
            const string script = @"
                disallow('Operation not allowed')
            ";

            var ex = Assert.Throws<DomainForbiddenException>(() => sut.Execute(new ScriptContext(), script));

            Assert.Equal("Operation not allowed", ex.Message);
        }

        [Fact]
        public void Should_catch_script_syntax_errors()
        {
            const string script = @"
                invalid()
            ";

            Assert.Throws<ValidationException>(() => sut.Execute(new ScriptContext(), script));
        }

        [Fact]
        public void Should_catch_script_runtime_errors()
        {
            const string script = @"
                throw 'Error';
            ";

            Assert.Throws<ValidationException>(() => sut.Execute(new ScriptContext(), script));
        }

        [Fact]
        public void Should_catch_script_runtime_errors_on_execute_and_transform()
        {
            const string script = @"
                throw 'Error';
            ";

            Assert.Throws<ValidationException>(() => sut.ExecuteAndTransform(new ScriptContext(), script));
        }

        [Fact]
        public void Should_return_original_content_when_transform_script_failed()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content };

            const string script = @"
                x => x
            ";

            var result = sut.Transform(context, script);

            Assert.Same(content, result);
        }

        [Fact]
        public void Should_throw_when_execute_and_transform_script_failed()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content };

            const string script = @"
                invalid();
            ";

            Assert.Throws<ValidationException>(() => sut.ExecuteAndTransform(context, script));
        }

        [Fact]
        public void Should_return_original_content_when_content_is_not_replaced()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content };

            const string script = @"
                var x = 0;
            ";

            var result = sut.ExecuteAndTransform(context, script);

            Assert.Same(content, result);
        }

        [Fact]
        public void Should_fetch_operation_name()
        {
            var content = new NamedContentData();

            var expected =
                new NamedContentData()
                    .AddField("operation",
                        new ContentFieldData()
                            .AddValue("iv", "MyOperation"));

            var context = new ScriptContext { Data = content, Operation = "MyOperation" };

            const string script = @"
                var data = ctx.data;

                data.operation = { iv: ctx.operation };

                replace(data);
            ";

            var result = sut.ExecuteAndTransform(context, script);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_transform_content_and_return_with_transform()
        {
            var content =
                new NamedContentData()
                    .AddField("number0",
                        new ContentFieldData()
                            .AddValue("iv", 1.0))
                    .AddField("number1",
                        new ContentFieldData()
                            .AddValue("iv", 1.0));
            var expected =
                new NamedContentData()
                    .AddField("number1",
                        new ContentFieldData()
                            .AddValue("iv", 2.0))
                    .AddField("number2",
                        new ContentFieldData()
                            .AddValue("iv", 10.0));

            var context = new ScriptContext { Data = content };

            const string script = @"
                var data = ctx.data;

                delete data.number0;

                data.number1.iv = data.number1.iv + 1;
                data.number2 = { 'iv': 10 };

                replace(data);
            ";

            var result = sut.Transform(context, script);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_slugify_value()
        {
            var content =
                new NamedContentData()
                    .AddField("title",
                        new ContentFieldData()
                            .AddValue("iv", "4 Häuser"));

            var expected =
                new NamedContentData()
                    .AddField("title",
                        new ContentFieldData()
                            .AddValue("iv", "4 Häuser"))
                    .AddField("slug",
                        new ContentFieldData()
                            .AddValue("iv", "4-haeuser"));

            var context = new ScriptContext { Data = content };

            const string script = @"
                var data = ctx.data;

                data.slug = { iv: slugify(data.title.iv) };

                replace(data);
            ";

            var result = sut.Transform(context, script);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_slugify_value_with_single_char()
        {
            var content =
                new NamedContentData()
                    .AddField("title",
                        new ContentFieldData()
                            .AddValue("iv", "4 Häuser"));

            var expected =
                new NamedContentData()
                    .AddField("title",
                        new ContentFieldData()
                            .AddValue("iv", "4 Häuser"))
                    .AddField("slug",
                        new ContentFieldData()
                            .AddValue("iv", "4-hauser"));

            var context = new ScriptContext { Data = content };

            const string script = @"
                var data = ctx.data;

                data.slug = { iv: slugify(data.title.iv, true) };

                replace(data);
            ";

            var result = sut.Transform(context, script);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_transform_content_and_return_with_execute_transform()
        {
            var content =
                new NamedContentData()
                    .AddField("number0",
                        new ContentFieldData()
                            .AddValue("iv", 1.0))
                    .AddField("number1",
                        new ContentFieldData()
                            .AddValue("iv", 1.0));
            var expected =
                new NamedContentData()
                    .AddField("number1",
                        new ContentFieldData()
                            .AddValue("iv", 2.0))
                    .AddField("number2",
                        new ContentFieldData()
                            .AddValue("iv", 10.0));

            var context = new ScriptContext { Data = content };

            const string script = @"
                var data = ctx.data;

                delete data.number0;

                data.number1.iv = data.number1.iv + 1;
                data.number2 = { 'iv': 10 };

                replace(data);
            ";

            var result = sut.ExecuteAndTransform(context, script);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_transform_content_with_old_content()
        {
            var content =
                new NamedContentData()
                    .AddField("number0",
                        new ContentFieldData()
                            .AddValue("iv", 3.0));

            var oldContent =
                new NamedContentData()
                    .AddField("number0",
                        new ContentFieldData()
                            .AddValue("iv", 5.0));

            var expected =
                new NamedContentData()
                    .AddField("number0",
                        new ContentFieldData()
                            .AddValue("iv", 13.0));

            var userIdentity = new ClaimsIdentity();
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            userIdentity.AddClaim(new Claim(OpenIdClaims.ClientId, "2"));

            var context = new ScriptContext { Data = content, DataOld = oldContent, User = userPrincipal };

            const string script = @"
                ctx.data.number0.iv = ctx.data.number0.iv + ctx.oldData.number0.iv * parseInt(ctx.user.id, 10);

                replace(ctx.data);
            ";

            var result = sut.ExecuteAndTransform(context, script);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_evaluate_to_true_when_expression_match()
        {
            const string script = @"
                value.i == 2
            ";

            var result = sut.Evaluate("value", new { i = 2 }, script);

            Assert.True(result);
        }

        [Fact]
        public void Should_evaluate_to_true_when_status_match()
        {
            const string script = @"
                value.status == 'Published'
            ";

            var result = sut.Evaluate("value", new { status = Status.Published }, script);

            Assert.True(result);
        }

        [Fact]
        public void Should_evaluate_to_false_when_expression_match()
        {
            const string script = @"
                value.i == 3
            ";

            var result = sut.Evaluate("value", new { i = 2 }, script);

            Assert.False(result);
        }

        [Fact]
        public void Should_evaluate_to_false_when_script_is_invalid()
        {
            const string script = @"
                function();
            ";

            var result = sut.Evaluate("value", new { i = 2 }, script);

            Assert.False(result);
        }

        [Fact]
        public async Task Should_make_json_request()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"key\": 42 }")
            };

            var httpHandler = new MockupHander(httpResponse);

            A.CallTo(() => httpClientFactory.CreateClient(A<string>._))
                .Returns(new HttpClient(httpHandler));

            const string script = @"
                getJSON('http://squidex.io', function(result) {
                    complete(result);
                });
            ";

            var result = await sut.GetAsync(new ScriptContext(), script);

            httpHandler.ShouldBeMethod(HttpMethod.Get);
            httpHandler.ShouldBeUrl("http://squidex.io/");

            var expectedResult = JsonValue.Object().Add("key", 42);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task Should_make_json_request_with_headers()
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"key\": 42 }")
            };

            var httpHandler = new MockupHander(httpResponse);

            A.CallTo(() => httpClientFactory.CreateClient(A<string>._))
                .Returns(new HttpClient(httpHandler));

            const string script = @"
                var headers = {
                    'X-Header1': 1,
                    'X-Header2': '2'                
                };

                getJSON('http://squidex.io', function(result) {
                    complete(result);
                }, headers);
            ";

            var result = await sut.GetAsync(new ScriptContext(), script);

            httpHandler.ShouldBeMethod(HttpMethod.Get);
            httpHandler.ShouldBeUrl("http://squidex.io/");
            httpHandler.ShouldBeHeader("X-Header1", "1");
            httpHandler.ShouldBeHeader("X-Header2", "2");

            var expectedResult = JsonValue.Object().Add("key", 42);

            Assert.Equal(expectedResult, result);
        }

        private sealed class MockupHander : HttpMessageHandler
        {
            private readonly HttpResponseMessage response;
            private HttpRequestMessage madeRequest;

            public void ShouldBeMethod(HttpMethod method)
            {
                Assert.Equal(method, madeRequest.Method);
            }

            public void ShouldBeUrl(string url)
            {
                Assert.Equal(url, madeRequest.RequestUri.ToString());
            }

            public void ShouldBeHeader(string key, string value)
            {
                Assert.Equal(value, madeRequest.Headers.GetValues(key).FirstOrDefault());
            }

            public MockupHander(HttpResponseMessage response)
            {
                this.response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                madeRequest = request;

                return Task.FromResult(response);
            }
        }
    }
}
