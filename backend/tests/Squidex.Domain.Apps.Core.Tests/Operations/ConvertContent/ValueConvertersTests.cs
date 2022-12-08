// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.ConvertContent;

public class ValueConvertersTests
{
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly DomainId id1 = DomainId.NewGuid();
    private readonly DomainId id2 = DomainId.NewGuid();

    private readonly RootField<StringFieldProperties> stringField
        = Fields.String(1, "1", Partitioning.Invariant);

    private readonly RootField<NumberFieldProperties> numberField
        = Fields.Number(1, "1", Partitioning.Invariant);

    public ValueConvertersTests()
    {
        A.CallTo(() => urlGenerator.AssetContent(appId, A<string>._))
            .ReturnsLazily(ctx => $"url/to/{ctx.GetArgument<string>(1)}");
    }

    [Fact]
    public void Should_return_true_if_field_hidden()
    {
        var source = JsonValue.Create(123);

        var (remove, _) =
            ExcludeHidden.Instance
                .ConvertValue(stringField.Hide(), source, null);

        Assert.True(remove);
    }

    [Fact]
    public void Should_return_true_if_field_has_wrong_type()
    {
        var source = JsonValue.Create("invalid");

        var (remove, _) =
            new ExcludeChangedTypes(TestUtils.DefaultSerializer)
                .ConvertValue(numberField, source, numberField);

        Assert.True(remove);
    }

    [Theory]
    [InlineData("assets")]
    [InlineData("*")]
    public void Should_convert_asset_ids_to_urls(string path)
    {
        var field = Fields.Assets(1, "assets", Partitioning.Invariant);

        var source =
            JsonValue.Array(
                id1,
                id2);

        var (_, actual) =
            new ResolveAssetUrls(appId, urlGenerator, HashSet.Of(path))
                .ConvertValue(field, source, null);

        var expected =
            JsonValue.Array(
                $"url/to/{id1}",
                $"url/to/{id2}");

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("other")]
    [InlineData("**")]
    public void Should_not_convert_asset_ids_if_field_name_does_not_match(string path)
    {
        var field = Fields.Assets(1, "assets", Partitioning.Invariant);

        var source =
            JsonValue.Array(
                id1,
                id2);

        var (_, actual) =
            new ResolveAssetUrls(appId, urlGenerator, HashSet.Of(path))
                .ConvertValue(field, source, null);

        var expected = source;

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("parent.assets")]
    [InlineData("*")]
    public void Should_convert_nested_asset_ids_to_urls(string path)
    {
        var field = Fields.Array(1, "parent", Partitioning.Invariant, null, null, Fields.Assets(11, "assets"));

        var source =
            JsonValue.Array(
                id1,
                id2);

        var (_, actual) =
            new ResolveAssetUrls(appId, urlGenerator, HashSet.Of(path))
                .ConvertValue(field.FieldsByName["assets"], source, field);

        var expected =
            JsonValue.Array(
                $"url/to/{id1}",
                $"url/to/{id2}");

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("assets")]
    [InlineData("parent")]
    [InlineData("parent.other")]
    [InlineData("other.assets")]
    public void Should_not_convert_nested_asset_ids_if_field_name_does_not_match(string path)
    {
        var field = Fields.Array(1, "parent", Partitioning.Invariant, null, null, Fields.Assets(11, "assets"));

        var source =
            JsonValue.Array(
                id1,
                id2);

        var (_, actual) =
            new ResolveAssetUrls(appId, urlGenerator, HashSet.Of(path))
                .ConvertValue(field, source, null);

        var expected = source;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_add_schema_name_if_component()
    {
        var field = Fields.Component(1, "component", Partitioning.Invariant);

        var componentId = DomainId.NewGuid();
        var component = new Schema("my-component");
        var components = new ResolvedComponents(new Dictionary<DomainId, Schema>
        {
            [componentId] = component
        });

        var source =
            new JsonObject()
                .Add(Component.Discriminator, componentId);

        var actual =
            new AddSchemaNames(components)
                .ConvertItem(field, source);

        var expected =
            new JsonObject()
                .Add(Component.Discriminator, componentId)
                .Add("schemaName", component.Name);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_not_add_schema_name_if_field_already_exists()
    {
        var field = Fields.Component(1, "component", Partitioning.Invariant);

        var componentId = DomainId.NewGuid();
        var component = new Schema("my-component");
        var components = new ResolvedComponents(new Dictionary<DomainId, Schema>
        {
            [componentId] = component
        });

        var source =
            new JsonObject()
                .Add(Component.Discriminator, componentId)
                .Add("schemaName", "existing");

        var actual =
            new AddSchemaNames(components)
                .ConvertItem(field, source);

        var expected = source;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_not_add_schema_name_if_array_field()
    {
        var field = Fields.Array(1, "component", Partitioning.Invariant);

        var componentId = DomainId.NewGuid();
        var component = new Schema("my-component");
        var components = new ResolvedComponents(new Dictionary<DomainId, Schema>
        {
            [componentId] = component
        });

        var source =
            new JsonObject()
                .Add(Component.Discriminator, componentId);

        var actual =
            new AddSchemaNames(components)
                .ConvertItem(field, source);

        var expected = source;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_not_add_schema_name_if_not_a_component()
    {
        var field = Fields.Component(1, "component", Partitioning.Invariant);

        var componentId = DomainId.NewGuid();
        var component = new Schema("my-component");
        var components = new ResolvedComponents(new Dictionary<DomainId, Schema>
        {
            [componentId] = component
        });

        var source =
            new JsonObject();

        var actual =
            new AddSchemaNames(components)
                .ConvertItem(field, source);

        var expected = source;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_not_add_schema_name_if_component_not_found()
    {
        var field = Fields.Component(1, "component", Partitioning.Invariant);

        var componentId = DomainId.NewGuid();

        var source =
            new JsonObject()
                .Add(Component.Discriminator, componentId);

        var actual =
            new AddSchemaNames(ResolvedComponents.Empty)
                .ConvertItem(field, source);

        var expected = source;

        Assert.Equal(expected, actual);
    }
}
