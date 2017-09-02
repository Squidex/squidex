// ==========================================================================
//  JurassicScriptEngineTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public class JurassicScriptEngineTests
    {
        private readonly JurassicScriptEngine scriptEngine =
            new JurassicScriptEngine(
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

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
        public void Should_catch_script_runtime_errors_on_transform()
        {
            Assert.Throws<ValidationException>(() => scriptEngine.ExecuteAndTransform(new ScriptContext(), "throw 'Error';", "update", true));
        }

        [Fact]
        public void Should_return_original_content_when_script_failed()
        {
            var content = new NamedContentData();
            var context = new ScriptContext { Data = content };

            var result = scriptEngine.ExecuteAndTransform(context, "x => x", "update");

            Assert.Same(content, result);
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
        public void Should_returning_empty_content_when_replacing_with_invalid_content()
        {
            var content =
                new NamedContentData()
                    .AddField("number0",
                        new ContentFieldData()
                            .AddValue("iv", 1))
                    .AddField("number1",
                        new ContentFieldData()
                            .AddValue("iv", 1));

            var context = new ScriptContext { Data = content };

            var result = scriptEngine.ExecuteAndTransform(context, @"replace({ test: 1 });", "update");

            Assert.Equal(new NamedContentData(), result);
        }

        [Fact]
        public void Should_transform_content_and_return()
        {
            var content =
                new NamedContentData()
                    .AddField("number0",
                        new ContentFieldData()
                            .AddValue("iv", 1))
                    .AddField("number1",
                        new ContentFieldData()
                            .AddValue("iv", 1));
            var expected =
                new NamedContentData()
                    .AddField("number1",
                        new ContentFieldData()
                            .AddValue("iv", 2))
                    .AddField("number2",
                        new ContentFieldData()
                            .AddValue("iv", 10));

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
