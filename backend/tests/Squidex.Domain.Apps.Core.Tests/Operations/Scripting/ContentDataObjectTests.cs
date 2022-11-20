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

namespace Squidex.Domain.Apps.Core.Operations.Scripting;

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

        const string script = @"
                data.number = { iv: 1 }
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
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

        const string script = @"
                data.number.iv = 1
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
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

        const string script = @"
                Object.defineProperty(data, 'number', { value: { iv: 1 } })
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_throw_exception_if_assigning_non_object_as_field()
    {
        var original = new ContentData();

        const string script = @"
                data.number = 1
            ";

        Assert.Throws<JavaScriptException>(() => ExecuteScript(original, script));
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

        const string script = @"
                delete data.number
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
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

        const string script = @"
                data.string.iv = data.string.iv + 'new'
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
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

        const string script = @"
                data.number.iv = data.number.iv + 2
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
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

        const string script = @"
                data.boolean.iv = !data.boolean.iv
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
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

        const string script = @"
                data.number.iv = [data.number.iv[0], data.number.iv[1] + 2, 5]
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_update_data_if_setting_field_value_with_object()
    {
        var original =
            new ContentData()
                .AddField("geo",
                    new ContentFieldData()
                        .AddInvariant(new JsonObject().Add("lat", 1.0)));

        var expected =
            new ContentData()
                .AddField("geo",
                    new ContentFieldData()
                        .AddInvariant(new JsonObject().Add("lat", 1.0).Add("lon", 4.0)));

        const string script = @"
                data.geo.iv = { lat: data.geo.iv.lat, lon: data.geo.iv.lat + 3 }
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_update_data_if_changing_nested_field()
    {
        var original =
            new ContentData()
                .AddField("geo",
                    new ContentFieldData()
                        .AddInvariant(new JsonObject().Add("lat", 1.0)));

        var expected =
            new ContentData()
                .AddField("geo",
                    new ContentFieldData()
                        .AddInvariant(new JsonObject().Add("lat", 2.0).Add("lon", 4.0)));

        const string script = @"
                var nested = data.geo.iv;
                nested.lat = 2;
                nested.lon = 4;
                data.geo.iv = nested
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_not_update_data_if_not_changing_nested_field()
    {
        var original =
            new ContentData()
                .AddField("geo",
                    new ContentFieldData()
                        .AddInvariant(new JsonObject().Add("lat", 2.0).Add("lon", 4.0)));

        const string script = @"
                var nested = data.geo.iv;
                nested.lat = 2;
                nested.lon = 4;
                data.geo.iv = nested
            ";

        var actual = ExecuteScript(original, script);

        Assert.Same(original, actual);
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

        const string script = @"
                Object.defineProperty(data.number, 'iv', { value: 1 })
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
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

        const string script = @"
                delete data.string.iv
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
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

        const string script = @"
                var actual = [];
                for (var x in data) {
                    var field = data[x];

                    for (var y in field) {
                        actual.push(field[y]);
                    }
                }
                actual;
            ";

        var actual = engine.Evaluate(script).ToObject();

        Assert.Equal(new[] { "1", "2", "3", "4" }, actual);
    }

    [Fact]
    public void Should_not_throw_exceptions_if_changing_arrays()
    {
        var original =
            new ContentData()
                .AddField("obj",
                    new ContentFieldData()
                        .AddInvariant(new JsonArray()));

        const string script = @"
                data.obj.iv[0] = 1
            ";

        ExecuteScript(original, script);
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("Number.POSITIVE_INFINITY")]
    [InlineData("Number.NEGATIVE_INFINITY")]
    public void Should_not_throw_exceptions_if_invalid_numbers(string input)
    {
        var original =
            new ContentData()
                .AddField("number",
                    new ContentFieldData()
                        .AddInvariant(new JsonArray()));

        var expected =
            new ContentData()
                .AddField("number",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Zero));

        string script = $@"
                data.number.iv = {input};
            ";

        var actual = ExecuteScript(original, script);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_null_propagate_unknown_fields()
    {
        const string script = @"
                data.string.iv = 'hello'
            ";

        ExecuteScript(new ContentData(), script);
    }

    private static ContentData ExecuteScript(ContentData original, string script)
    {
        var engine = new Engine(o => o.Strict());

        var value = new ContentDataObject(engine, original);

        engine.SetValue("data", value);
        engine.Execute(script);

        value.TryUpdate(out var actual);

        return actual;
    }
}
