// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities;

public class AppProviderExtensionsTests : GivenContext
{
    private readonly NamedId<DomainId> componentId1 = NamedId.Of(DomainId.NewGuid(), "my-schema");
    private readonly NamedId<DomainId> componentId2 = NamedId.Of(DomainId.NewGuid(), "my-schema");

    [Fact]
    public async Task Should_do_nothing_if_no_component_found()
    {
        var components = await AppProvider.GetComponentsAsync(Schema, ct: CancellationToken);

        Assert.Empty(components);

        A.CallTo(() => AppProvider.GetSchemaAsync(A<DomainId>._, A<DomainId>._, false, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_resolve_self_as_component()
    {
        Schema = Schema
            .AddComponent(1, "1", Partitioning.Invariant, new ComponentFieldProperties
            {
                SchemaId = SchemaId.Id
            });

        var components = await AppProvider.GetComponentsAsync(Schema, ct: CancellationToken);

        Assert.Single(components);
        Assert.Same(Schema, components[Schema.Id]);

        A.CallTo(() => AppProvider.GetSchemaAsync(A<DomainId>._, A<DomainId>._, false, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_resolve_from_component()
    {
        var component = new Schema { Id = componentId1.Id, Name = componentId1.Name };

        Schema = Schema
            .AddComponent(1, "1", Partitioning.Invariant, new ComponentFieldProperties
            {
                SchemaId = componentId1.Id
            });

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, componentId1.Id, false, CancellationToken))
            .Returns(component);

        var components = await AppProvider.GetComponentsAsync(Schema, ct: CancellationToken);

        Assert.Single(components);
        Assert.Same(component, components[componentId1.Id]);
    }

    [Fact]
    public async Task Should_resolve_from_components()
    {
        var component = new Schema { Id = componentId1.Id, Name = componentId1.Name };

        Schema = Schema
            .AddComponents(1, "1", Partitioning.Invariant, new ComponentsFieldProperties
            {
                SchemaId = componentId1.Id
            });

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, componentId1.Id, false, CancellationToken))
            .Returns(component);

        var components = await AppProvider.GetComponentsAsync(Schema, ct: CancellationToken);

        Assert.Single(components);
        Assert.Same(component, components[componentId1.Id]);
    }

    [Fact]
    public async Task Should_resolve_from_array()
    {
        var component = new Schema { Id = componentId1.Id, Name = componentId1.Name };

        Schema = Schema
            .AddArray(1, "1", Partitioning.Invariant, a => a
                .AddComponent(2, "2", new ComponentFieldProperties
                {
                    SchemaId = componentId1.Id
                }));

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, componentId1.Id, false, CancellationToken))
            .Returns(component);

        var components = await AppProvider.GetComponentsAsync(Schema, ct: CancellationToken);

        Assert.Single(components);
        Assert.Same(component, components[componentId1.Id]);
    }

    [Fact]
    public async Task Should_resolve_self_referencing_component()
    {
        var component = new Schema { Id = componentId1.Id, Name = componentId1.Name }
            .AddComponent(1, "1", Partitioning.Invariant, new ComponentFieldProperties
            {
                SchemaId = componentId1.Id
            });

        Schema = Schema
            .AddComponent(1, "1", Partitioning.Invariant, new ComponentFieldProperties
            {
                SchemaId = componentId1.Id
            });

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, componentId1.Id, false, CancellationToken))
            .Returns(component);

        var components = await AppProvider.GetComponentsAsync(Schema, ct: CancellationToken);

        Assert.Single(components);
        Assert.Same(component, components[componentId1.Id]);
    }

    [Fact]
    public async Task Should_resolve_component_of_component()
    {
        var component1 = new Schema { Id = componentId1.Id, Name = componentId1.Name }
            .AddComponent(1, "1", Partitioning.Invariant, new ComponentFieldProperties
            {
                SchemaId = componentId2.Id
            });

        var component2 = new Schema { Id = componentId2.Id, Name = componentId2.Name }
            .AddComponent(1, "1", Partitioning.Invariant, new ComponentFieldProperties
            {
                SchemaId = componentId1.Id
            });

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, componentId1.Id, false, CancellationToken))
            .Returns(component1);

        A.CallTo(() => AppProvider.GetSchemaAsync(AppId.Id, componentId2.Id, false, CancellationToken))
            .Returns(component2);

        Schema = Schema
            .AddComponent(1, "1", Partitioning.Invariant, new ComponentFieldProperties
            {
                SchemaId = componentId1.Id
            });

        var components = await AppProvider.GetComponentsAsync(Schema, ct: CancellationToken);

        Assert.Equal(2, components.Count);
        Assert.Same(component1, components[componentId1.Id]);
        Assert.Same(component2, components[componentId2.Id]);
    }
}
