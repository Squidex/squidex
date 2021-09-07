// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.Scripting
{
    public class JintScriptEngineTests
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

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            sut = new JintScriptEngine(cache, extensions)
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            };
        }

        [Fact]
        public async Task ExecuteAsync_should_catch_script_syntax_errors()
        {
            const string script = @"
                invalid()
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
            var context = new ScriptVars { ["data"] = content };

            const string script = @"
                x => x
            ";

            var result = await sut.TransformAsync(context, script, contentOptions);

            Assert.Empty(result);
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

            var context = new ScriptVars { ["data"] = content };

            const string script = @"
                var data = ctx.data;

                delete data.number0;

                data.number1.iv = data.number1.iv + 1;
                data.number2 = { 'iv': 10 };

                replace(data);
            ";

            var result = await sut.TransformAsync(context, script, contentOptions);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task TransformAsync_should_catch_javascript_error()
        {
            const string script = @"
                throw 'Error';
            ";

            await Assert.ThrowsAsync<ValidationException>(() => sut.TransformAsync(new ScriptVars(), script));
        }

        [Fact]
        public async Task TransformAsync_should_throw_exception_if_script_failed()
        {
            var content = new ContentData();
            var context = new ScriptVars { ["data"] = content };

            const string script = @"
                invalid();
            ";

            await Assert.ThrowsAsync<ValidationException>(() => sut.TransformAsync(context, script, contentOptions));
        }

        [Fact]
        public async Task TransformAsync_should_return_original_content_if_not_replaced()
        {
            var content = new ContentData();
            var context = new ScriptVars { ["data"] = content };

            const string script = @"
                var x = 0;
            ";

            var result = await sut.TransformAsync(context, script, contentOptions);

            Assert.Empty(result);
        }

        [Fact]
        public async Task TransformAsync_should_return_original_content_if_not_replaced_async()
        {
            var content = new ContentData();
            var context = new ScriptVars { ["data"] = content };

            const string script = @"
                async = true;

                var x = 0;

                getJSON('http://squidex.io', function(result) {
                    complete();
                });                    
            ";

            var result = await sut.TransformAsync(context, script, contentOptions);

            Assert.Empty(result);
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

            var context = new ScriptVars
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

            var result = await sut.TransformAsync(context, script, contentOptions);

            Assert.Equal(expected, result);
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

            var context = new ScriptVars
            {
                ["data"] = content,
                ["dataOld"] = null,
                ["operation"] = "MyOperation"
            };

            const string script = @"
                async = true;

                var data = ctx.data;

                getJSON('http://squidex.io', function(result) {
                    data.operation = { iv: result.key };

                    replace(data);
                });        

            ";

            var result = await sut.TransformAsync(context, script, contentOptions);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task TransformAsync_should_not_ignore_transformation_if_async_not_set()
        {
            var content = new ContentData();
            var context = new ScriptVars
            {
                ["data"] = content,
                ["dataOld"] = null,
                ["operation"] = "MyOperation"
            };

            const string script = @"
                var data = ctx.data;

                getJSON('http://squidex.io', function(result) {
                    data.operation = { iv: result.key };

                    replace(data);
                });        

            ";

            var result = await sut.TransformAsync(context, script, contentOptions);

            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task TransformAsync_should_timeout_if_replace_never_called()
        {
            var content = new ContentData();
            var context = new ScriptVars
            {
                ["data"] = content,
                ["dataOld"] = null,
                ["operation"] = "MyOperation"
            };

            const string script = @"
                async = true;

                var data = ctx.data;

                getJSON('http://squidex.io', function(result) {
                    data.operation = { iv: result.key };
                });
            ";

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sut.TransformAsync(context, script, contentOptions));
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

            var context = new ScriptVars { ["data"] = content };

            const string script = @"
                var data = ctx.data;

                delete data.number0;

                data.number1.iv = data.number1.iv + 1;
                data.number2 = { 'iv': 10 };

                replace(data);
            ";

            var result = await sut.TransformAsync(context, script, contentOptions);

            Assert.Equal(expected, result);
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

            var context = new ScriptVars
            {
                ["data"] = content,
                ["dataOld"] = oldContent,
                ["user"] = userPrincipal
            };

            const string script = @"
                ctx.data.number0.iv = ctx.data.number0.iv + ctx.dataOld.number0.iv * parseInt(ctx.user.id, 10);

                replace(ctx.data);
            ";

            var result = await sut.TransformAsync(context, script, contentOptions);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Evaluate_should_return_true_if_expression_match()
        {
            const string script = @"
                value.i == 2
            ";

            var context = new ScriptVars
            {
                ["value"] = new { i = 2 }
            };

            var result = ((IScriptEngine)sut).Evaluate(context, script);

            Assert.True(result);
        }

        [Fact]
        public void Evaluate_should_return_true_if_status_match()
        {
            const string script = @"
                value.status == 'Published'
            ";

            var context = new ScriptVars
            {
                ["value"] = new { status = Status.Published }
            };

            var result = ((IScriptEngine)sut).Evaluate(context, script);

            Assert.True(result);
        }

        [Fact]
        public void Evaluate_should_return_false_if_expression_match()
        {
            const string script = @"
                value.i == 3
            ";

            var context = new ScriptVars
            {
                ["value"] = new { i = 2 }
            };

            var result = ((IScriptEngine)sut).Evaluate(context, script);

            Assert.False(result);
        }

        [Fact]
        public void Evaluate_should_return_false_if_script_is_invalid()
        {
            const string script = @"
                function();
            ";

            var context = new ScriptVars
            {
                ["value"] = new { i = 2 }
            };

            var result = ((IScriptEngine)sut).Evaluate(context, script);

            Assert.False(result);
        }

        [Fact]
        public void Should_handle_domain_id_as_string()
        {
            var id = DomainId.NewGuid();

            const string script = @"
                return value;
            ";

            var context = new ScriptVars
            {
                ["value"] = id
            };

            var result = sut.Execute(context, script);

            Assert.Equal(id.ToString(), result.ToString());
        }
    }
}
