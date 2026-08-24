// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent;

public class ContentConversionRemovalTests
{
    private static readonly DomainId ComponentId = DomainId.NewGuid();
    private readonly ResolvedComponents components;
    private readonly Schema schema;

    public ContentConversionRemovalTests()
    {
        schema =
            new Schema { Name = "my-schema" }
                .AddComponents(1, "components", Partitioning.Invariant)
                .AddArray(2, "array", Partitioning.Invariant, a => a
                    .AddString(21, "value"));

        components = new ResolvedComponents(new Dictionary<DomainId, Schema>
        {
            [ComponentId] =
                new Schema { Name = "my-component" }
                    .AddString(1, "value", Partitioning.Invariant),
        });
    }

    [Theory]
    [InlineData("abc", "abc")]
    [InlineData("-bc", "bc")]
    [InlineData("a-c", "ac")]
    [InlineData("ab-", "ab")]
    [InlineData("--c", "c")]
    [InlineData("-b-", "b")]
    [InlineData("a--", "a")]
    [InlineData("---", "")]
    public void Should_remove_array_items_that_are_not_objects(string source, string expected)
    {
        var items = source.Select(x => x == '-' ? JsonValue.Create(0) : Item(x));

        Assert.Equal(expected, Convert("array", items));
    }

    [Theory]
    [InlineData("abc", "abc")]
    [InlineData("-bc", "bc")]
    [InlineData("a-c", "ac")]
    [InlineData("ab-", "ab")]
    [InlineData("--c", "c")]
    [InlineData("-b-", "b")]
    [InlineData("a--", "a")]
    [InlineData("---", "")]
    public void Should_remove_components_of_unknown_schema(string source, string expected)
    {
        var items = source.Select(x => x == '-' ? ComponentOf(x, DomainId.NewGuid()) : ComponentOf(x, ComponentId));

        Assert.Equal(expected, Convert("components", items));
    }

    [Theory]
    [InlineData("abc", "abc")]
    [InlineData("-bc", "bc")]
    [InlineData("a-c", "ac")]
    [InlineData("ab-", "ab")]
    [InlineData("--c", "c")]
    [InlineData("-b-", "b")]
    [InlineData("a--", "a")]
    [InlineData("---", "")]
    public void Should_remove_components_without_discriminator(string source, string expected)
    {
        var items = source.Select(x => x == '-' ? Item(x) : ComponentOf(x, ComponentId));

        Assert.Equal(expected, Convert("components", items));
    }

    private string Convert(string field, IEnumerable<JsonValue> items)
    {
        var source =
            new ContentData()
                .AddField(field,
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(items.ToArray())));

        var converted = new ContentConverter(components, schema).Convert(source);

        if (!converted.TryGetValue(field, out var data) || data?["iv"].Value is not JsonArray array)
        {
            return string.Empty;
        }

        return string.Concat(array.Select(x => ((JsonObject)x.Value!)["value"].ToString()));
    }

    private static JsonValue Item(char value)
    {
        return JsonValue.Object().Add("value", value.ToString());
    }

    private static JsonValue ComponentOf(char value, DomainId schemaId)
    {
        return JsonValue.Object().Add("value", value.ToString()).Add(Component.Discriminator, schemaId);
    }
}
