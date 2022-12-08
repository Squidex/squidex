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

public class ContentConversionTests
{
    private readonly Schema schema;
    private readonly ResolvedComponents components;

    public ContentConversionTests()
    {
        schema =
            new Schema("my-schema")
                .AddComponent(1, "component", Partitioning.Invariant)
                .AddComponents(2, "components", Partitioning.Invariant)
                .AddAssets(3, "assets1", Partitioning.Invariant)
                .AddAssets(4, "assets2", Partitioning.Invariant)
                .AddReferences(5, "references", Partitioning.Invariant)
                .AddArray(6, "array", Partitioning.Invariant, a => a
                    .AddAssets(31, "nested"));

        components = new ResolvedComponents(new Dictionary<DomainId, Schema>
        {
            [DomainId.Empty] = schema
        });
    }

    [Fact]
    public void Should_apply_value_conversion_on_all_levels()
    {
        var source =
            new ContentData()
                .AddField("references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(1, 2)))
                .AddField("assets1",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(1)))
                .AddField("array",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                new JsonObject()
                                    .Add("nested", JsonValue.Array(1, 2)))))
                .AddField("component",
                    new ContentFieldData()
                        .AddInvariant(
                            new JsonObject()
                                .Add("references",
                                    JsonValue.Array(1, 2))
                                .Add("assets1",
                                    JsonValue.Array(1))
                                .Add("array",
                                    JsonValue.Array(
                                        new JsonObject()
                                            .Add("nested", JsonValue.Array(1, 2))))
                                .Add(Component.Discriminator, DomainId.Empty)))
                .AddField("components",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                new JsonObject()
                                    .Add("references",
                                        JsonValue.Array(1, 2))
                                    .Add("assets1",
                                        JsonValue.Array(1))
                                    .Add("array",
                                        JsonValue.Array(
                                            new JsonObject()
                                                .Add("nested", JsonValue.Array(1, 2))))
                                    .Add(Component.Discriminator, DomainId.Empty))));

        var actual =
            new ContentConverter(components, schema)
                .Add(new ValueConverter())
                .Convert(source);

        var expected =
            new ContentData()
                .AddField("references",
                    new ContentFieldData())
                .AddField("assets1",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(1)))
                .AddField("array",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                new JsonObject())))
                .AddField("component",
                    new ContentFieldData()
                        .AddInvariant(
                            new JsonObject()
                                .Add("assets1",
                                    JsonValue.Array(1))
                                .Add("array",
                                    JsonValue.Array(
                                        new JsonObject()))
                                .Add(Component.Discriminator, DomainId.Empty)))
                .AddField("components",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                new JsonObject()
                                    .Add("assets1",
                                        JsonValue.Array(1))
                                    .Add("array",
                                        JsonValue.Array(
                                            new JsonObject()))
                                    .Add(Component.Discriminator, DomainId.Empty))));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_apply_item_conversion_on_all_levels()
    {
        var source =
            new ContentData()
                .AddField("references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(1, 2)))
                .AddField("assets1",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(1)))
                .AddField("array",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                new JsonObject()
                                    .Add("nested", JsonValue.Array(1, 2)))))
                .AddField("component",
                    new ContentFieldData()
                        .AddInvariant(
                            new JsonObject()
                                .Add("references",
                                    JsonValue.Array(1, 2))
                                .Add("assets1",
                                    JsonValue.Array(1))
                                .Add("array",
                                    JsonValue.Array(
                                        new JsonObject()
                                            .Add("nested", JsonValue.Array(1, 2))))
                                .Add(Component.Discriminator, DomainId.Empty)))
                .AddField("components",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                new JsonObject()
                                    .Add("references",
                                        JsonValue.Array(1, 2))
                                    .Add("assets1",
                                        JsonValue.Array(1))
                                    .Add("array",
                                        JsonValue.Array(
                                            new JsonObject()
                                                .Add("nested", JsonValue.Array(1, 2))))
                                    .Add(Component.Discriminator, DomainId.Empty))));

        var actual =
            new ContentConverter(components, schema)
                .Add(new ItemConverter())
                .Convert(source);

        var expected =
            new ContentData()
                .AddField("references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(1, 2)))
                .AddField("assets1",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(1)))
                .AddField("array",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                new JsonObject()
                                    .Add("extraField", 42)
                                    .Add("nested", JsonValue.Array(1, 2)))))
                .AddField("component",
                    new ContentFieldData()
                        .AddInvariant(
                            new JsonObject()
                                .Add("extraField", 42)
                                .Add("references",
                                    JsonValue.Array(1, 2))
                                .Add("assets1",
                                    JsonValue.Array(1))
                                .Add("array",
                                    JsonValue.Array(
                                        new JsonObject()
                                            .Add("extraField", 42)
                                            .Add("nested", JsonValue.Array(1, 2))))
                                .Add(Component.Discriminator, DomainId.Empty)))
                .AddField("components",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                new JsonObject()
                                    .Add("extraField", 42)
                                    .Add("references",
                                        JsonValue.Array(1, 2))
                                    .Add("assets1",
                                        JsonValue.Array(1))
                                    .Add("array",
                                        JsonValue.Array(
                                            new JsonObject()
                                                .Add("extraField", 42)
                                                .Add("nested", JsonValue.Array(1, 2))))
                                    .Add(Component.Discriminator, DomainId.Empty))));

        Assert.Equal(expected, actual);
    }

    private sealed class ItemConverter : IContentItemConverter
    {
        public JsonObject ConvertItem(IField field, JsonObject source)
        {
            source["extraField"] = 42;

            return source;
        }
    }

    private sealed class ValueConverter : IContentValueConverter
    {
        public (bool Remove, JsonValue) ConvertValue(IField field, JsonValue source, IField? parent)
        {
            var remove = field.Name != "assets1";

            return (remove, source);
        }
    }
}
