// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Scripting.Extensions;
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
            var extensions = new IScriptExtension[]
            {
                new DateTimeScriptExtension(),
                new HttpScriptExtension(httpClientFactory),
                new StringScriptExtension()
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"key\": 42 }")
            };

            var httpHandler = new MockupHttpHandler(httpResponse);

            A.CallTo(() => httpClientFactory.CreateClient(A<string>._))
                .Returns(new HttpClient(httpHandler));

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            sut = new JintScriptEngine(cache, extensions)
            {
                Timeout = TimeSpan.FromSeconds(1)
            };
        }

        [Fact]
        public async Task ExecuteAsync_should_catch_script_syntax_errors()
        {
            const string script = @"
                invalid()
            ";

            await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync(new ScriptContext(), script));
        }

        [Fact]
        public async Task ExecuteAsync_should_catch_script_runtime_errors()
        {
            const string script = @"
                throw 'Error';
            ";

            await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync(new ScriptContext(), script));
        }

        [Fact]
        public async Task TransformAsync_should_return_original_content_when_script_failed()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content };

            const string script = @"
                x => x
            ";

            var result = await sut.TransformAsync(context, script);

            Assert.Empty(result);
        }

        [Fact]
        public async Task TransformAsync_should_transform_content()
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

            var result = await sut.TransformAsync(context, script);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ExecuteAndTransformAsync_should_catch_javascript_error()
        {
            const string script = @"
                throw 'Error';
            ";

            await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAndTransformAsync(new ScriptContext(), script));
        }

        [Fact]
        public async Task ExecuteAndTransformAsync_should_throw_when_script_failed()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content };

            const string script = @"
                invalid();
            ";

            await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAndTransformAsync(context, script));
        }

        [Fact]
        public async Task ExecuteAndTransformAsync_should_return_original_content_when_not_replaced()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content };

            const string script = @"
                var x = 0;
            ";

            var result = await sut.ExecuteAndTransformAsync(context, script);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ExecuteAndTransformAsync_should_return_original_content_when_not_replaced_async()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content };

            const string script = @"
                async = true;

                var x = 0;

                getJSON('http://squidex.io', function(result) {
                    complete();
                });                    
            ";

            var result = await sut.ExecuteAndTransformAsync(context, script);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ExecuteAndTransformAsync_should_transform_object()
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

            var result = await sut.ExecuteAndTransformAsync(context, script);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ExecuteAndTransformAsync_should_transform_object_async()
        {
            var content = new NamedContentData();

            var expected =
                new NamedContentData()
                    .AddField("operation",
                        new ContentFieldData()
                            .AddValue("iv", 42));

            var context = new ScriptContext { Data = content, Operation = "MyOperation" };

            const string script = @"
                async = true;

                var data = ctx.data;

                getJSON('http://squidex.io', function(result) {
                    data.operation = { iv: result.key };

                    replace(data);
                });        

            ";

            var result = await sut.ExecuteAndTransformAsync(context, script);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ExecuteAndTransformAsync_should_ignore_transformation_when_async_not_set()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content, Operation = "MyOperation" };

            const string script = @"
                var data = ctx.data;

                getJSON('http://squidex.io', function(result) {
                    data.operation = { iv: result.key };

                    replace(data);
                });        

            ";

            var result = await sut.ExecuteAndTransformAsync(context, script);

            Assert.Empty( result);
        }

        [Fact]
        public async Task ExecuteAndTransformAsync_should_timeout_when_replace_never_called()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content, Operation = "MyOperation" };

            const string script = @"
                async = true;

                var data = ctx.data;

                getJSON('http://squidex.io', function(result) {
                    data.operation = { iv: result.key };
                });        

            ";

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sut.ExecuteAndTransformAsync(context, script));
        }

        [Fact]
        public async Task ExecuteAndTransformAsync_should_transform_content_and_return_with_execute_transform()
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

            var result = await sut.ExecuteAndTransformAsync(context, script);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ExecuteAndTransformAsync_should_transform_content_with_old_content()
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

            var result = await sut.ExecuteAndTransformAsync(context, script);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Evaluate_should_return_true_when_expression_match()
        {
            const string script = @"
                value.i == 2
            ";

            var context = new ScriptContext
            {
                ["value"] = new { i = 2 }
            };

            var result = sut.Evaluate(context, script);

            Assert.True(result);
        }

        [Fact]
        public void Evaluate_should_return_true_when_status_match()
        {
            const string script = @"
                value.status == 'Published'
            ";

            var context = new ScriptContext
            {
                ["value"] = new { status = Status.Published }
            };

            var result = sut.Evaluate(context, script);

            Assert.True(result);
        }

        [Fact]
        public void Evaluate_should_return_false_when_expression_match()
        {
            const string script = @"
                value.i == 3
            ";

            var context = new ScriptContext
            {
                ["value"] = new { i = 2 }
            };

            var result = sut.Evaluate(context, script);

            Assert.False(result);
        }

        [Fact]
        public void Evaluate_should_return_false_when_script_is_invalid()
        {
            const string script = @"
                function();
            ";

            var context = new ScriptContext
            {
                ["value"] = new { i = 2 }
            };

            var result = sut.Evaluate(context, script);

            Assert.False(result);
        }
    }
}
