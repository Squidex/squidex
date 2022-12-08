// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.States;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.Commands;

public class DefaultDomainObjectFactoryTests
{
    private record TestObject(DomainId Id, IPersistenceFactory<int> PersistenceFactory);

    [Fact]
    public void Should_create_with_service_locator()
    {
        var id = DomainId.NewGuid();

        var persistenceFactory = A.Fake<IPersistenceFactory<int>>();

        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(persistenceFactory)
                .BuildServiceProvider();

        var sut = new DefaultDomainObjectFactory(serviceProvider);

        var created = sut.Create<TestObject>(id);

        Assert.Equal(id, created.Id);
        Assert.Same(persistenceFactory, created.PersistenceFactory);
    }

    [Fact]
    public void Should_create_with_service_locator_and_custom_persistence_factory()
    {
        var id = DomainId.NewGuid();

        var persistenceFactory = A.Fake<IPersistenceFactory<int>>();

        var serviceProvider =
            new ServiceCollection()
                .BuildServiceProvider();

        var sut = new DefaultDomainObjectFactory(serviceProvider);

        var created = sut.Create<TestObject, int>(id, persistenceFactory);

        Assert.Equal(id, created.Id);
        Assert.Same(persistenceFactory, created.PersistenceFactory);
    }
}
