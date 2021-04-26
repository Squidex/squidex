// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Runtime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.Scripting
{
    public class ContentDataObjectTests
    {
        [Fact]
        public void Should_update_data_if_setting_field()
        {
            var original = new ContentData();

            var expected =
                new ContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddInvariant(1.0));

            var result = ExecuteScript(original, @"data.number = { iv: 1 }");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_data_if_setting_lazy_field()
        {
            var original = new ContentData();

            var expected =
                new ContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddInvariant(1.0));

            var result = ExecuteScript(original, @"data.number.iv = 1");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_data_defining_property_for_content()
        {
            var original = new ContentData();

            var expected =
                new ContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddInvariant(1.0));

            var result = ExecuteScript(original, "Object.defineProperty(data, 'number', { value: { iv: 1 } })");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_throw_exception_if_assigning_non_object_as_field()
        {
            var original = new ContentData();

            Assert.Throws<JavaScriptException>(() => ExecuteScript(original, @"data.number = 1"));
        }

        [Fact]
        public void Should_update_data_if_deleting_field()
        {
            var original =
                new ContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddInvariant(1.0));

            var expected = new ContentData();

            var result = ExecuteScript(original, @"delete data.number");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_data_if_setting_field_value_with_string()
        {
            var original =
                new ContentData()
                    .AddField("string",
                        new ContentFieldData()
                            .AddInvariant("1"));

            var expected =
                new ContentData()
                    .AddField("string",
                        new ContentFieldData()
                            .AddInvariant("1new"));

            var result = ExecuteScript(original, @"data.string.iv = data.string.iv + 'new'");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_data_if_setting_field_value_with_number()
        {
            var original =
                new ContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddInvariant(1.0));

            var expected =
                new ContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddInvariant(3.0));

            var result = ExecuteScript(original, @"data.number.iv = data.number.iv + 2");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_data_if_setting_field_value_with_boolean()
        {
            var original =
                new ContentData()
                    .AddField("boolean",
                        new ContentFieldData()
                            .AddInvariant(false));

            var expected =
                new ContentData()
                    .AddField("boolean",
                        new ContentFieldData()
                            .AddInvariant(true));

            var result = ExecuteScript(original, @"data.boolean.iv = !data.boolean.iv");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_data_if_setting_field_value_with_array()
        {
            var original =
                new ContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(1.0, 2.0)));

            var expected =
                new ContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(1.0, 4.0, 5.0)));

            var result = ExecuteScript(original, @"data.number.iv = [data.number.iv[0], data.number.iv[1] + 2, 5]");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_data_if_setting_field_value_with_object()
        {
            var original =
                new ContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Object().Add("lat", 1.0)));

            var expected =
                new ContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Object().Add("lat", 1.0).Add("lon", 4.0)));

            var result = ExecuteScript(original, @"data.number.iv = { lat: data.number.iv.lat, lon: data.number.iv.lat + 3 }");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_throw_if_defining_property_for_field()
        {
            var original =
                new ContentData()
                    .AddField("number",
                        new ContentFieldData());

            var expected =
                new ContentData()
                    .AddField("number",
                        new ContentFieldData()
                            .AddInvariant(1.0));

            var result = ExecuteScript(original, "Object.defineProperty(data.number, 'iv', { value: 1 })");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_update_data_if_deleting_field_value()
        {
            var original =
                new ContentData()
                    .AddField("string",
                        new ContentFieldData()
                            .AddInvariant("hello"));

            var expected =
                new ContentData()
                    .AddField("string",
                        new ContentFieldData());

            var result = ExecuteScript(original, @"delete data.string.iv");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_be_able_to_iterate_over_fields()
        {
            var content =
                new ContentData()
                    .AddField("f1",
                        new ContentFieldData()
                            .AddLocalized("v11", "1")
                            .AddLocalized("v12", "2"))
                    .AddField("f2",
                        new ContentFieldData()
                            .AddLocalized("v21", "3")
                            .AddLocalized("v22", "4"));

            var engine = new Engine();

            engine.SetValue("data", new ContentDataObject(engine, content));

            var result = engine.Execute(@"
                var result = [];
                for (var x in data) {
                    var field = data[x];

                    for (var y in field) {
                        result.push(field[y]);
                    }
                }
                result;").GetCompletionValue().ToObject();

            Assert.Equal(new[] { "1", "2", "3", "4" }, result);
        }

        [Fact]
        public void Should_throw_exceptions_if_changing_objects()
        {
            var original =
                new ContentData()
                    .AddField("obj",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Object().Add("readonly", 1)));

            Assert.Throws<JavaScriptException>(() => ExecuteScript(original, "data.obj.iv.invalid = 1"));
            Assert.Throws<JavaScriptException>(() => ExecuteScript(original, "data.obj.iv.readonly = 2"));
        }

        [Fact]
        public void Should_not_throw_exceptions_if_changing_arrays()
        {
            var original =
                new ContentData()
                    .AddField("obj",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array()));

            ExecuteScript(original, "data.obj.iv[0] = 1");
        }

        [Fact]
        public void Should_null_propagate_unknown_fields()
        {
            ExecuteScript(new ContentData(), @"data.string.iv = 'hello'");
        }

        private static ContentData ExecuteScript(ContentData original, string script)
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
