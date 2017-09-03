// ==========================================================================
//  ContentDataObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Jint;
using Jint.Runtime;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;
using Xunit;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ContentDataObjectTests
    {
        [Fact]
        public void Should_update_content_when_setting_field()
        {
            var original = new NamedContentData();

            var expected =
                new NamedContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddValue("iv", 1.0));

            var result = ExecuteScript(original, @"data.number = { iv: 1 }");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_content_when_deleting_field()
        {
            var original =
                new NamedContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddValue("iv", 1.0));

            var expected = new NamedContentData();

            var result = ExecuteScript(original, @"delete data.number");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_content_when_setting_field_value_with_string()
        {
            var original =
                new NamedContentData()
                    .AddField("string",
                        new ContentFieldData()
                            .AddValue("iv", "1"));

            var expected =
                new NamedContentData()
                    .AddField("string",
                        new ContentFieldData()
                            .AddValue("iv", "1new"));

            var result = ExecuteScript(original, @"data.string.iv = data.string.iv + 'new'");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_content_when_setting_field_value_with_number()
        {
            var original =
                new NamedContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddValue("iv", 1.0));

            var expected =
                new NamedContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddValue("iv", 3.0));

            var result = ExecuteScript(original, @"data.number.iv = data.number.iv + 2");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_content_when_setting_field_value_with_array()
        {
            var original =
                new NamedContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddValue("iv", new JArray(1.0, 2.0)));

            var expected =
                new NamedContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddValue("iv", new JArray(1.0, 4.0, 5.0)));

            var result = ExecuteScript(original, @"data.number.iv = [data.number.iv[0], data.number.iv[1] + 2, 5]");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_content_when_setting_field_value_with_object()
        {
            var original =
                new NamedContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddValue("iv", new JObject(new JProperty("lat", 1.0))));

            var expected =
                new NamedContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddValue("iv", new JObject(new JProperty("lat", 1.0), new JProperty("lon", 4.0))));

            var result = ExecuteScript(original, @"data.number.iv = { lat: data.number.iv.lat, lon: data.number.iv.lat + 3 }");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_content_when_setting_field_value_with_boolean()
        {
            var original =
                new NamedContentData()
                    .AddField("boolean",
                        new ContentFieldData()
                            .AddValue("iv", false));

            var expected =
                new NamedContentData()
                    .AddField("boolean",
                        new ContentFieldData()
                            .AddValue("iv", true));

            var result = ExecuteScript(original, @"data.boolean.iv = !data.boolean.iv");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_content_when_deleting_field_value()
        {
            var original =
                new NamedContentData()
                    .AddField("string",
                        new ContentFieldData()
                            .AddValue("iv", "hello"));

            var expected =
                new NamedContentData()
                    .AddField("string",
                        new ContentFieldData());

            var result = ExecuteScript(original, @"delete data.string.iv");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_throw_exceptions_when_changing_objects()
        {
            var original =
                new NamedContentData()
                    .AddField("obj",
                        new ContentFieldData()
                            .AddValue("iv", new JObject(new JProperty("readonly", 1))));

            Assert.Throws<JavaScriptException>(() => ExecuteScript(original, "data.obj.iv.invalid = 1"));
            Assert.Throws<JavaScriptException>(() => ExecuteScript(original, "data.obj.iv.readonly = 2"));
        }

        [Fact]
        public void Should_not_throw_exceptions_when_changing_arrays()
        {
            var original =
                new NamedContentData()
                    .AddField("obj",
                        new ContentFieldData()
                            .AddValue("iv", new JArray()));

            ExecuteScript(original, "data.obj.iv[0] = 1");
        }

        [Fact]
        public void Should_null_propagate_unknown_fields()
        {
            ExecuteScript(new NamedContentData(), @"data.string.iv = 'hello'");
        }

        private static NamedContentData ExecuteScript(NamedContentData original, string script)
        {
            var engine = new Engine(o => o.Strict());

            var value = new ContentDataObject(engine, original);

            engine.SetValue("data", value);
            engine.Execute(script);

            value.TryUpdate(out var result);

            return result;
        }
    }
}
