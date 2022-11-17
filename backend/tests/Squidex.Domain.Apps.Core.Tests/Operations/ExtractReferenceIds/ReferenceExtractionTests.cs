// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ExtractReferenceIds;

public class ReferenceExtractionTests
{
    private readonly Schema schema;
    private readonly ResolvedComponents components;

    public ReferenceExtractionTests()
    {
        schema =
            new Schema("my-schema")
                .AddComponent(1, "component", Partitioning.Invariant)
                .AddComponents(2, "components", Partitioning.Invariant)
                .AddAssets(3, "assets1", Partitioning.Invariant)
                .AddAssets(4, "assets2", Partitioning.Invariant)
                .AddReferences(5, "references", Partitioning.Invariant)
                .AddArray(6, "array", Partitioning.Invariant, a => a
                    .AddAssets(31, "nestedAssets")
                    .AddComponent(32, "nestedComponent")
                    .AddComponents(33, "nestedComponents"));

        components = new ResolvedComponents(new Dictionary<DomainId, Schema>
        {
            [DomainId.Empty] = schema
        });
    }

    [Fact]
    public void Should_get_ids_from_name_data()
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var input =
            new ContentData()
                .AddField("assets1",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(id1.ToString(), id2.ToString())));

        var ids = new HashSet<DomainId>();

        input.AddReferencedIds(schema, ids, components);

        Assert.Equal(new[] { id1, id2 }, ids);
    }

    [Fact]
    public void Should_get_limited_ids_from_name_data()
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var input =
            new ContentData()
                .AddField("assets1",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(id1.ToString(), id2.ToString())));

        var ids = new HashSet<DomainId>();

        input.AddReferencedIds(schema, ids, components, 1);

        Assert.Equal(new[] { id1 }, ids);
    }

    [Fact]
    public void Should_cleanup_deleted_ids()
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var source =
            new ContentData()
                .AddField("references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(id1, id2)))
                .AddField("assets1",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(id1)))
                .AddField("array",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                new JsonObject()
                                    .Add("nestedAssets", JsonValue.Array(id1, id2))
                                    .Add("nestedComponent",
                                        new JsonObject()
                                            .Add("references",
                                                JsonValue.Array(id1, id2))
                                            .Add(Component.Discriminator, DomainId.Empty))
                                    .Add("nestedComponents",
                                        JsonValue.Array(
                                            new JsonObject()
                                                .Add("references",
                                                    JsonValue.Array(id1, id2))
                                                .Add(Component.Discriminator, DomainId.Empty))))))
                .AddField("component",
                    new ContentFieldData()
                        .AddInvariant(
                            new JsonObject()
                                .Add("references",
                                    JsonValue.Array(id1, id2))
                                .Add("assets1",
                                    JsonValue.Array(id1))
                                .Add("array",
                                    JsonValue.Array(
                                        new JsonObject()
                                            .Add("nestedAssets", JsonValue.Array(id1, id2))))
                                .Add(Component.Discriminator, DomainId.Empty)))
                .AddField("components",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                new JsonObject()
                                    .Add("references",
                                        JsonValue.Array(id1, id2))
                                    .Add("assets1",
                                        JsonValue.Array(id1))
                                    .Add("array",
                                        JsonValue.Array(
                                            new JsonObject()
                                                .Add("nestedAssets", JsonValue.Array(id1, id2))))
                                    .Add(Component.Discriminator, DomainId.Empty))));

        var expected =
            new ContentData()
                .AddField("references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(id2)))
                .AddField("assets1",
                    new ContentFieldData()
                        .AddInvariant(new JsonArray()))
                .AddField("array",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                new JsonObject()
                                    .Add("nestedAssets", JsonValue.Array(id2))
                                    .Add("nestedComponent",
                                        new JsonObject()
                                            .Add("references",
                                                JsonValue.Array(id2))
                                            .Add(Component.Discriminator, DomainId.Empty))
                                    .Add("nestedComponents",
                                        JsonValue.Array(
                                            new JsonObject()
                                                .Add("references",
                                                    JsonValue.Array(id2))
                                                .Add(Component.Discriminator, DomainId.Empty))))))
                .AddField("component",
                    new ContentFieldData()
                        .AddInvariant(
                            new JsonObject()
                                .Add("references",
                                    JsonValue.Array(id2))
                                .Add("assets1",
                                    new JsonArray())
                                .Add("array",
                                    JsonValue.Array(
                                        new JsonObject()
                                            .Add("nestedAssets", JsonValue.Array(id2))))
                                .Add(Component.Discriminator, DomainId.Empty)))
                .AddField("components",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                new JsonObject()
                                    .Add("references",
                                        JsonValue.Array(id2))
                                    .Add("assets1",
                                        new JsonArray())
                                    .Add("array",
                                        JsonValue.Array(
                                            new JsonObject()
                                                .Add("nestedAssets", JsonValue.Array(id2))))
                                    .Add(Component.Discriminator, DomainId.Empty))));

        var actual =
            new ContentConverter(components, schema)
                .Add(new ValueReferencesConverter(new HashSet<DomainId> { id2 }))
                .Convert(source);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_return_empty_list_from_non_references_field()
    {
        var sut = Fields.String(1, "myString", Partitioning.Invariant);

        var actual = sut.GetReferencedIds(JsonValue.Create("invalid"), components).ToArray();

        Assert.Empty(actual);
    }

    [Theory]
    [MemberData(nameof(ReferencingNestedFields))]
    public void Should_return_ids_from_nested_field(NestedField field)
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var arrayField = Fields.Array(1, "myArray", Partitioning.Invariant, null, null, field);

        var value =
            JsonValue.Array(
                new JsonObject()
                    .Add(field.Name, CreateValue(id1, id2)));

        var actual = arrayField.GetReferencedIds(value, components).ToArray();

        Assert.Equal(new[] { id1, id2 }, actual);
    }

    [Theory]
    [MemberData(nameof(ReferencingFields))]
    public void Should_return_empty_list_from_field_if_value_item_is_invalid(IField field)
    {
        var actual = field.GetReferencedIds(JsonValue.Array(1), components).ToArray();

        Assert.Empty(actual);
    }

    [Theory]
    [MemberData(nameof(ReferencingFields))]
    public void Should_return_empty_list_from_field_if_value_is_empty(IField field)
    {
        var actual = field.GetReferencedIds(new JsonArray(), components).ToArray();

        Assert.Empty(actual);
    }

    [Theory]
    [MemberData(nameof(ReferencingFields))]
    public void Should_return_empty_list_from_field_if_value_is_json_null(IField field)
    {
        var actual = field.GetReferencedIds(default, components).ToArray();

        Assert.Empty(actual);
    }

    [Theory]
    [MemberData(nameof(ReferencingFields))]
    public void Should_return_empty_list_from_field_if_value_is_null(IField field)
    {
        var actual = field.GetReferencedIds(default, components).ToArray();

        Assert.Empty(actual);
    }

    [Theory]
    [MemberData(nameof(ReferencingFields))]
    public void Should_return_ids_from_field(IField field)
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var value = CreateValue(id1, id2);

        var actual = field.GetReferencedIds(value, components);

        Assert.Equal(new HashSet<DomainId> { id1, id2 }, actual);
    }

    [Theory]
    [MemberData(nameof(ReferencingFields))]
    public void Should_return_same_value_from_field_if_value_is_json_null(IField field)
    {
        var (_, actual) = new ValueReferencesConverter(RandomIds()).ConvertValue(field, JsonValue.Null, null);

        Assert.Equal(JsonValue.Null, actual);
    }

    [Theory]
    [MemberData(nameof(ReferencingFields))]
    public void Should_remove_deleted_ids_from_field(IField field)
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var value = CreateValue(id1, id2);

        var (_, actual) = new ValueReferencesConverter(HashSet.Of(id1)).ConvertValue(field, value, null);

        Assert.Equal(CreateValue(id1), actual);
    }

    public static IEnumerable<object[]> ReferencingNestedFields()
    {
        yield return new object[] { Fields.References(1, "myRefs") };
        yield return new object[] { Fields.Assets(1, "myAssets") };
    }

    public static IEnumerable<object[]> ReferencingFields()
    {
        yield return new object[] { Fields.References(1, "myRefs", Partitioning.Invariant) };
        yield return new object[] { Fields.Assets(1, "myAssets", Partitioning.Invariant) };
    }

    private static HashSet<DomainId> RandomIds()
    {
        return HashSet.Of(DomainId.NewGuid());
    }

    private static JsonValue CreateValue(params object[] ids)
    {
        return JsonValue.Array(ids);
    }
}
