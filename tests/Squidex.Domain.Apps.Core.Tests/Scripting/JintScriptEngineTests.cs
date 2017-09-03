// ==========================================================================
//  JintScriptEngineTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public class JintScriptEngineTests
    {
        private readonly JintScriptEngine scriptEngine = new JintScriptEngine { Timeout = TimeSpan.FromSeconds(1) };

        [Fact]
        public void Should_throw_validation_exception_when_calling_reject()
        {
            Assert.Throws<ValidationException>(() => scriptEngine.Execute(new ScriptContext(), "reject()", "update"));
            Assert.Throws<ValidationException>(() => scriptEngine.Execute(new ScriptContext(), "reject('Not valid')", "update"));
        }

        [Fact]
        public void Should_throw_security_exception_when_calling_reject()
        {
            Assert.Throws<DomainForbiddenException>(() => scriptEngine.Execute(new ScriptContext(), "disallow()", "Update"));
            Assert.Throws<DomainForbiddenException>(() => scriptEngine.Execute(new ScriptContext(), "disallow('Not allowed')", "update"));
        }

        [Fact]
        public void Should_catch_script_syntax_errors()
        {
            Assert.Throws<ValidationException>(() => scriptEngine.Execute(new ScriptContext(), "x => x", "update"));
        }

        [Fact]
        public void Should_catch_script_runtime_errors()
        {
            Assert.Throws<ValidationException>(() => scriptEngine.Execute(new ScriptContext(), "throw 'Error';", "update"));
        }

        [Fact]
        public void Should_catch_script_runtime_errors_on_execute_and_transform()
        {
            Assert.Throws<ValidationException>(() => scriptEngine.ExecuteAndTransform(new ScriptContext(), "throw 'Error';", "update"));
        }

        [Fact]
        public void Should_return_original_content_when_transform_script_failed()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content };

            var result = scriptEngine.Transform(context, "x => x");

            Assert.Same(content, result);
        }

        [Fact]
        public void Should_throw_when_execute_and_transform_script_failed()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content };

            Assert.Throws<ValidationException>(() => scriptEngine.ExecuteAndTransform(context, "x => x", "update"));
        }

        [Fact]
        public void Should_return_original_content_when_content_is_not_replaced()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content };

            var result = scriptEngine.ExecuteAndTransform(context, "var x = 0;", "update");

            Assert.Same(content, result);
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

            var result = scriptEngine.Transform(context, @"
                var data = ctx.data;

                delete data.number0;

                data.number1.iv = data.number1.iv + 1;
                data.number2 = { 'iv': 10 };

                replace(data);");

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

            var result = scriptEngine.ExecuteAndTransform(context, @"
                var data = ctx.data;

                delete data.number0;

                data.number1.iv = data.number1.iv + 1;
                data.number2 = { 'iv': 10 };

                replace(data);", "update");

            Assert.Equal(expected, result);
        }
    }
}
